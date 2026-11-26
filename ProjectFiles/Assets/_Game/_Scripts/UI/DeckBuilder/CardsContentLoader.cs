using System.Collections.Generic;
using UnityEngine;

public class CardsView : MonoBehaviour
{
    [SerializeField] CardShopView cardTemplate;
    [SerializeField] GameObject deckTitle;
    [SerializeField] Transform container;
    [SerializeField] List<CardShopView> cards = new();

    // ===== Lookup =====

    public CardShopView GetCard(Card card)
    {
        if (card == null) return null;
        int id = card.ID;

        foreach (var cardView in cards)
        {
            if (cardView == null || cardView.cardData == null) continue;
            if (cardView.cardData.ID == id) return cardView;
        }
        return null;
    }

    // ===== Lifecycle =====

    private void OnEnable()
    {
        if (cardTemplate != null) cardTemplate.gameObject.SetActive(false);
        Rebuild();
    }

    private void OnDisable()
    {
        Clear();
    }

    // ===== Build =====

    private void Rebuild()
    {
        Clear();

        var gc = GameController.Instance;
        if (gc == null) return;

        var global = gc.globalCards;
        if (global == null || global.GlobalCardsDictionary == null) return;
        if (cardTemplate == null || container == null) return;

        foreach (var cardData in global.GlobalCardsDictionary.Values)
        {
            if (cardData == null) continue;

            var cardView = Instantiate(cardTemplate, container);
            cardView.SetCardData(cardData);
            cardView.gameObject.SetActive(true);
            cards.Add(cardView);
        }
    }

    private void Clear()
    {
        if (container != null)
        {
            foreach (Transform child in container)
            {
                if (cardTemplate != null && cardTemplate.transform == child) continue;
                if (deckTitle != null && deckTitle.transform == child) continue;
                Destroy(child.gameObject);
            }
        }
        cards.Clear();
    }
}