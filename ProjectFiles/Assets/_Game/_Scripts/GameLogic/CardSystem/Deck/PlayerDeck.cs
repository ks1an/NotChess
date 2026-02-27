using DG.Tweening;
using UnityEngine;

public sealed class PlayerDeck : Deck
{
    public PlayerCardHand hand;
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
        cardsInDeck = new();
        for (int i = 0; i < cardCollectionFromSave.CardsInCollection.Count; i++)
            if (i < maxDeckSize)
                cardsInDeck.Add(cardCollectionFromSave.CardsInCollection[i]);

        if (cardsInDeck.Count < maxDeckSize)
        {
            int j = Random.Range(0, cardCollectionFromSave.CardsInCollection.Count);
            cardsInDeck.Add(cardCollectionFromSave.CardsInCollection[j]);
            for (int i = cardsInDeck.Count + 1; i < maxDeckSize; i++)
            {
                if (j == cardCollectionFromSave.CardsInCollection.Count - 1)
                    j = 0;
                else
                    j++;

                cardsInDeck.Add(cardCollectionFromSave.CardsInCollection[j]);
            }
        }

        DestroyAllCard();
        AddToDeckView(maxDeckSize, true);
        isNet = GameController.Instance.states.isNetMatch;
        if (isNet)
        {
            netCard = GameController.Instance.netMatch.cardSync;
            netCard.Enemy_SetDefaultRpc();
        }
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
            if (curDeckSize == maxDeckSize) break;
            curDeckSize++;
            cardAdded++;
        }
        deckView.Add(cardCollectionFromSave.cardBack, cardAdded);

        if (isNet && !isDefSet)
            netCard.Enemy_AddToDeckRpc(count);
    }
    public void AddToGraveyard(int count)
    {
        if (count < 0)
        {
            Debug.LogError("Trying add to player graveyard negative count of cards");
            return;
        }
        gravejardView.Add(cardCollectionFromSave.cardBack, count);
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

        newCard.Init();
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
        for (int i = 0; i < amount; i++)
        {
            if (curDeckSize == 0) break;
            if (!ignoreCardLimit && hand.CardsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) break;

            Card card = GetRandomCard();
            cardsInDeck.Remove(card);
            DrawCardInHand(card);
            deckView.Remove();

            curDeckSize--;
            cardSpawnedCount++;
        }

        banForDrawLastIssuedCard = false;
        banForDrawLastGraveyardCard = false;
        if (netCard)
            netCard.Enemy_DrawHandRandomFromDeckRpc(cardSpawnedCount, ignoreCardLimit);
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
            gravejardView.Add(cardCollectionFromSave.cardBack);
        }

        card.transform.DOComplete();
        Destroy(card.gameObject);
        hand.RemoveCard(card);

        if (isNet)
            netCard.Enemy_DestroyCardRpc(needToGravejard);
    }

    public void DestroyAllCardsInHand(bool needToAddInGraveyard = true)
    {
        if (needToAddInGraveyard)
            gravejardView.Add(cardCollectionFromSave.cardBack, hand.CardsInHand.Count);
        hand.RemoveAllCards();

        if (isNet)
            netCard.Enemy_DestroyAllCardsInHandRpc(needToAddInGraveyard);
    }

    public override int GetGraveyardCardCount() { return gravejardView.GetCountInStack(); }
}
