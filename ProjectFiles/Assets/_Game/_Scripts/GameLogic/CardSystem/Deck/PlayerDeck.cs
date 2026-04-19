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
            Destroy(gameObject);
    }

    public override void SetDefaultSettings()
    {
        maxDeckSize = GameController.Instance.matchSettings.maxDeck;
        hand = PlayerCardHand.Instance;
        cardsInDeck = new();

        for (int i = 0; i < cardCollection.CardsInCollection.Count; i++)
            if (i < maxDeckSize)
                cardsInDeck.Add(cardCollection.CardsInCollection[i]);

        if (cardsInDeck.Count < maxDeckSize)
        {
            int j = Random.Range(0, cardCollection.CardsInCollection.Count);
            cardsInDeck.Add(cardCollection.CardsInCollection[j]);
            for (int i = cardsInDeck.Count; i < maxDeckSize; i++)
            {
                if (j == cardCollection.CardsInCollection.Count - 1)
                    j = 0;
                else
                    j++;

                cardsInDeck.Add(cardCollection.CardsInCollection[j]);
            }
        }
        MathOperations.GetInstance().ShuffleList(cardsInDeck);

        DestroyAllCard();
        AddToDeckView(maxDeckSize, true);
        isNet = GameController.Instance.states.isNetMatch;
        if (isNet)
        {
            netCard = GameController.Instance.netMatch.cardSync;
            netCard.Enemy_SetDefaultRpc();
        }
    }

    public void AddCardsToDeck(Card[] cards, bool needShuffle = true)
    {
        int[] cardIdAdded = new int[cards.Length];
        for(int i = 0; i < cards.Length; i++)
        {
            cardsInDeck.Add(cards[i]);
            cardIdAdded[i] = cards[i].GetID();
        }
        AddToDeckView(cards.Length);
        
        if(needShuffle)
            MathOperations.GetInstance().ShuffleList(cardsInDeck);

        if (isNet)
            netCard.Enemy_AddToDeckRpc(cardIdAdded, needShuffle);
    }

    public override void AddToDeckView(int count, bool isDefSet = false)
    {
        if (count < 0)
        {
            Debug.LogError("Trying add to player deck negative count of cards");
            return;
        }

        int cardAdded = 0;
        for (int i = 0; i < count; i++)
        {
            if (curDeckSize >= maxDeckSize) break;
            curDeckSize++;
            cardAdded++;
        }
        if (curDeckSize != cardsInDeck.Count) Debug.Log(curDeckSize + " // " + cardsInDeck.Count);
        deckView.Add(cardCollection.cardBack, cardAdded);

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
        gravejardView.Add(cardCollection.cardBack, count);
        if (isNet)
            netCard.Enemy_AddToGraveyardRpc(count);
    }

    //====DRAW====
    public void DrawCardInHand(Card card)
    {
        Card newCard = Instantiate(card.gameObject, parent: hand.transform).GetComponent<Card>();
        Vector3 targetScale = card.transform.localScale;
        newCard.transform.localScale = Vector3.zero;
        newCard.transform.DOScale(targetScale, 0.15f);
        hand.AddCard(newCard);

        newCard.Init(GameController.Instance.player.GetLocalPlayerTeam());
    }
    public override void DrawHandRandomFromDeck(int amount, bool ignoreCardLimit = false)
    {
        if (amount < 0)
        {
            Debug.LogError("Amount is negative!");
            return;
        }
        if ((!ignoreCardLimit && hand.CardsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) || curDeckSize <= 0)
            return;

        if (banForDrawLastGraveyardCard && Random.Range(0, 100) > (100 - chanceToSkipRestrictOnGetLastDestroyedCard))
            banForDrawLastGraveyardCard = false;

        int cardSpawnedCount = 0;
        int[] cardsIDs = new int[amount];
        for (int i = 0; i < amount; i++)
        {
            if (curDeckSize == 0) break;
            if (curDeckSize != cardsInDeck.Count) Debug.LogError(curDeckSize + " / " + cardsInDeck.Count);
            if (!ignoreCardLimit && hand.CardsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) break;

            Card card = GetRandomCard();
            curDeckSize--;
            cardsIDs[i] = card.GetID();
            cardsInDeck.Remove(card);
            DrawCardInHand(card);
            deckView.Remove();

            cardSpawnedCount++;
        }
        banForDrawLastIssuedCard = false;
        banForDrawLastGraveyardCard = false;
        if (netCard)
            netCard.Enemy_DrawHandFromDeckRpc(cardsIDs, ignoreCardLimit);
    }
    public override void DrawLastFromGraveyard()
    {
        Card card = null;
        if (gravejardView.GetCountInStack() > 0)
        {
            GameController.Instance.globalCards.GlobalCardsDictionary.TryGetValue(lastGraveyardCardID, out Card c);
            card = c;
        }

        if (card != null)
        {
            gravejardView.Remove();
            DrawCardInHand(card);

            if (isNet)
                netCard.Enemy_DrawLastFromGraveyardRpc();
        }
    }

    //====DESTROY====
    public override void DestroyAllCard()
    {
        hand.RemoveAllCards();
        deckView.RemoveAll();
        gravejardView.RemoveAll();
        curDeckSize = 0;
        lastGraveyardCardID = -1;
        lastIssuedCardID = -1;

        if (isNet)
            netCard.Enemy_DestroyAllRpc();
    }
    public void DestroyCardInHand(Card card, bool needToGravejard = true)
    {
        if (needToGravejard)
        {
            lastGraveyardCardID = card.GetID();
            gravejardView.Add(cardCollection.cardBack);
        }

        card.transform.DOComplete();
        card.KillCard();
        if (isNet)
            netCard.Enemy_DestroyCardRpc(hand.CardsInHand.IndexOf(card), needToGravejard);
        hand.RemoveCard(card);
    }

    public void DestroyAllCardsInHand(bool needToAddInGraveyard = true)
    {
        if (needToAddInGraveyard)
            gravejardView.Add(cardCollection.cardBack, hand.CardsInHand.Count);
        hand.RemoveAllCards();

        if (isNet)
            netCard.Enemy_DestroyAllCardsInHandRpc(needToAddInGraveyard);
    }

    public override int GetGraveyardCardCount() { return gravejardView.GetCountInStack(); }
}
