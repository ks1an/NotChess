using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class CardShopView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] CardsInDeckViewModel cardsViewModel;
    [SerializeField] TextMeshProUGUI nameCardTxt, describeTxt, manaCostTxt, graveCoinCostTxt;
    [SerializeField] Image image, blackoutName, blackoutDescribe, costFrame;
    [SerializeField] CardShopInDeckIndicator inDeckIndicator;

    [Space(10), Header("Hower Anim")]
    [SerializeField] float durationTransFocusView;
    [SerializeField] Color notHowerImageColor, howerImageColor;

    public Card cardData;
    float startLocalY_forNameBlackoutAnim, startLocalY_forDescribeBlackoutAnim, startLocalY_forManaCostAnim;

    // ===== Lifecycle =====

    void OnEnable()
    {
        if (cardsViewModel != null)
            cardsViewModel.DeckChanged += RefreshInDeckIndicator;
        RefreshInDeckIndicator();
    }

    void OnDisable()
    {
        if (cardsViewModel != null)
            cardsViewModel.DeckChanged -= RefreshInDeckIndicator;
    }

    // ===== Data =====

    public void SetCardData(Card data)
    {
        cardData = data;

        describeTxt.text = cardData.originalDescription;
        manaCostTxt.text = cardData.ManaCost.ToString();
        graveCoinCostTxt.text = cardData.GraveTokensCost.ToString();

        if (data.ManaCost == 0 && data.GraveTokensCost != 0)
            manaCostTxt.gameObject.transform.parent.gameObject.SetActive(false);
        else if (data.GraveTokensCost == 0)
            graveCoinCostTxt.gameObject.transform.parent.gameObject.SetActive(false);

        image.sprite = cardData.Image;
        startLocalY_forNameBlackoutAnim = blackoutName.rectTransform.localPosition.y;
        startLocalY_forDescribeBlackoutAnim = blackoutDescribe.rectTransform.localPosition.y;
        startLocalY_forManaCostAnim = costFrame.rectTransform.localPosition.y;

        inDeckIndicator.SetCard(cardData, cardsViewModel);
        RefreshInDeckIndicator();

        string cardnameKey = cardData.originalCardName.Replace(" ", "");
        var nameLocalize = nameCardTxt.gameObject.GetComponent<LocalizeStringEvent>();
        if (!GameController.Instance.gameSettings.DontTranslateNameOfCard.Value)
        {
            nameLocalize.enabled = true;
            nameLocalize.SetEntry(cardnameKey + "CardName");
        }
        else
        {
            nameLocalize.enabled = false;
            nameCardTxt.text = cardData.originalCardName;
        }

        describeTxt.gameObject.GetComponent<LocalizeStringEvent>().SetEntry(cardnameKey + "CardDescribe");
    }

    void RefreshInDeckIndicator()
    {
        if (inDeckIndicator == null || cardData == null || cardsViewModel == null) return;
        var d = cardsViewModel.CurrentDeck;
        inDeckIndicator.gameObject.SetActive(d != null && d.Contains(cardData));
    }

    // ===== Actions =====

    public void OnClicked() => cardsViewModel?.AddCard(cardData);

    // ===== Hover =====

    public void OnPointerEnter(PointerEventData eventData)
    {
        blackoutName.rectTransform.DOKill(true);
        blackoutName.rectTransform.DOLocalMoveY(startLocalY_forNameBlackoutAnim + 280f, durationTransFocusView)
            .SetEase(Ease.OutSine);

        blackoutDescribe.rectTransform.DOKill(true);
        blackoutDescribe.rectTransform.DOLocalMoveY(startLocalY_forDescribeBlackoutAnim + 280f, durationTransFocusView)
            .SetEase(Ease.OutSine);

        costFrame.rectTransform.DOKill(true);
        costFrame.rectTransform.DOLocalMoveY(startLocalY_forManaCostAnim - 280f, durationTransFocusView / 2);

        image.DOColor(howerImageColor, durationTransFocusView).SetEase(Ease.InOutQuart);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        blackoutName.rectTransform.DOKill(true);
        blackoutName.rectTransform.DOLocalMoveY(startLocalY_forNameBlackoutAnim, durationTransFocusView / 2)
            .SetEase(Ease.OutSine);

        blackoutDescribe.rectTransform.DOKill(true);
        blackoutDescribe.rectTransform.DOLocalMoveY(startLocalY_forDescribeBlackoutAnim, durationTransFocusView / 2)
            .SetEase(Ease.OutSine);

        costFrame.rectTransform.DOKill(true);
        costFrame.rectTransform.DOLocalMoveY(startLocalY_forManaCostAnim, durationTransFocusView);

        image.DOColor(notHowerImageColor, durationTransFocusView);
    }
}