using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class CardShopView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] CardsInDeckViewModel cardsViewModel;
    [SerializeField] TextMeshProUGUI nameCardTxt, describeTxt, manaCostTxt;
    [SerializeField] Image image, blackoutName, blackoutDescribe, manaCostFrame;

    [Space(10), Header("Hower Anim")]
    [SerializeField] float durationTransFocusView;
    [SerializeField] Color notHowerImageColor, howerImageColor;

    Card cardData;
    float startLocalY_forNameBlackoutAnim, startLocalY_forDescribeBlackoutAnim, startLocalY_forManaCostAnim;

    public void SetCardData(Card data)
    {
        cardData = data;

        describeTxt.text = cardData.originalDescription;
        manaCostTxt.text = cardData.ManaCost.ToString();

        image.sprite = cardData.Image;
        startLocalY_forNameBlackoutAnim = blackoutName.rectTransform.localPosition.y;
        startLocalY_forDescribeBlackoutAnim = blackoutDescribe.rectTransform.localPosition.y;
        startLocalY_forManaCostAnim = manaCostFrame.rectTransform.localPosition.y;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        blackoutName.rectTransform.DOKill(true);
        blackoutName.rectTransform.DOLocalMoveY(startLocalY_forNameBlackoutAnim + 280f, durationTransFocusView)
            .SetEase(Ease.OutSine);

        blackoutDescribe.rectTransform.DOKill(true);
        blackoutDescribe.rectTransform.DOLocalMoveY(startLocalY_forDescribeBlackoutAnim + 280f, durationTransFocusView)
            .SetEase(Ease.OutSine);

        manaCostFrame.rectTransform.DOKill(true);
        manaCostFrame.rectTransform.DOLocalMoveY(startLocalY_forManaCostAnim - 280f, durationTransFocusView / 2);

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

        manaCostFrame.rectTransform.DOKill(true);
        manaCostFrame.rectTransform.DOLocalMoveY(startLocalY_forManaCostAnim, durationTransFocusView);

        image.DOColor(notHowerImageColor, durationTransFocusView);
    }
}
