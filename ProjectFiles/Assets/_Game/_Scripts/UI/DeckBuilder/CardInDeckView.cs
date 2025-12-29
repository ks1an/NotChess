using TMPro;
using UnityEngine;

public class CardInDeckView : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI nameCardTxt,
        manaCost, countInDeck;

    internal void SetCardData(Card cardData, int amountInDeck)
    {
        nameCardTxt.text = cardData.originalCardName;
        manaCost.text = cardData.ManaCost.ToString();
        countInDeck.text = amountInDeck.ToString();
    }
}
