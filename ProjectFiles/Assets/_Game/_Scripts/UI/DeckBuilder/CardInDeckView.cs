using TMPro;
using UnityEngine;

public class CardInDeckView : MonoBehaviour
{
    [HideInInspector] public Card cardData;

    [SerializeField]
    TextMeshProUGUI nameCardTxt,
        manaCost, countInDeck;
    [SerializeField] CardsInDeckViewModel viewModel;

    public void OnClick()
    {
        viewModel.RemoveCard(cardData);
    }

    internal void SetCardData(Card cardData, int amountInDeck)
    {
        this.cardData = cardData;
        nameCardTxt.text = cardData.originalCardName;
        manaCost.text = cardData.ManaCost.ToString();
        countInDeck.text = amountInDeck.ToString();
    }
}
