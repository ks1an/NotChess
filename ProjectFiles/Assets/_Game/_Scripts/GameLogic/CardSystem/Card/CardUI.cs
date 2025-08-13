using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class CardUI : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] TextMeshPro cardName;
    [SerializeField] TextMeshPro manaCostTxt;
    [SerializeField] TextMeshPro cardDescription;
    [SerializeField] SpriteRenderer picture, border, cardRender;
    [SerializeField] SortingGroup sorting;

    [Header("Settings")]
    [SerializeField] Color passiveColor;
    [SerializeField] Color hoverColor, selectColor;

    Card card;
    int orientation, effectType, rarity;

    public void SetCardUI()
    {
        card = GetComponent<Card>();
        if(sorting == null)
            sorting = GetComponent<SortingGroup>();

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

    void Awake()
    {
        cardRender.color = passiveColor;
    }

    void OnMouseEnter()
    {
        if (Deck.Instance.playerHand.CurrentSelectCard != card)
        {
            sorting.sortingOrder = 1;
            cardRender.color = hoverColor;
        }
    }
    void OnMouseExit()
    {
        if (Deck.Instance.playerHand.CurrentSelectCard != card)
        {
            sorting.sortingOrder = 0;
            cardRender.color = passiveColor;
        }
        else
        {
            sorting.sortingOrder = 2;
            cardRender.color = selectColor;
        }
    }
}
