using System.Collections.Generic;
using UnityEngine;

public abstract class Deck : MonoBehaviour
{
    [SerializeField] protected CardCollection cardCollection;
    [SerializeField, Range(0, 100)] protected int chanceToSkipRestrictOnGetLastDestroyedCard, maxDeckSize, curDeckSize;
    [SerializeField] protected StackView gravejardView, deckView;

    protected bool banForDrawLastIssuedCard, banForDrawLastGraveyardCard;
    protected int lastIssuedCardID, lastGraveyardCardID;

    public abstract void SetDefaultSettings();

    public abstract void AddToDeck(int count, bool isDefSet = false);

    //DRAW
    public abstract void DrawHandRandomFromDeck(int amount);
    public abstract void DrawLastFromGraveyard();

    //DESTROY
    public abstract void DestroyAllCard();

    #region GetSome
    public virtual int GetGraveyardCardCount() { return gravejardView.GetCountInStack(); }

    protected Card GetRandomCard()
    {
        List<Card> collectionWithoutBlock = new();
        for (int i = 0; i < cardCollection.CardsInCollection.Count; i++)
        {
            Card potentionalCard = cardCollection.CardsInCollection[i];
            int ID = potentionalCard.SelfGetID();

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
            lastIssuedCardID = card.SelfGetID();
            return card;
        }
        card = cardCollection.CardsInCollection[Random.Range(0, cardCollection.CardsInCollection.Count)];
        lastIssuedCardID = card.SelfGetID();
        return card;
    }
    #endregion
}
