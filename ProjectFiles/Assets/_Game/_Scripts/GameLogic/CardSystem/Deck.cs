using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public sealed class Deck : MonoBehaviour
{
    public static Deck Instance { get; private set; }

    public CardHand playerHand;
    [SerializeField] CardCollection playerDeck;
    [SerializeField, Range(0, 100)] int chanceToSkipRestrictOnGetLastDestroyedCard;

    int lastIssuedCardID, lastDestroyedCardID;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            lastIssuedCardID = -1;
            lastDestroyedCardID = -1;
        }
        else
            Destroy(gameObject);
    }

    public void DrawHand(int amount)
    {
        if (playerHand.CardsInHand.Count == GameController.Instance.settings.maxCardsInHand)
            return;

        if (lastDestroyedCardID > -1 && Random.Range(0, 100) > (100-chanceToSkipRestrictOnGetLastDestroyedCard))
            lastDestroyedCardID = -1;

        for (int i = 0; i < amount; i++)
        {
            Card card = GetRandomCard();

            Card newCard = Instantiate(card.gameObject, parent: playerHand.transform).GetComponent<Card>();
            newCard.gameObject.transform.localScale = Vector3.zero;
            newCard.gameObject.transform.DOScale(Vector3.one, 0.15f);
            StartCoroutine(playerHand.AddCard(newCard));

            newCard.Init();
        }

        lastIssuedCardID = -1;
        lastDestroyedCardID = -1;
    }

    public void DestroyCard(Card card)
    {
        lastDestroyedCardID = card.ID;
        Destroy(card.gameObject);
        StartCoroutine(playerHand.RemoveCard(card));
    }

    public void DestroyAllCard()
    {
        for (int i = 0; i < playerHand.CardsInHand.Count; i++)
            Destroy(playerHand.CardsInHand[i].gameObject);
        playerHand.RemoveAllCards();
        lastIssuedCardID = -1;
        lastDestroyedCardID = -1;
    }

    Card GetRandomCard()
    {
        List<Card> collectionWithoutBlock = new();
        for (int i = 0; i < playerDeck.CardsInCollection.Count; i++)
        {
            Card potentionalCard = playerDeck.CardsInCollection[i];
            int ID = potentionalCard.SelfGetID();
            if (ID != lastIssuedCardID && ID != lastDestroyedCardID)
                collectionWithoutBlock.Add(potentionalCard);
        }

        Card card;
        if (collectionWithoutBlock.Count > 1)
        {
            card = collectionWithoutBlock[Random.Range(0, collectionWithoutBlock.Count)];
            lastIssuedCardID = card.SelfGetID();
            return card;
        }
        card = playerDeck.CardsInCollection[Random.Range(0, playerDeck.CardsInCollection.Count)];
        lastIssuedCardID = card.SelfGetID();
        return card;
    }

}
