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

    void Awake()
    {
        cardRender.color = passiveColor;
    }

    public void SetCardUI()
    {
        card = GetComponent<Card>();
        if (sorting == null)
            sorting = GetComponent<SortingGroup>();

        SetCardSettings();
    }

    void SetCardSettings()
    {
        picture.sprite = card.Image;
        SetBorderColor(card.ColorBorder);

        cardName.text = card.DisplayName;
        cardDescription.text = card.DisplayDescription;
        manaCostTxt.text = card.ManaCost.ToString();

        orientation = (int)card.Orientation;
        effectType = (int)card.Category;
        rarity = (int)card.Rarity;
    }

    public void SetBorderColor(Color color) => border.color = color;

    public void CreateCardInHandLogic()
    {
        HoverObject cardInHand = gameObject.AddComponent<HoverObject>();
        cardInHand.SetSettigns(MouseEnterFromCard, MouseExitFromCard);
    }

    void MouseEnterFromCard(GameObject cardInHand)
    {
        card.Hand.CardUpDownMove(card, true, true);
        if (PlayerDeck.Instance.hand.CurrentSelectCard != card)
        {
            sorting.sortingOrder = 1;
            cardRender.color = hoverColor;
        }
    }

    void MouseExitFromCard(GameObject cardInHand)
    {
        sorting.sortingOrder = 0;
        cardRender.color = passiveColor;
        card.Hand.CardUpDownMove(card, false, true);
        if (PlayerDeck.Instance.hand.CurrentSelectCard == card)
        {
            sorting.sortingOrder = 2;
            cardRender.color = selectColor;
        }
    }
}
