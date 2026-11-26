using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class DeckLibrarySaveModel
{
    public string activeDeckId;
    public List<DeckData> decks = new();
}

public sealed class DeckLibraryManager : MonoBehaviour
{
    public static DeckLibraryManager Instance { get; private set; }

    [SerializeField] string fileName = "deck_library.json";
    [SerializeField] List<CardCollection> starterPresets = new();
    [SerializeField] MenuUI menuUI;
    [SerializeField] TabMenu tabMenu;
    [SerializeField] DeckView deckView;
    [SerializeField] CardsInDeckViewModel cardsInDeckViewModel;

    readonly List<DeckData> _decks = new();
    readonly List<string> _sessionOrderIds = new();
    string _activeDeckId;
    bool _initialized;
    bool _loadedFromDisk;

    public event Action LibraryChanged;

    public string ActiveDeckId => _activeDeckId;
    public IReadOnlyList<DeckData> Decks => _decks;

    string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Initialize();
    }

    void OnApplicationQuit()
    {
        SyncActiveDeckToLibrary(saveToDisk: true, notify: false);
    }

    public void Initialize()
    {
        var gc = GameController.Instance;
        var playerDeck = gc != null && gc.player != null ? gc.player.UniqueDeck : null;
        var global = gc != null ? gc.globalCards : null;

        if (!_initialized)
        {
            _loadedFromDisk = LoadLibrary();
            _initialized = true;
        }

        if (_decks.Count == 0 && global != null)
        {
            if (playerDeck != null && playerDeck.Count > 0)
            {
                var existing = playerDeck.ToData();
                if (string.IsNullOrEmpty(existing.id))
                    existing.id = Guid.NewGuid().ToString("N");
                if (existing.lastUsedTicks == 0)
                    existing.lastUsedTicks = DateTime.UtcNow.Ticks;

                _decks.Add(existing);
                _activeDeckId = existing.id;
            }

            if (starterPresets != null && starterPresets.Count > 0)
            {
                int maxSize = playerDeck != null ? playerDeck.MaxSize : DeckService.DefaultMaxSize;
                long baseTicks = DateTime.UtcNow.Ticks;
                int offset = 1;

                foreach (var preset in starterPresets)
                {
                    if (preset == null) continue;
                    var svc = preset.BuildService(global, maxSize);
                    var data = svc != null ? svc.ToData() : new DeckData();
                    data.id = Guid.NewGuid().ToString("N");
                    data.collectionName = preset.DisplayName;
                    data.lastUsedTicks = baseTicks - offset++;
                    _decks.Add(data);
                }
            }

            if (_decks.Count == 0)
            {
                int maxSize = playerDeck != null ? playerDeck.MaxSize : DeckService.DefaultMaxSize;
                var svc = new DeckService(global, maxSize);
                svc.ResetToStandard();
                var data = svc.ToData();
                data.id = Guid.NewGuid().ToString("N");
                data.lastUsedTicks = DateTime.UtcNow.Ticks;
                _decks.Add(data);
            }

            if (string.IsNullOrEmpty(_activeDeckId) && _decks.Count > 0)
                _activeDeckId = _decks[0].id;
        }

        if (_decks.Count > 0)
        {
            var active = _decks.FirstOrDefault(d => d != null && d.id == _activeDeckId) ?? _decks[0];
            _activeDeckId = active.id;

            if (playerDeck != null && (_loadedFromDisk || playerDeck.Count == 0))
                playerDeck.LoadFromData(active, fallbackToStandardIfEmpty: false);

            if (_sessionOrderIds.Count == 0)
                RebuildSessionOrderInternal();

            SaveLibrary();
        }
    }

    public void RefreshSessionOrder()
    {
        Initialize();
        RebuildSessionOrderInternal();
    }

    void RebuildSessionOrderInternal()
    {
        _sessionOrderIds.Clear();
        foreach (var d in _decks
                     .Where(d => d != null)
                     .OrderByDescending(d => d.lastUsedTicks))
        {
            if (!string.IsNullOrEmpty(d.id))
                _sessionOrderIds.Add(d.id);
        }
    }

    public List<DeckData> GetMenuDecks()
    {
        Initialize();

        if (_sessionOrderIds.Count == 0 && _decks.Count > 0)
            RebuildSessionOrderInternal();

        var result = new List<DeckData>();
        foreach (var id in _sessionOrderIds)
        {
            var d = _decks.FirstOrDefault(x => x != null && x.id == id);
            if (d != null)
                result.Add(d);
        }

        foreach (var d in _decks)
        {
            if (d != null && !string.IsNullOrEmpty(d.id) && !_sessionOrderIds.Contains(d.id))
            {
                result.Add(d);
                _sessionOrderIds.Add(d.id);
            }
        }

        return result;
    }

    public DeckData CreateNewDeck(string customName = null, CardCollection preset = null)
    {
        Initialize();

        var gc = GameController.Instance;
        var global = gc != null ? gc.globalCards : null;
        var playerDeck = gc != null && gc.player != null ? gc.player.UniqueDeck : null;
        int maxSize = playerDeck != null ? playerDeck.MaxSize : DeckService.DefaultMaxSize;

        DeckData newData;
        if (preset != null && global != null)
        {
            var svc = preset.BuildService(global, maxSize);
            newData = svc != null ? svc.ToData() : new DeckData();
            newData.collectionName = string.IsNullOrWhiteSpace(customName) ? preset.DisplayName : customName.Trim();
        }
        else if (global != null)
        {
            var svc = new DeckService(global, maxSize);
            svc.ResetToStandard();
            newData = svc.ToData();
            newData.collectionName = string.IsNullOrWhiteSpace(customName)
                ? $"New deck {_decks.Count + 1}"
                : customName.Trim();
        }
        else
        {
            newData = new DeckData
            {
                collectionName = string.IsNullOrWhiteSpace(customName)
                    ? $"New deck {_decks.Count + 1}"
                    : customName.Trim()
            };
        }

        newData.id = Guid.NewGuid().ToString("N");
        newData.isFavorite = false;
        long minTicks = _decks.Count > 0 ? _decks.Min(d => d.lastUsedTicks) : DateTime.UtcNow.Ticks;
        newData.lastUsedTicks = minTicks - 1;

        _decks.Add(newData);
        if (!_sessionOrderIds.Contains(newData.id))
            _sessionOrderIds.Add(newData.id);

        SaveLibrary();
        LibraryChanged?.Invoke();
        return newData;
    }

    public bool DeleteDeck(string deckId)
    {
        Initialize();
        if (string.IsNullOrEmpty(deckId)) return false;

        if (_decks.Count <= 1)
        {
            Debug.LogWarning("[DeckLibraryManager] Cannot delete the last remaining deck.");
            return false;
        }

        var target = _decks.FirstOrDefault(d => d != null && d.id == deckId);
        if (target == null) return false;

        bool wasActive = _activeDeckId == target.id;
        _decks.Remove(target);
        _sessionOrderIds.Remove(target.id);

        if (wasActive)
        {
            var fallback = _decks.OrderByDescending(d => d.lastUsedTicks).FirstOrDefault() ?? _decks.FirstOrDefault();
            _activeDeckId = fallback?.id;

            var playerDeck = GameController.Instance?.player?.UniqueDeck;
            if (fallback != null && playerDeck != null)
                playerDeck.LoadFromData(fallback, fallbackToStandardIfEmpty: false);
        }

        SaveLibrary();
        LibraryChanged?.Invoke();
        return true;
    }

    public void SelectDeck(string deckId)
    {
        Initialize();

        var target = _decks.FirstOrDefault(d => d != null && d.id == deckId);
        if (target == null) return;

        _activeDeckId = target.id;
        target.lastUsedTicks = DateTime.UtcNow.Ticks;

        var playerDeck = GameController.Instance?.player?.UniqueDeck;
        playerDeck?.LoadFromData(target, fallbackToStandardIfEmpty: false);

        SaveLibrary();
        LibraryChanged?.Invoke();
    }

    public void OpenEditor(string deckId)
    {
        Initialize();

        var target = _decks.FirstOrDefault(d => d != null && d.id == deckId);
        if (target == null) return;

        if (menuUI != null)
            menuUI.SetActiveTabMenu(true);

        if (tabMenu != null)
            tabMenu.ActiveDeckPage();

        if (deckView != null)
            deckView.SetOpenDeckLibraryAfterDisable(true);

        if (cardsInDeckViewModel != null)
            cardsInDeckViewModel.OpenDeckForEdit(target);
    }

    public void SaveEditedDeck(DeckData updatedData)
    {
        Initialize();
        if (updatedData == null) return;

        int index = _decks.FindIndex(d => d != null && d.id == updatedData.id);
        if (index < 0) return;

        var existing = _decks[index];
        updatedData.id = existing.id;
        updatedData.isFavorite = existing.isFavorite;
        updatedData.lastUsedTicks = existing.lastUsedTicks;

        _decks[index] = updatedData;

        if (updatedData.id == _activeDeckId)
        {
            var playerDeck = GameController.Instance?.player?.UniqueDeck;
            if (playerDeck != null)
            {
                playerDeck.LoadFromData(updatedData, fallbackToStandardIfEmpty: false);
                playerDeck.Save();
            }
        }

        SaveLibrary();
        LibraryChanged?.Invoke();
    }

    public void SyncActiveDeckToLibrary(bool saveToDisk = true, bool notify = true)
    {
        if (!_initialized || string.IsNullOrEmpty(_activeDeckId)) return;

        var playerDeck = GameController.Instance?.player?.UniqueDeck;
        if (playerDeck == null) return;

        var current = _decks.FirstOrDefault(d => d != null && d.id == _activeDeckId);
        if (current == null) return;

        if (playerDeck.Count == 0 && current.cardIds != null && current.cardIds.Count > 0)
            return;

        var updated = playerDeck.ToData();
        updated.id = current.id;
        updated.isFavorite = current.isFavorite;
        updated.lastUsedTicks = current.lastUsedTicks;

        int index = _decks.IndexOf(current);
        _decks[index] = updated;

        if (saveToDisk)
            SaveLibrary();

        if (notify)
            LibraryChanged?.Invoke();
    }

    public void RenameDeck(string deckId, string newName)
    {
        Initialize();
        if (string.IsNullOrEmpty(deckId) || string.IsNullOrWhiteSpace(newName))
            return;

        var target = _decks.FirstOrDefault(d => d != null && d.id == deckId);
        if (target == null)
            return;

        string trimmed = newName.Trim();
        target.collectionName = trimmed;

        if (target.id == _activeDeckId)
        {
            var playerDeck = GameController.Instance?.player?.UniqueDeck;
            if (playerDeck != null)
            {
                playerDeck.SetCollectionName(trimmed);
                playerDeck.Save();
            }
        }

        SaveLibrary();
        LibraryChanged?.Invoke();
    }

    public void SaveLibrary()
    {
        try
        {
            var model = new DeckLibrarySaveModel
            {
                activeDeckId = _activeDeckId,
                decks = _decks
            };
            string json = JsonUtility.ToJson(model, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save deck library: {ex.Message}");
        }
    }

    bool LoadLibrary()
    {
        _decks.Clear();
        _activeDeckId = null;

        if (!File.Exists(SavePath)) return false;

        try
        {
            string json = File.ReadAllText(SavePath);
            var model = JsonUtility.FromJson<DeckLibrarySaveModel>(json);
            if (model?.decks != null && model.decks.Count > 0)
            {
                foreach (var d in model.decks)
                {
                    if (d == null) continue;
                    if (string.IsNullOrEmpty(d.id)) d.id = Guid.NewGuid().ToString("N");
                    _decks.Add(d);
                }
                _activeDeckId = model.activeDeckId;
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load deck library: {ex.Message}");
        }

        return false;
    }
}