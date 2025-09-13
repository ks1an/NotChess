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
        DestroyAllCard();
        curDeckSize = maxDeckSize;
        AddToDeck(maxDeckSize, true);
        isNet = GameController.Instance.states.isNetMatch;
        if (isNet)
            netCard = GameController.Instance.netMatch.cardSync;
        if (isNet)
            netCard.SetDefaultRpc();
    }
    public override void AddToDeck(int count, bool isDefSet = false)
    {
        if (count < 0)
        {
            Debug.LogError("Trying add to player deck negative count of cards");
            return;
        }

        deckView.Add(cardCollection.cardBack, count);
        if (isNet && !isDefSet)
            netCard.AddToDeckRpc(count);
    }

    //====DRAW====
    void DrawCardInHand(Card card)
    {
        Card newCard = Instantiate(card.gameObject, parent: hand.transform).GetComponent<Card>();
        Vector3 targetScale = card.transform.localScale;
        newCard.transform.localScale = Vector3.zero;
        newCard.transform.DOScale(targetScale, 0.15f);
        hand.AddCard(newCard);

        newCard.Init();
    }
    public override void DrawHandRandomFromDeck(int amount)
    {
        if (hand.CardsInHand.Count == GameController.Instance.settings.maxCardsInHand || curDeckSize <= 0)
            return;

        if (banForDrawLastGraveyardCard && Random.Range(0, 100) > (100 - chanceToSkipRestrictOnGetLastDestroyedCard))
            banForDrawLastGraveyardCard = false;
        for (int i = 0; i < amount; i++)
        {
            Card card = GetRandomCard();
            DrawCardInHand(card);
            deckView.Remove();
            curDeckSize--;
            if (curDeckSize == 0) break;
        }
    
        banForDrawLastIssuedCard = false;
        banForDrawLastGraveyardCard = false;
        if(netCard)
            netCard.DrawHandRandomFromDeckRpc(amount);
    }
    public override void DrawLastFromGraveyard()
    {
        Card card = null;
        if (gravejardView.GetCountInStack() > 0)
        {
            GameController.Instance.globalCardCollection.GlobalCardsDictionary.TryGetValue(lastGraveyardCardID, out Card c);
            card = c;
        }

        if (card != null)
        {
            gravejardView.Remove();
            DrawCardInHand(card);
            if (isNet)
                netCard.DrawLastFromGraveyardRpc();
        }
    }

    //====DESTROY====
    public override void DestroyAllCard()
    {
        hand.RemoveAllCards();
        deckView.RemoveAll();
        gravejardView.RemoveAll();
        lastGraveyardCardID = -1;
        lastIssuedCardID = -1;

        if (isNet)
            netCard.DestroyAllRpc();
        else
            EnemyDeck.Instance.DestroyAllCard();
    }
    public void DestroyCardInHand(Card card)
    {
        lastGraveyardCardID = card.ID;
        Destroy(card.gameObject);
        hand.RemoveCard(card);
        gravejardView.Add(cardCollection.cardBack);
        if (isNet)
            netCard.DestroyCardRpc();
    }

    public override int GetGraveyardCardCount() { return gravejardView.GetCountInStack(); }
}
