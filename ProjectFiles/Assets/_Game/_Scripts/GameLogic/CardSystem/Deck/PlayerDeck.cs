using DG.Tweening;
using UnityEngine;

public sealed class PlayerDeck : Deck
{
    PlayerCardHand hand;
    public static PlayerDeck Instance { get; private set; }

    CardSystemSync netCard;
    bool isNet;

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
            lastIssuedCardID = -1;
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
        hand = PlayerCardHand.Instance;

        isNet = false;
        netCard = null;

        DestroyAllCard();              

        PopulateRuntimeDeckFromService(); 
        FillRuntimeDeckUpToSize();     

        if (cardsInDeck.Count > 0)
            MathOperations.GetInstance().ShuffleList(cardsInDeck);

        AddToDeckView(cardsInDeck.Count, isDefSet: true);

        isNet = GameController.Instance.states.isNetMatch;
        if (isNet)
        {
            netCard = GameController.Instance.netMatch.cardSync;
            netCard.Enemy_SetDefaultRpc();
        }
    }

    // ===== Net-sync =====

    public void AddCardsToDeck(Card[] cards, bool needShuffle = true)
    {
        if (cards == null || cards.Length == 0) return;

        var ids = new System.Collections.Generic.List<int>(cards.Length);
        for (int i = 0; i < cards.Length; i++)
        {
            var c = cards[i];
            if (c == null) { Debug.LogWarning("PlayerDeck: null card in AddCardsToDeck"); continue; }
            cardsInDeck.Add(c);
            ids.Add(c.ID);
        }

        int added = ids.Count;
        if (added == 0) return;

        AddToDeckView(added);

        if (needShuffle)
            MathOperations.GetInstance().ShuffleList(cardsInDeck);

        if (isNet)
            netCard.Enemy_AddToDeckRpc(ids.ToArray(), needShuffle);
    }

    // ===== View =====

    public override void AddToDeckView(int count, bool isDefSet = false)
    {
        if (count < 0)
        {
            Debug.LogError("Trying add to player deck negative count of cards");
            return;
        }

        var back = _deckService != null ? _deckService.CardBack : null;
        if (back == null)
        {
            Debug.LogError("PlayerDeck: card back is null — set it via DeckService", this);
            return;
        }

        if (CurDeckSize > maxDeckSize)
            Debug.LogWarning($"PlayerDeck: deck over capacity {CurDeckSize}/{maxDeckSize}");

        deckView.Add(back.gameObject, count);

        if (isNet && !isDefSet)
            netCard.Enemy_AddToDeckViewRpc(count);
    }

    public void AddToGraveyard(int count)
    {
        if (count < 0)
        {
            Debug.LogError("Trying add to player graveyard negative count of cards");
            return;
        }

        var back = _deckService != null ? _deckService.CardBack : null;
        if (back == null) { Debug.LogError("PlayerDeck: card back is null", this); return; }

        gravejardView.Add(back.gameObject, count);

        if (isNet)
            netCard.Enemy_AddToGraveyardRpc(count);
    }

    // ===== DRAW =====

    public void DrawCardInHand(Card card)
    {
        if (card == null)
        {
            Debug.LogError("PlayerDeck: trying to draw null card");
            return;
        }

        Card newCard = Instantiate(card.gameObject, parent: hand.transform).GetComponent<Card>();
        Vector3 targetScale = card.transform.localScale;
        newCard.transform.localScale = Vector3.zero;
        newCard.transform.DOScale(targetScale, 0.15f);
        hand.AddCard(newCard);
        newCard.Init(GameController.Instance.player.GetTeam());
    }

    public override void DrawHandRandomFromDeck(int amount, bool ignoreCardLimit = false)
    {
        if (amount < 0)
        {
            Debug.LogError("Amount is negative!");
            return;
        }
        if ((!ignoreCardLimit && hand.CardsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) || CurDeckSize <= 0)
            return;

        if (banForDrawLastGraveyardCard && Random.Range(0, 100) > (100 - chanceToSkipRestrictOnGetLastDestroyedCard))
            banForDrawLastGraveyardCard = false;

        var ids = new System.Collections.Generic.List<int>(amount);

        for (int i = 0; i < amount; i++)
        {
            if (CurDeckSize == 0) break;
            if (!ignoreCardLimit && hand.CardsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) break;

            Card card = GetRandomCard();
            if (card == null) break;

            cardsInDeck.Remove(card);
            ids.Add(card.ID);
            DrawCardInHand(card);
            deckView.Remove();
        }

        banForDrawLastIssuedCard = false;
        banForDrawLastGraveyardCard = false;

        if (isNet)
            netCard.Enemy_DrawHandFromDeckRpc(ids.ToArray(), ignoreCardLimit);
    }

    public override void DrawLastFromGraveyard()
    {
        if (gravejardView.GetCountInStack() <= 0) return;

        if (!GameController.Instance.globalCards.GlobalCardsDictionary.TryGetValue(lastGraveyardCardID, out var card) || card == null)
        {
            Debug.LogWarning($"PlayerDeck: graveyard card id {lastGraveyardCardID} is unknown");
            return;
        }

        gravejardView.Remove();
        DrawCardInHand(card);

        if (isNet)
            netCard?.Enemy_DrawLastFromGraveyardRpc();
    }

    // ===== DESTROY =====

    public override void DestroyAllCard()
    {
        hand?.RemoveAllCards();
        deckView.RemoveAll();
        gravejardView.RemoveAll();
        cardsInDeck.Clear();
        lastGraveyardCardID = -1;
        lastIssuedCardID = -1;

        if (isNet)
            netCard.Enemy_DestroyAllRpc();
    }

    public void DestroyCardInHand(Card card, bool needToGravejard = true)
    {
        if (card == null) return;

        if (needToGravejard)
        {
            var back = _deckService != null ? _deckService.CardBack : null;
            if (back != null)
            {
                lastGraveyardCardID = card.ID;
                gravejardView.Add(back.gameObject);
            }
            else
            {
                Debug.LogError("PlayerDeck: card back is null, cannot put to graveyard", this);
            }
        }

        int index = hand.CardsInHand.IndexOf(card);
        card.transform.DOComplete();
        card.KillCard();

        if (isNet)
            netCard.Enemy_DestroyCardRpc(index, needToGravejard);

        hand.RemoveCard(card);
    }

    public void DestroyAllCardsInHand(bool needToAddInGraveyard = true)
    {
        if (needToAddInGraveyard)
        {
            var back = _deckService != null ? _deckService.CardBack : null;
            if (back != null)
                gravejardView.Add(back.gameObject, hand.CardsInHand.Count);
            else
                Debug.LogError("PlayerDeck: card back is null", this);
        }

        hand.RemoveAllCards();

        if (isNet)
            netCard.Enemy_DestroyAllCardsInHandRpc(needToAddInGraveyard);
    }

    public override int GetGraveyardCardCount() => gravejardView != null ? gravejardView.GetCountInStack() : 0;
}