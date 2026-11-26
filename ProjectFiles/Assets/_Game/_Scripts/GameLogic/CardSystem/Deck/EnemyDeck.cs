using DG.Tweening;
using UnityEngine;

public sealed class EnemyDeck : Deck
{
    public EnemyCardHand hand;
    public static EnemyDeck Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            lastGraveyardCardID = -1;
        }
        else
        {
            Destroy(gameObject);  
        }
    }

    // ===== Setup =====

    public override void SetDefaultSettings()
    {
        DeckSize = GameController.Instance.matchSettings.maxDeck;

        DestroyAllCard();

        if (!GameController.Instance.states.isNetMatch)
        {
            PopulateRuntimeDeck();
            if (cardsInDeck.Count > 0)
                MathOperations.GetInstance().ShuffleList(cardsInDeck);
        }

        AddToDeckView(cardsInDeck.Count, isDefSet: true);
    }

    void PopulateRuntimeDeck()
    {
        cardsInDeck.Clear();

        if (_deckService == null || _deckService.Count == 0)
        {
            Debug.LogError("EnemyDeck: DeckService is not bound or empty", this);
            return;
        }

        foreach (var card in _deckService.Cards)
        {
            if (cardsInDeck.Count >= maxDeckSize) break;
            if (card != null) cardsInDeck.Add(card);
        }

        int unique = cardsInDeck.Count;
        if (unique > 0)
        {
            int i = 0;
            while (cardsInDeck.Count < maxDeckSize)
            {
                cardsInDeck.Add(cardsInDeck[i % unique]);
                i++;
            }
        }
    }

    // ===== Net-sync =====

    public void AddCardsToDeck(int[] cardsId, bool needShuffle = true)
    {
        if (cardsId == null) return;

        int added = 0;
        foreach (int cardId in cardsId)
        {
            if (GameController.Instance.globalCards.GlobalCardsDictionary.TryGetValue(cardId, out var card) && card != null)
            {
                cardsInDeck.Add(card);
                added++;
            }
            else
            {
                Debug.LogWarning($"EnemyDeck: unknown card id {cardId} skipped");
            }
        }

        AddToDeckView(added);

        if (needShuffle)
            MathOperations.GetInstance().ShuffleList(cardsInDeck);
    }

    // ===== View =====

    public override void AddToDeckView(int count, bool isDefSet = false)
    {
        if (count < 0)
        {
            Debug.LogError("Trying add to enemy deck negative count of cards");
            return;
        }

        var back = _deckService?.CardBack;
        if (back == null)
        {
            Debug.LogError("EnemyDeck: card back is null — set it via DeckService", this);
            return;
        }

        deckView.Add(back.gameObject, count);
    }

    public void AddToGraveyardMirror(int count)
    {
        if (count < 0)
        {
            Debug.LogError("Trying add to enemy graveyard negative count of cards");
            return;
        }

        var back = _deckService?.CardBack;
        if (back == null)
        {
            Debug.LogError("EnemyDeck: card back is null", this);
            return;
        }

        gravejardView.Add(back.gameObject, count);
    }

    // ===== Draw =====

    public void DrawCardInHand(Card card)
    {
        if (card == null)
        {
            Debug.LogError("EnemyDeck: trying to draw null card");
            return;
        }

        var back = _deckService != null ? _deckService.CardBack : null;
        if (back == null)
        {
            Debug.LogError("EnemyDeck: card back is null", this);
            return;
        }

        GameObject newCard = Instantiate(back.gameObject, parent: hand.transform);
        Vector3 targetScale = back.transform.localScale;
        newCard.transform.localScale = Vector3.zero;
        newCard.transform.DOScale(targetScale, 0.1f);
        hand.AddCard(newCard, card);
    }

    public void DrawHandFromDeck(int[] cardIDs, bool ignoreCardLimit = false)
    {
        if (cardIDs == null || cardIDs.Length <= 0)
        {
            Debug.LogError("Amount is negative or zero!");
            return;
        }
        if ((!ignoreCardLimit && hand.CardGameobjectsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) || CurDeckSize <= 0)
        {
            Debug.LogError($"Return draw. Info: {ignoreCardLimit}, {hand.CardGameobjectsInHand.Count}, {CurDeckSize}");
            return;
        }

        for (int i = 0; i < cardIDs.Length; i++)
        {
            if (!GameController.Instance.globalCards.GlobalCardsDictionary.TryGetValue(cardIDs[i], out var card) || card == null)
            {
                Debug.LogWarning($"EnemyDeck: unknown card id {cardIDs[i]} on draw");
                continue;
            }

            DrawCardInHand(card);
            deckView.Remove();

            if (CurDeckSize == 0) break;
            if (!ignoreCardLimit && hand.CardGameobjectsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) break;
        }
    }

    public override void DrawHandRandomFromDeck(int amount, bool ignoreCardLimit = false)
    {
        if (amount < 0)
        {
            Debug.LogError("Amount is negative!");
            return;
        }
        if ((!ignoreCardLimit && hand.CardGameobjectsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) || CurDeckSize <= 0)
            return;

        if (banForDrawLastGraveyardCard && Random.Range(0, 100) > (100 - chanceToSkipRestrictOnGetLastDestroyedCard))
            banForDrawLastGraveyardCard = false;

        for (int i = 0; i < amount; i++)
        {
            if (CurDeckSize == 0) break;
            if (!ignoreCardLimit && hand.CardGameobjectsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) break;

            Card card = GetRandomCard();
            if (card == null) break;

            cardsInDeck.Remove(card);
            DrawCardInHand(card);
            deckView.Remove();
        }

        banForDrawLastIssuedCard = false;
        banForDrawLastGraveyardCard = false;
    }

    public override void DrawLastFromGraveyard()
    {
        if (gravejardView.GetCountInStack() < 1) return;
        gravejardView.Remove();

        if (!GameController.Instance.globalCards.GlobalCardsDictionary.TryGetValue(lastGraveyardCardID, out var c) || c == null)
        {
            Debug.LogWarning($"EnemyDeck: graveyard card id {lastGraveyardCardID} is unknown");
            return;
        }
        DrawCardInHand(c);
    }

    // ===== Destroy =====

    public override void DestroyAllCard()
    {
        hand.RemoveAllCards();
        deckView.RemoveAll();
        gravejardView.RemoveAll();
        cardsInDeck.Clear();     
        lastGraveyardCardID = -1;
    }

    public void DestroyCardInHand(GameObject card, bool needGravejard = true)
    {
        if (card == null) return;
        hand.cardsInHand.TryGetValue(card, out Card c);

        if (c != null) lastGraveyardCardID = c.ID;
        else Debug.LogWarning("EnemyDeck: card not found in hand map");

        hand.RemoveCard(card);
        if (needGravejard)
            gravejardView.Add(card.gameObject);
    }

    public void DestroyAllCardsIn(bool needToAddInGraveyard = true)
    {
        if (needToAddInGraveyard)
        {
            var back = _deckService != null ? _deckService.CardBack : null;
            if (back != null)
                gravejardView.Add(back.gameObject, hand.CardGameobjectsInHand.Count);
        }
        hand.RemoveAllCards();
    }
}