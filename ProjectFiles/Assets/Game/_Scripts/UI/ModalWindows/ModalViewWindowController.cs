using System;
using UnityEngine;

public sealed class ModalViewWindowController : MonoBehaviour
{
    public static ModalViewWindowController Instance;
    public ModalViewWindow modalWindow;
    bool dontCloseUntilChoose;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this); 
    }

    public void ShowHorizontal(bool dontCloseUntilChoose, string title, string message, string confirmTxt = null, Action greenAction = null, string declineTxt = null,
        Action redAction = null, string altTxt = null, Action altAction = null, Sprite icon = null, Action doItAnyway = null)
    {
        modalWindow.gameObject.SetActive(true);
        CloseUntilChooseOrNot(dontCloseUntilChoose);

        modalWindow.ShowHorizontal(title, message, confirmTxt, greenAction, declineTxt, redAction, altTxt, altAction, icon, 
            () => 
            {
                CloseUntilChooseOrNot(false);
                doItAnyway?.Invoke();
            });
    }

    void CloseUntilChooseOrNot(bool b) => dontCloseUntilChoose = b;

    public void CloseModalWindow(bool isForceClosure = false)
    {
        if(!dontCloseUntilChoose || isForceClosure)
            modalWindow.CloseModalWindow();
    }
}
