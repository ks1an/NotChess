using UnityEngine;

public sealed class WaitingWindowController : MonoBehaviour
{
    public static WaitingWindowController Instance;
    [SerializeField] WaitingWindow window;

    void OnEnable()
    {
        Instance = this;
    }

    public void ShowOnNetServicesNotInit()
    {
        window.gameObject.SetActive(true);
        window.SetTxtOnNetServicesNotInit();
    }

    public void ShowWithRandomTxt()
    {
        window.gameObject.SetActive(true);
        window.SetLoadText();
    }

    public void Hide()
    {
        window.gameObject.SetActive(false);
    }
}
