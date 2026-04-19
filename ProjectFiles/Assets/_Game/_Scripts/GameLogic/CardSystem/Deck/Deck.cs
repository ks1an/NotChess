using System.Collections.Generic;
using UnityEngine;

public abstract class Deck : MonoBehaviour
{
    [HideInInspector] public List<Card> cardsInDeck;

    public int DeckSize { get => maxDeckSize; set { DeckSize = maxDeckSize; } }

    [HideInInspector] public CardCollection cardCollection;
    [SerializeField, Range(0, 100)] protected int chanceToSkipRestrictOnGetLastDestroyedCard, maxDeckSize, curDeckSize;
    [SerializeField] protected StackView gravejardView, deckView;

    protected bool banForDrawLastIssuedCard, banForDrawLastGraveyardCard;
    protected int lastIssuedCardID, lastGraveyardCardID;

    public void SetCardCollection(CardCollection newCollection)
    {
        cardCollection = newCollection;
    }

    public abstract void SetDefaultSettings();

    public abstract void AddToDeckView(int count, bool isDefSet = false);

    //DRAW
    public abstract void DrawHandRandomFromDeck(int amount, bool ignoreCardLimit = false);
    public abstract void DrawLastFromGraveyard();

    //DESTROY
    public abstract void DestroyAllCard();

    #region GetSome
    public virtual int GetGraveyardCardCount() { return gravejardView.GetCountInStack(); }

    protected Card GetRandomCard()
    {
        List<Card> collectionWithoutBlock = new();
        for (int i = 0; i < cardsInDeck.Count; i++)
        {
            Card potentionalCard = cardsInDeck[i];
            int ID = potentionalCard.GetID();

            if (banForDrawLastIssuedCard)
                if (ID == lastIssuedCardID)
                    ID = -1;
            if (banForDrawLastGraveyardCard)
                if (ID == lastGraveyardCardID)
                    ID = -1;


            if (ID > -1)
                collectionWithoutBlock.Add(potentionalCard);
        }

        Card card;
        if (collectionWithoutBlock.Count > 1)
        {
            card = collectionWithoutBlock[Random.Range(0, collectionWithoutBlock.Count)];
            lastIssuedCardID = card.GetID();
            return card;
        }
        card = cardsInDeck[0];
        lastIssuedCardID = card.GetID();
        return card;
    }
    #endregion
}
