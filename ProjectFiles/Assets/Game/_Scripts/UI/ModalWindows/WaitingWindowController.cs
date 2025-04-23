using UnityEngine;

public sealed class WaitingWindowController : MonoBehaviour
{
    public static WaitingWindowController Instance;
    [SerializeField] WaitingWindow window;

    void OnEnable()
    {
        Instance = this;
    }

    public void Show()
    {
        window.gameObject.SetActive(true);
        window.SetLoadText();
    }

    public void Hide()
    {
        window.gameObject.SetActive(false);
    }
}
