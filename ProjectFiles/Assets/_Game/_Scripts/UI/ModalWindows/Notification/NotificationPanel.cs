using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NotificationPanel : MonoBehaviour
{
    [SerializeField] float onScreenYPos, outScreenYPos;
    [SerializeField] float defaultMoveTime, defaultStayTime;
    [SerializeField] TextMeshProUGUI contentText;

    RectTransform rect;
    Button button;
    Sequence curAnim;
    void Awake()
    {
        rect = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        gameObject.SetActive(false);
    }

    public void ShowNotification(string notificationText, UnityAction actionOnClick, float moveTime = -1, float stayTime = -1)
    {
        button.onClick.AddListener(actionOnClick);
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, outScreenYPos);
        if (moveTime <= 0)
            moveTime = defaultMoveTime;
        if (stayTime <= 0)
            stayTime = defaultStayTime;

        contentText.text = notificationText;

        if (curAnim.IsActive()) curAnim.Kill();
        curAnim = DOTween.Sequence();
        curAnim.Join(rect.DOAnchorPosY(onScreenYPos, moveTime))
            .AppendInterval(stayTime)
            .OnComplete(() => rect.DOAnchorPosY(outScreenYPos, moveTime).OnComplete(() =>
            {
                button.onClick.RemoveAllListeners();
                gameObject.SetActive(false);
            }));
    }
}
