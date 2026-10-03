using System;
using UnityEngine;

public sealed class WaitingWindowController : MonoBehaviour
{
    public static WaitingWindowController Instance;
    [SerializeField] WaitingWindow window;

    void OnEnable()
    {
        Instance = this;
    }

    public void Show(string title = null, string content = null, 
        Action onBttnExit = null, bool enableTimer = false)
    {
        window.gameObject.SetActive(true);
        window.SetEnableWaitingWindow(title, content, onBttnExit, enableTimer);
    }

    public void SwitchRndLoadTxt() => window.SwitchRandomLoadTxt();

    public void Hide()
    {
        window.gameObject.SetActive(false);
    }
}
