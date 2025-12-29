using DG.Tweening;
using UnityEngine;

public sealed class EnemyDeck : Deck
{
    public EnemyCardHand hand;
    public static EnemyDeck Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    public override void SetDefaultSettings()
    {
        DestroyAllCard();
        AddToDeck(maxDeckSize, true);
    }

    public override void AddToDeck(int count, bool isDefSet = false)
    {
        if (count < 0)
        {
            Debug.LogError("Trying add to enemy deck negative count of cards");
            return;
        }
        int cardAdded = 0;
        for (int i = 0; i < count; i++)
        {
            if (curDeckSize == maxDeckSize) break;
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

    public void DrawCardInHand(GameObject card)
    {
        GameObject newCard = Instantiate(card, parent: hand.transform);
        Vector3 targetScale = card.transform.localScale;
        newCard.transform.localScale = Vector3.zero;
        newCard.transform.DOScale(targetScale, 0.1f);
        hand.AddCard(newCard);
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

        for (int i = 0; i < amount; i++)
        {
            DrawCardInHand(cardCollection.cardBack);
            deckView.Remove();
            curDeckSize--;
            if (curDeckSize == 0) break;
            if (!ignoreCardLimit && hand.CardsInHand.Count == GameController.Instance.matchSettings.defaultCardsInHand) break;
        }
    }

    public override void DrawLastFromGraveyard()
    {
        if (gravejardView.GetCountInStack() < 1) return;
        gravejardView.Remove();
        DrawCardInHand(cardCollection.cardBack);
    }

    public override void DestroyAllCard()
    {
        hand.RemoveAllCards();
        deckView.RemoveAll();
        gravejardView.RemoveAll();
        curDeckSize = 0;
    }

    public void DestroyCardInHand(GameObject card)
    {
        hand.RemoveCard(card);
        gravejardView.Add(card);
    }

    public void DestroyAllCardsIn(bool needToAddInGraveyard = true)
    {
        if (needToAddInGraveyard)
            gravejardView.Add(cardCollection.cardBack, hand.CardsInHand.Count);
        hand.RemoveAllCards();
    }

    public GameObject GetCardBack() { return cardCollection.cardBack; }
    public GameObject GetRandomCardFromHand() { return hand.CardsInHand[Random.Range(0, hand.CardsInHand.Count)]; }
}
