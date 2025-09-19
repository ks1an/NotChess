using UnityEngine;

public sealed class WaitingWindowController : MonoBehaviour
{
    public static WaitingWindowController Instance;
    [SerializeField] WaitingWindow window;

    void OnEnable()
    {
        Instance = this;
    }

    public void ShowWithRandomTxt()
    {
        window.gameObject.SetActive(true);
        window.SetRandomLoadText();
    }

    public void Hide()
    {
        window.gameObject.SetActive(false);
    }
}
