using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public enum CardVisualState
{
    Passive,     
    Hover,      
    Selected    
}

[RequireComponent(typeof(SortingGroup), typeof(BoxCollider))]
public class CardVisual : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] TextMeshPro cardName;
    [SerializeField] TextMeshPro manaCostTxt;
    [SerializeField] TextMeshPro cardDescription;
    [SerializeField] SpriteRenderer picture, border, blakoutRender;
    [SerializeField] SortingGroup sorting;

    [Header("Settings")]
    [SerializeField] Color passiveColor;
    [SerializeField] Color hoverColor, selectColor;

    Card card;
    int orientation, effectType, rarity;
    new BoxCollider collider;
    CardVisualState visualState;

    void Awake()
    {
        blakoutRender.color = passiveColor;
        collider = GetComponent<BoxCollider>();
    }

    public void SetCardUI()
    {
        card = GetComponent<Card>();
        if (sorting == null)
            sorting = GetComponent<SortingGroup>();

        SetCardSettings();
    }

    public void CreateCardInHandLogic()
    {
        HoverObject cardInHand = gameObject.AddComponent<HoverObject>();
        cardInHand.SetSettigns(MouseEnterFromCard, MouseExitFromCard);
    }

    public void MouseEnterFromCard(GameObject cardInHand)
    {
        if(visualState == CardVisualState.Passive)
        {
            if(card.Hand.CurrentSelectCard == card)
            {
                card.Hand.CardUpDownMove(card, true, false);
                SetSelectedState();
            }
            else
            {
                card.Hand.CardUpDownMove(card, true, true);
                SetHoverState();
            }
        }
        else if(visualState == CardVisualState.Selected)
        {
            card.Hand.CardUpDownMove(card, true, true);
        }
        else if(visualState == CardVisualState.Hover)
        {
            if(card.Hand.CurrentSelectCard == card)
            {
                card.Hand.CardUpDownMove(card, true, false);
                SetSelectedState();
            }
        }
    }

    public void MouseExitFromCard(GameObject cardInHand)
    {
        if(visualState == CardVisualState.Hover)
        {
            card.Hand.CardUpDownMove(card, false, true);
            SetPassiveState();
        }
        else if(visualState == CardVisualState.Selected)
        {
            if(card.Hand.CurrentSelectCard == card)
            {
                card.Hand.CardUpDownMove(card, false, true);
            }
            else
            {
                if (orientation != (int)CardOrientation.Social)
                    EnvironmentManager.Instance.SetActiveVignetteFocus(false);
                card.Hand.CardUpDownMove(card, false, false);
                SetPassiveState();
            }
        }
    }

    void SetPassiveState()
    {
        visualState = CardVisualState.Passive;
        border.color = card.ColorBorder;
        sorting.sortingOrder = 0;
        collider.layerOverridePriority = 0;
        blakoutRender.color = passiveColor;
    }

    void SetHoverState()
    {
        visualState = CardVisualState.Hover;
        border.color = card.ColorBorder;
        sorting.sortingOrder = 1;
        collider.layerOverridePriority = 1;
        blakoutRender.color = hoverColor;
    }

    void SetSelectedState()
    {
        border.color = card.BoarderColorOnDrag;
        visualState = CardVisualState.Selected;
        sorting.sortingOrder = 2;
        collider.layerOverridePriority = 2;
        blakoutRender.color = selectColor;

        if (orientation != (int)CardOrientation.Social)
            EnvironmentManager.Instance.SetActiveVignetteFocus(true);
    }

    void SetCardSettings()
    {
        picture.sprite = card.Image;
        border.color = card.ColorBorder;

        cardName.text = card.DisplayName;
        cardDescription.text = card.DisplayDescription;
        manaCostTxt.text = card.ManaCost.ToString();

        orientation = (int)card.Orientation;
        effectType = (int)card.Category;
        rarity = (int)card.Rarity;
        visualState = CardVisualState.Passive;
    }
}
