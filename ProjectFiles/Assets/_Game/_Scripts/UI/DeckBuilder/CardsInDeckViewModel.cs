using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class CardsInDeckViewModel : MonoBehaviour
{
    [SerializeField] CardInDeckView cardTemplate;
    [SerializeField] Transform container;
    [SerializeField] TextMeshProUGUI avgManaText, countInDeck;

    List<CardInDeckView> cardsInDeckViewModel = new();

    public void AddCard(Card cardData)
    {
        int sumManaCost = cardData.ManaCost;
        for (int i = 0; i < cardsInDeckViewModel.Count; i++)
        {
            if (cardsInDeckViewModel[i].cardData.GetType() == cardData.GetType())
                return;
            sumManaCost += cardsInDeckViewModel[i].cardData.ManaCost;
        }

        CardInDeckView cardView = GameObject.Instantiate(cardTemplate, container);
        cardsInDeckViewModel.Add(cardView);
        cardView.SetCardData(cardData, 1);
        cardView.gameObject.SetActive(true);

        countInDeck.text = $"<b>{cardsInDeckViewModel.Count}</b> /{PlayerDeck.Instance.DeckSize} \n unique cards in deck";
        avgManaText.text = $"<b><color=#43A5BE>{sumManaCost / cardsInDeckViewModel.Count}</b></color> \n avg. mana";
    }

    public void RemoveCard(Card cardData)
    {
        int sumManaCost = 0;
        for (int i = 0; i < cardsInDeckViewModel.Count; i++)
        {
            sumManaCost += cardsInDeckViewModel[i].cardData.ManaCost;
            if (cardsInDeckViewModel[i].cardData.GetType() == cardData.GetType())
            {
                sumManaCost -= cardsInDeckViewModel[i].cardData.ManaCost;
                Destroy(cardsInDeckViewModel[i].gameObject);
                cardsInDeckViewModel.Remove(cardsInDeckViewModel[i]);
            }
        }

        if (cardsInDeckViewModel.Count > 0)
        {
            countInDeck.text = $"<b>{cardsInDeckViewModel.Count}</b> /{PlayerDeck.Instance.DeckSize} \n unique cards in deck";
            avgManaText.text = $"<b><color=#43A5BE>{sumManaCost / cardsInDeckViewModel.Count}</b></color> \n avg. mana";
        }
        else
        {
            countInDeck.text = $"<b>0</b> /{PlayerDeck.Instance.DeckSize} \n unique cards in deck";
            avgManaText.text = $"<b><color=#43A5BE>0</b></color> \n avg. mana";
        }
    }

    public void SaveDeck()
    {
        PlayerDeck.Instance.cardCollectionFromSave.ClearCollection();
        for(int i = 0;i < cardsInDeckViewModel.Count; i++)
        {
            PlayerDeck.Instance.cardCollectionFromSave.AddCardToCollection(cardsInDeckViewModel[i].cardData);
        }
        PlayerDeck.Instance.cardCollectionFromSave.SaveDataToJson();
    }

    private void OnEnable()
    {
        cardTemplate.gameObject.SetActive(false);
        foreach (Card cardData in PlayerDeck.Instance.cardCollectionFromSave.CardsInCollection)
        {
            AddCard(cardData);
        }
    }

    private void OnDisable()
    {
        foreach (Transform child in container)
        {
            if (cardTemplate.transform == child) continue;
            Destroy(child.gameObject);
        }

        cardsInDeckViewModel.Clear();
    }
}
