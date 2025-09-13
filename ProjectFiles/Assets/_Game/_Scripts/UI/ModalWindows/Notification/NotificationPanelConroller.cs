using UnityEngine;
using UnityEngine.Events;

public class NotificationPanelConroller : MonoBehaviour
{
    public static NotificationPanelConroller Instance { get; private set; }
    [SerializeField] NotificationPanel panel;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void ShowNotification(string notificationText, UnityAction actionOnClick, float moveTime = -1, float stayTime = -1)
    {
        panel.gameObject.SetActive(true);
        panel.ShowNotification(notificationText, actionOnClick, moveTime, stayTime);
    }
}
