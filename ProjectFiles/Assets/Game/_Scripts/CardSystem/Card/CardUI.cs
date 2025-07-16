using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class CardUI : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] TextMeshPro cardName;
    [SerializeField] TextMeshPro manaCostTxt;
    [SerializeField] TextMeshPro cardDescription;

    [SerializeField] SpriteRenderer picture;
    [SerializeField] SpriteRenderer border;

    SortingGroup sorting;
    Card card;
    int orientation, effectType, rarity;

    public void SetCardUI()
    {
        sorting = GetComponent<SortingGroup>();
        card = GetComponent<Card>();
        SetCardSettings();
    }

    public void SetBorderColor(Color color) => border.color = color;

    void SetCardSettings()
    {
        picture.sprite = card.Image;
        SetBorderColor(card.ColorBorder);

        cardName.text = card.Name;
        cardDescription.text = card.Description;
        manaCostTxt.text = card.ManaCost.ToString();

        orientation = (int)card.Orientation;
        effectType = (int)card.Category;
        rarity = (int)card.Rarity;
    }

    void OnMouseEnter()
    {
        if (Deck.Instance.playerHand.CurrentSelectCard != card)
            sorting.sortingOrder = 1;
    }
    void OnMouseExit()
    {
        if (Deck.Instance.playerHand.CurrentSelectCard != card)
            sorting.sortingOrder = 0;
        else
            sorting.sortingOrder = 2;
    }
}
