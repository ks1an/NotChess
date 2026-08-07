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

    public void SetCardData(Card data)
    {
        cardData = data;

        describeTxt.text = cardData.originalDescription;

        manaCostTxt.text = cardData.ManaCost.ToString();
        graveCoinCostTxt.text = cardData.GraveTokensCost.ToString();

        if(data.ManaCost == 0 && data.GraveTokensCost != 0)
        {
            manaCostTxt.gameObject.transform.parent.gameObject.SetActive(false);
        }
        else if (data.GraveTokensCost == 0)
        {
            graveCoinCostTxt.gameObject.transform.parent.gameObject.SetActive(false);
        }

        image.sprite = cardData.Image;
        startLocalY_forNameBlackoutAnim = blackoutName.rectTransform.localPosition.y;
        startLocalY_forDescribeBlackoutAnim = blackoutDescribe.rectTransform.localPosition.y;
        startLocalY_forManaCostAnim = costFrame.rectTransform.localPosition.y;

        inDeckIndicator.SetCard(cardData, cardsViewModel);
        inDeckIndicator.gameObject.SetActive(false);
        string cardnameKey = cardData.originalCardName.Replace(" ", "");
        LocalizeStringEvent nameLocalize = nameCardTxt.gameObject.GetComponent<LocalizeStringEvent>();
        if (!GameController.Instance.gameSettings.DontTranslateNameOfCard.Value)
        {
            string entryNameName = cardnameKey + "CardName";
            nameLocalize.enabled = true;
            nameLocalize.SetEntry(entryNameName);
        }
        else
        {
            nameLocalize.enabled = false;
            nameCardTxt.text = cardData.originalCardName;
        }

        string entryNameDescribe = cardnameKey + "CardDescribe";
        describeTxt.gameObject.GetComponent<LocalizeStringEvent>().SetEntry(entryNameDescribe);
    }

    public void OnClicked() => cardsViewModel.AddCard(cardData);

    public void UpdateView(bool isInDeck)
    {
        inDeckIndicator.gameObject.SetActive(isInDeck);
    }

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
