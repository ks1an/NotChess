using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardInDeckView : MonoBehaviour
{
    [HideInInspector] public Card cardData;

    [SerializeField]
    TextMeshProUGUI nameCardTxt,
        manaCost, countInDeck;
    [SerializeField] Image CountInDeckImage, backCardImage;
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
        if (amountInDeck > 1)
        {
            countInDeck.gameObject.SetActive(true);
            countInDeck.text = "x" + amountInDeck.ToString();
            CountInDeckImage.gameObject.SetActive(true);
        }
        else
        {
            countInDeck.gameObject.SetActive(false);
            CountInDeckImage.gameObject.SetActive(false);
        }
        backCardImage.gameObject.SetActive(false);
        //backCardImage.sprite = cardData.Image;
    }
}
