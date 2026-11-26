using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class DeckSelectionMenuView : MonoBehaviour
{
    [SerializeField] Transform contentParent;
    [SerializeField] DeckMenuItemView itemPrefab;
    [SerializeField] Button createDeckButton;
    [SerializeField] CardCollection newDeckPreset;

    readonly List<DeckMenuItemView> _pool = new();

    void Awake()
    {
        if (createDeckButton != null)
            createDeckButton.onClick.AddListener(OnCreateDeckClicked);

        if (itemPrefab != null && itemPrefab.gameObject.scene.IsValid())
            itemPrefab.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        var manager = DeckLibraryManager.Instance;
        if (manager != null)
        {
            manager.LibraryChanged += Rebuild;
            manager.RefreshSessionOrder();
        }

        Rebuild();
    }

    void OnDisable()
    {
        if (DeckLibraryManager.Instance != null)
            DeckLibraryManager.Instance.LibraryChanged -= Rebuild;
    }

    void OnDestroy()
    {
        if (createDeckButton != null)
            createDeckButton.onClick.RemoveListener(OnCreateDeckClicked);
    }

    public void ClosePanel()
    {
        var manager = DeckLibraryManager.Instance;
        if (manager != null)
        {
            manager.SyncActiveDeckToLibrary(saveToDisk: true, notify: false);
            manager.SaveLibrary();
        }

        gameObject.SetActive(false);
    }

    void OnCreateDeckClicked()
    {
        DeckLibraryManager.Instance.CreateNewDeck(preset: newDeckPreset);
    }

    void OnSelectClicked(string deckId)
    {
        DeckLibraryManager.Instance.SelectDeck(deckId);
    }

    void OnEditClicked(string deckId)
    {
        DeckLibraryManager.Instance.OpenEditor(deckId);
        gameObject.SetActive(false);
    }

    void OnDeleteClicked(string deckId)
    {
        DeckLibraryManager.Instance.DeleteDeck(deckId);
    }

    public void Rebuild()
    {
        var manager = DeckLibraryManager.Instance;
        if (manager == null || contentParent == null || itemPrefab == null) return;

        var items = manager.GetMenuDecks();
        var global = GameController.Instance?.globalCards;
        var playerDeck = GameController.Instance?.player?.UniqueDeck;
        int maxSize = playerDeck != null ? playerDeck.MaxSize : DeckService.DefaultMaxSize;

        while (_pool.Count < items.Count)
        {
            var inst = Instantiate(itemPrefab, contentParent);
            _pool.Add(inst);
        }

        for (int i = 0; i < _pool.Count; i++)
        {
            if (i < items.Count)
            {
                var deck = items[i];
                bool isActive = deck.id == manager.ActiveDeckId;

                DeckStats stats = (isActive && playerDeck != null)
                    ? playerDeck.GetStats()
                    : DeckStats.CalculateFromIds(deck.cardIds, global);

                _pool[i].gameObject.SetActive(true);
                _pool[i].Bind(
                    deck,
                    stats,
                    isActive,
                    canDelete: items.Count > 1,
                    maxSize,
                    OnSelectClicked,
                    OnEditClicked,
                    OnDeleteClicked
                );
            }
            else
            {
                _pool[i].gameObject.SetActive(false);
            }
        }

        if (createDeckButton != null)
        {
            if (createDeckButton.transform.parent != contentParent)
                createDeckButton.transform.SetParent(contentParent, false);

            createDeckButton.gameObject.SetActive(true);
            createDeckButton.transform.SetAsLastSibling();
        }

        if (contentParent is RectTransform rect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }
}