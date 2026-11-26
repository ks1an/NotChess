using System.Collections.Generic;
using UnityEngine;

public abstract class Deck : MonoBehaviour
{
    [HideInInspector] public List<Card> cardsInDeck = new();

    [SerializeField, Range(0, 100)] protected int chanceToSkipRestrictOnGetLastDestroyedCard;
    [SerializeField, Range(1, 100)] protected int maxDeckSize = 30;
    [SerializeField] protected StackView gravejardView, deckView;

    protected bool banForDrawLastIssuedCard, banForDrawLastGraveyardCard;
    protected int lastIssuedCardID, lastGraveyardCardID;

    protected DeckService _deckService;

    public int DeckSize
    {
        get => maxDeckSize;
        set => maxDeckSize = Mathf.Max(1, value);
    }

    public int CurDeckSize => cardsInDeck.Count;

    public void BindDeckService(DeckService deckService)
    {
        _deckService = deckService;
    }

    protected void PopulateRuntimeDeckFromService()
    {
        cardsInDeck.Clear();
        if (_deckService == null || _deckService.Count == 0) return;

        foreach (Card card in _deckService.Cards)
            cardsInDeck.Add(card);

        FillRuntimeDeckUpToSize();
    }

    protected void FillRuntimeDeckUpToSize()
    {
        if (_deckService == null || _deckService.Count == 0) return;
        var source = _deckService.Cards;
        int i = 0;
        while (cardsInDeck.Count < maxDeckSize)
        {
            cardsInDeck.Add(source[i % source.Count]);
            i++;
        }
    }

    public abstract void SetDefaultSettings();
    public abstract void AddToDeckView(int count, bool isDefSet = false);

    // DRAW
    public abstract void DrawHandRandomFromDeck(int amount, bool ignoreCardLimit = false);
    public abstract void DrawLastFromGraveyard();

    // DESTROY
    public abstract void DestroyAllCard();

    public virtual int GetGraveyardCardCount() => gravejardView != null ? gravejardView.GetCountInStack() : 0;

    protected Card GetRandomCard()
    {
        if (cardsInDeck.Count == 0) return null;

        var pool = new List<Card>(cardsInDeck.Count);
        foreach (var c in cardsInDeck)
        {
            if (c == null) continue;
            int id = c.ID;
            if (banForDrawLastIssuedCard && id == lastIssuedCardID) continue;
            if (banForDrawLastGraveyardCard && id == lastGraveyardCardID) continue;
            pool.Add(c);
        }

        if (pool.Count == 0) pool.AddRange(cardsInDeck);

        var chosen = pool[Random.Range(0, pool.Count)];
        if (chosen == null) return null;

        lastIssuedCardID = chosen.ID;
        return chosen;
    }
}