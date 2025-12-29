using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class CardsInDeckViewModel : MonoBehaviour
{
    [SerializeField] CardInDeckView cardTemplate;
    [SerializeField] Transform container;
    [SerializeField] TextMeshProUGUI avgManaText, countInDeck;

    List<Card> cardsInDeckViewModel = new();

    private void OnEnable()
    {
        cardTemplate.gameObject.SetActive(false);
        int sumManaCost = 0;
        foreach (Card cardData in GameController.Instance.globalCards.GlobalCardsDictionary.Values)
        {
            CardInDeckView cardView = GameObject.Instantiate(cardTemplate, container);

            cardsInDeckViewModel.Add(cardData);
            int countInDeckViewModel = 0;
            for(int i = 0; i < cardsInDeckViewModel.Count; i++)
            {
                if (cardsInDeckViewModel[i].GetType() == cardData.GetType())
                    countInDeckViewModel++;
            }

            cardView.SetCardData(cardData, countInDeckViewModel);
            cardView.gameObject.SetActive(true);

            sumManaCost += cardData.ManaCost;
        }

        countInDeck.text = $"<b>{cardsInDeckViewModel.Count}</b> /30 \n cards in deck";
        avgManaText.text = $"<b><color=#43A5BE>{sumManaCost / cardsInDeckViewModel.Count}</b></color> \n avg. mana";
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
