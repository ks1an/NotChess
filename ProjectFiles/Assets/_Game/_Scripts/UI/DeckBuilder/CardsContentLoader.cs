using System.Collections.Generic;
using UnityEngine;

public class CardsView : MonoBehaviour
{
    [SerializeField] CardShopView cardTemplate;
    [SerializeField] GameObject deckTitle;
    [SerializeField] Transform container;
    [SerializeField] List<CardShopView> cards;
    [SerializeField] CardsInDeckViewModel inDeckViewModel; 

    public CardShopView GetCard(Card card)
    {
        foreach(CardShopView cardView in cards)
        {
            if(cardView.cardData == card)
                return cardView;
        }
        Debug.LogError("NOT FOUND");
        return null;
    }

    private void OnEnable()
    {
        cardTemplate.gameObject.SetActive(false);
        foreach (Card cardData in GameController.Instance.globalCards.GlobalCardsDictionary.Values)
        {
            CardShopView cardView = GameObject.Instantiate(cardTemplate, container);
            cardView.SetCardData(cardData);
            cardView.gameObject.SetActive(true);
            cards.Add(cardView);
        }
        inDeckViewModel.LoadCards();
    }

    private void OnDisable()
    {
        foreach (Transform child in container)
        {
            if (cardTemplate.transform == child || deckTitle.transform == child) continue;
            Destroy(child.gameObject);
        }
        cards.Clear();
    }
}
