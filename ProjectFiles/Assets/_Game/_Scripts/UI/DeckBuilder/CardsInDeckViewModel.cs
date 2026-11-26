using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class CardsInDeckViewModel : MonoBehaviour
{
    [SerializeField] CardInDeckView cardTemplate;
    [SerializeField] Transform container;
    [SerializeField] TextMeshProUGUI avgManaText, avgBonesText, countInDeck;
    [SerializeField] CardsView cardsView;

    readonly List<CardInDeckView> _rows = new();
    DeckService _draft;
    DeckService _subscribedTo;
    DeckData _editingDeckData;

    public DeckService CurrentDeck
    {
        get
        {
            if (_draft != null) return _draft;
            var p = PlayerRef;
            return p != null ? p.UniqueDeck : null;
        }
    }

    public event Action DeckChanged;

    Player PlayerRef
    {
        get
        {
            var gc = GameController.Instance;
            return gc != null ? gc.player : null;
        }
    }

    // ===== Lifecycle (сессия редактирования) =====

    void OnEnable()
    {
        if (cardTemplate != null) cardTemplate.gameObject.SetActive(false);

        if (_editingDeckData != null)
        {
            Subscribe(_draft);
            RebuildRows();
            UpdateStatsUI();
            DeckChanged?.Invoke();
            return;
        }

        var gc = GameController.Instance;
        var global = gc != null ? gc.globalCards : null;
        var p = PlayerRef;
        if (global != null && p != null && p.UniqueDeck != null)
        {
            _draft = new DeckService(global, p.UniqueDeck.MaxSize);
            _draft.LoadFromData(p.UniqueDeck.ToData(), fallbackToStandardIfEmpty: false);
        }

        Subscribe(_draft);
        RebuildRows();
        UpdateStatsUI();
        DeckChanged?.Invoke();
    }

    void OnDisable()
    {
        Unsubscribe();
        _draft = null;
        _editingDeckData = null;

        ClearRows();
        DeckChanged?.Invoke();
    }

    public void OpenDeckForEdit(DeckData deckData)
    {
        _editingDeckData = deckData;

        var gc = GameController.Instance;
        var global = gc != null ? gc.globalCards : null;
        var p = PlayerRef;
        int maxSize = (p != null && p.UniqueDeck != null) ? p.UniqueDeck.MaxSize : DeckService.DefaultMaxSize;

        if (global != null && deckData != null)
        {
            _draft = new DeckService(global, maxSize);
            _draft.LoadFromData(deckData, fallbackToStandardIfEmpty: false);
            Subscribe(_draft);
        }

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        RebuildRows();
        UpdateStatsUI();
        DeckChanged?.Invoke();
    }

    void Subscribe(DeckService d)
    {
        if (_subscribedTo == d) return;
        if (_subscribedTo != null) _subscribedTo.Changed -= OnDeckChanged;
        _subscribedTo = d;
        if (_subscribedTo != null) _subscribedTo.Changed += OnDeckChanged;
    }

    void Unsubscribe()
    {
        if (_subscribedTo != null) _subscribedTo.Changed -= OnDeckChanged;
        _subscribedTo = null;
    }

    void OnDeckChanged()
    {
        RebuildRows();
        UpdateStatsUI();
        DeckChanged?.Invoke();
    }

    // ===== Commands =====

    public void AddCard(Card cardData)
    {
        if (cardData == null) return;
        CurrentDeck?.Add(cardData);
    }

    public void RemoveCard(Card cardData)
    {
        if (cardData == null) return;
        CurrentDeck?.Remove(cardData);
    }

    public void SaveDeck()
    {
        if (_draft == null) return;

        var updatedData = _draft.ToData();
        var p = PlayerRef;

        if (_editingDeckData != null)
        {
            updatedData.id = _editingDeckData.id;
            updatedData.isFavorite = _editingDeckData.isFavorite;
            updatedData.lastUsedTicks = _editingDeckData.lastUsedTicks;
            _editingDeckData = updatedData;

            DeckLibraryManager.Instance?.SaveEditedDeck(updatedData);
        }
        else if (p != null && p.UniqueDeck != null)
        {
            p.UniqueDeck.LoadFromData(updatedData, fallbackToStandardIfEmpty: false);
            p.UniqueDeck.Save();
            DeckLibraryManager.Instance.SyncActiveDeckToLibrary(saveToDisk: true, notify: true);
        }
    }

    public void CancelChanges()
    {
        var gc = GameController.Instance;
        var global = gc != null ? gc.globalCards : null;
        var p = PlayerRef;
        int maxSize = (p != null && p.UniqueDeck != null) ? p.UniqueDeck.MaxSize : DeckService.DefaultMaxSize;
        if (global == null) return;

        DeckData sourceData = _editingDeckData ?? p.UniqueDeck?.ToData();
        if (sourceData == null) return;

        _draft = new DeckService(global, maxSize);
        _draft.LoadFromData(sourceData, fallbackToStandardIfEmpty: false);

        Subscribe(_draft);
        RebuildRows();
        UpdateStatsUI();
        DeckChanged?.Invoke();
    }

    // ===== View =====

    void RebuildRows()
    {
        ClearRows();
        var d = CurrentDeck;
        if (d == null || cardTemplate == null) return;

        foreach (var card in d.Cards)
        {
            if (card == null) continue;
            var row = Instantiate(cardTemplate, container);
            row.SetCardData(card, 1);
            row.gameObject.SetActive(true);
            _rows.Add(row);
        }
    }

    void ClearRows()
    {
        foreach (var row in _rows) if (row != null) Destroy(row.gameObject);
        _rows.Clear();
    }

    void UpdateStatsUI()
    {
        var d = CurrentDeck;
        if (d == null)
        {
            if (countInDeck != null) countInDeck.text = "<b>0</b>\ncards in deck";
            if (avgManaText != null) avgManaText.text = "<b><color=#43A5BE>0.0</color></b>\n avg. mana";
            if (avgBonesText != null) avgBonesText.text = "<b><color=red>0.0</color></b>\n avg. bones";
            return;
        }

        var s = d.GetStats();
        if (countInDeck != null)
            countInDeck.text = $"<b>{s.Count}</b> /{d.MaxSize}\n cards in deck";
        if (avgManaText != null)
            avgManaText.text = $"<b><color=#43A5BE>{s.AvgManaAll:F1}</color></b> " +
                               $"(<color=#43A5BE>{s.AvgManaOnManaCards:F1}</color> on {s.ManaCards})\n avg. mana";
        if (avgBonesText != null)
            avgBonesText.text = $"<b><color=red>{s.AvgBonesAll:F1}</color></b> " +
                                $"(<color=red>{s.AvgBonesOnBoneCards:F1}</color> on {s.BoneCards})\n avg. bones";
    }
}