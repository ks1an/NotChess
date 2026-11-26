using System;
using System.Collections.Generic;
using System.Linq;

public sealed class DeckLibraryService
{
    readonly GlobalCardCollection _global;
    readonly int _maxDeckSize;
    readonly List<CardCollection> _starterPresets;

    readonly List<DeckEntryData> _entries = new();
    string _activeDeckId;

    public event Action LibraryChanged;
    public event Action<DeckEntryData> EditRequested;

    public IReadOnlyList<DeckEntryData> Entries => _entries;
    public string ActiveDeckId => _activeDeckId;

    public DeckLibraryService(
        GlobalCardCollection global,
        int maxDeckSize = DeckService.DefaultMaxSize,
        IEnumerable<CardCollection> starterPresets = null)
    {
        _global = global ?? throw new ArgumentNullException(nameof(global));
        _maxDeckSize = maxDeckSize > 0 ? maxDeckSize : DeckService.DefaultMaxSize;
        _starterPresets = starterPresets?.Where(p => p != null).ToList() ?? new List<CardCollection>();
    }

    public void InitializeFromSave(DeckLibraryData savedData, DeckService targetPlayerDeck)
    {
        _entries.Clear();

        if (savedData?.decks != null && savedData.decks.Count > 0)
        {
            _entries.AddRange(savedData.decks.Where(d => d != null && d.deckData != null));
            _activeDeckId = savedData.activeDeckId;
        }
        else
        {
            if (_starterPresets.Count > 0)
            {
                foreach (var preset in _starterPresets)
                {
                    var tempService = preset.BuildService(_global, _maxDeckSize);
                    var data = tempService != null ? tempService.ToData() : new DeckData { collectionName = preset.DisplayName };
                    data.collectionName = preset.DisplayName;
                    _entries.Add(new DeckEntryData
                    {
                        lastUsedTicks = DateTime.UtcNow.Ticks,
                        deckData = data
                    });
                }
            }
            else
            {
                var tempService = new DeckService(_global, _maxDeckSize);
                tempService.ResetToStandard();
                _entries.Add(new DeckEntryData
                {
                    lastUsedTicks = DateTime.UtcNow.Ticks,
                    deckData = tempService.ToData()
                });
            }

            _activeDeckId = _entries[0].id;
        }

        SortEntries();

        var activeEntry = GetOrderedEntries().FirstOrDefault(e => e.id == _activeDeckId)
                          ?? GetOrderedEntries().FirstOrDefault();

        if (activeEntry != null && targetPlayerDeck != null)
        {
            _activeDeckId = activeEntry.id;
            targetPlayerDeck.LoadFromData(activeEntry.deckData);
        }

        LibraryChanged?.Invoke();
    }

    public List<DeckEntryData> GetOrderedEntries()
    {
        return _entries
            .OrderByDescending(e => e.isFavorite)
            .ThenByDescending(e => e.lastUsedTicks)
            .ToList();
    }

    void SortEntries()
    {
        var sorted = GetOrderedEntries();
        _entries.Clear();
        _entries.AddRange(sorted);
    }

    public DeckEntryData CreateNewDeck(string name = null, CardCollection preset = null, bool openEditor = true)
    {
        DeckData data;
        if (preset != null)
        {
            var svc = preset.BuildService(_global, _maxDeckSize);
            data = svc != null ? svc.ToData() : new DeckData();
            data.collectionName = string.IsNullOrWhiteSpace(name) ? preset.DisplayName : name.Trim();
        }
        else
        {
            var svc = new DeckService(_global, _maxDeckSize);
            svc.ResetToStandard();
            data = svc.ToData();
            data.collectionName = string.IsNullOrWhiteSpace(name)
                ? $"Новая колода {_entries.Count + 1}"
                : name.Trim();
        }

        var entry = new DeckEntryData
        {
            isFavorite = false,
            lastUsedTicks = DateTime.UtcNow.Ticks,
            deckData = data
        };

        _entries.Add(entry);
        SortEntries();
        LibraryChanged?.Invoke();

        if (openEditor)
            EditRequested?.Invoke(entry);

        return entry;
    }

    public void SelectDeck(string entryId, DeckService targetPlayerDeck)
    {
        var entry = _entries.FirstOrDefault(e => e.id == entryId);
        if (entry == null) return;

        SyncActiveDeckData(targetPlayerDeck);

        _activeDeckId = entry.id;
        entry.lastUsedTicks = DateTime.UtcNow.Ticks;

        if (targetPlayerDeck != null)
            targetPlayerDeck.LoadFromData(entry.deckData);

        SortEntries();
        LibraryChanged?.Invoke();
    }

    public void ToggleFavorite(string entryId)
    {
        var entry = _entries.FirstOrDefault(e => e.id == entryId);
        if (entry == null) return;

        entry.isFavorite = !entry.isFavorite;
        SortEntries();
        LibraryChanged?.Invoke();
    }

    public void RequestEditDeck(string entryId, DeckService targetPlayerDeck)
    {
        var entry = _entries.FirstOrDefault(e => e.id == entryId);
        if (entry == null) return;

        SelectDeck(entry.id, targetPlayerDeck);
        EditRequested?.Invoke(entry);
    }

    public void SyncActiveDeckData(DeckService activeDeckService)
    {
        if (activeDeckService == null || string.IsNullOrEmpty(_activeDeckId)) return;

        var activeEntry = _entries.FirstOrDefault(e => e.id == _activeDeckId);
        if (activeEntry == null) return;

        activeEntry.deckData = activeDeckService.ToData();
        activeEntry.lastUsedTicks = DateTime.UtcNow.Ticks;
        LibraryChanged?.Invoke();
    }

    public DeckLibraryData ToSaveData(DeckService activeDeckService = null)
    {
        SyncActiveDeckData(activeDeckService);
        return new DeckLibraryData
        {
            activeDeckId = _activeDeckId,
            decks = _entries
        };
    }
}