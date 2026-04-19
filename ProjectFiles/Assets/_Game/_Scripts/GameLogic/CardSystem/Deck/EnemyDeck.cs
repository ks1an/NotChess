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
            Destroy(Instance);
    }

    public override void SetDefaultSettings()
    {
        maxDeckSize = GameController.Instance.matchSettings.maxDeck;
        if (!GameController.Instance.states.isNetMatch)
        {
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
        }

        DestroyAllCard();
        AddToDeckView(maxDeckSize, true);
    }

    public void AddCardsToDeck(int[] cardsId, bool needShuffle = true)
    {
        foreach(int cardId in cardsId)
        {
            cardsInDeck.Add(GameController.Instance.globalCards.GlobalCardsDictionary[cardId]);
        }
        AddToDeckView(cardsId.Length);

        if (needShuffle)
            MathOperations.GetInstance().ShuffleList(cardsInDeck);
    }
    public override void AddToDeckView(int count, bool isDefSet = false)
    {
        if (count < 0)
        {
            Debug.LogError("Trying add to enemy deck negative count of cards");
            return;
        }
        int cardAdded = 0;
        for (int i = 0; i < count; i++)
        {
            curDeckSize++;
            cardAdded++;
        }
        deckView.Add(cardCollection.cardBack, cardAdded);
    }

    public void AddToGraveyardMirror(int count)
    {
        if (count < 0)
        {
            Debug.LogError("Trying add to player graveyard negative count of cards");
            return;
        }
        gravejardView.Add(cardCollection.cardBack, count);
    }

    public void DrawCardInHand(Card card)
    {
        GameObject newCard = Instantiate(cardCollection.cardBack, parent: hand.transform);
        Vector3 targetScale = cardCollection.cardBack.transform.localScale;
        newCard.transform.localScale = Vector3.zero;
        newCard.transform.DOScale(targetScale, 0.1f);
        hand.AddCard(newCard, card);
    }

    public void DrawHandFromDeck(int[] cardIDs, bool ignoreCardLimit = false)
    {
        if (cardIDs.Length <= 0)
        {
            Debug.LogError("Amount is negative or zero!");
            return;
        }
        if ((!ignoreCardLimit && hand.CardGameobjectsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) || curDeckSize <= 0)
        {
            Debug.LogError($"Return draw. Info: {ignoreCardLimit}, {hand.CardGameobjectsInHand.Count}, {curDeckSize}");
            return;
        }

        for (int i = 0; i < cardIDs.Length; i++)
        {
            GameController.Instance.globalCards.GlobalCardsDictionary.TryGetValue(cardIDs[i], out Card card);
            DrawCardInHand(card);
            deckView.Remove();

            if (curDeckSize == 0) break;
            if (!ignoreCardLimit && hand.CardGameobjectsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) break;
        }
    }

    public override void DrawLastFromGraveyard()
    {
        if (gravejardView.GetCountInStack() < 1) return;
        gravejardView.Remove();
        GameController.Instance.globalCards.GlobalCardsDictionary.TryGetValue(lastGraveyardCardID, out Card c);
        DrawCardInHand(c);
    }

    public override void DestroyAllCard()
    {
        hand.RemoveAllCards();
        deckView.RemoveAll();
        gravejardView.RemoveAll();
        curDeckSize = 0;
        lastGraveyardCardID = -1;
    }

    public void DestroyCardInHand(GameObject card, bool needGravejard = true)
    {
        hand.cardsInHand.TryGetValue(card, out Card c);
        lastGraveyardCardID = c.GetID();

        hand.RemoveCard(card);
        if (needGravejard)
            gravejardView.Add(card);
    }

    public void DestroyAllCardsIn(bool needToAddInGraveyard = true)
    {
        if (needToAddInGraveyard)
            gravejardView.Add(cardCollection.cardBack, hand.CardGameobjectsInHand.Count);
        hand.RemoveAllCards();
    }

    public override void DrawHandRandomFromDeck(int amount, bool ignoreCardLimit = false)
    {
        if (amount < 0)
        {
            Debug.LogError("Amount is negative!");
            return;
        }
        if ((!ignoreCardLimit && hand.CardGameobjectsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) || curDeckSize <= 0)
            return;

        if (banForDrawLastGraveyardCard && Random.Range(0, 100) > (100 - chanceToSkipRestrictOnGetLastDestroyedCard))
            banForDrawLastGraveyardCard = false;

        for (int i = 0; i < amount; i++)
        {
            if (curDeckSize == 0) break;
            if (!ignoreCardLimit && hand.CardGameobjectsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) break;

            Card card = GetRandomCard();
            curDeckSize--;
            cardsInDeck.Remove(card);
            DrawCardInHand(card);
            deckView.Remove();

        }

        banForDrawLastIssuedCard = false;
        banForDrawLastGraveyardCard = false;
    }
}
