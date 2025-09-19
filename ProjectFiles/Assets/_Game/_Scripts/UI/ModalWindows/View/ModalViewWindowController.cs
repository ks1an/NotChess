using System;
using UnityEngine;
using UnityEngine.Localization;

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

    /// <summary>
    /// Not recommend to use in directly. Use ShowHorizontalWithLocalize()
    /// </summary>
    public void ShowHorizontal(bool dontCloseUntilChoose, string title, string message, bool backInFocus = false,
        string confirmTxt = null, Action greenAction = null, string declineTxt = null,
        Action redAction = null, string altTxt = null, Action altAction = null, Sprite icon = null, Action doItAnyway = null)
    {
        modalWindow.gameObject.SetActive(true);
        CloseUntilChooseOrNot(dontCloseUntilChoose);

        modalWindow.ShowHorizontal(title, message, backInFocus, confirmTxt, greenAction, declineTxt, redAction, altTxt, altAction, icon,
            () =>
            {
                CloseUntilChooseOrNot(false);
                doItAnyway?.Invoke();
            });
    }

    public void ShowHorizontalWithLocalize(LocalizedStringTable localTable, string entryKey,
        bool dontCloseUntilChoose, bool backInFocus = false,
        Action greenAction = null, Action redAction = null, Action altAction = null, Sprite icon = null, Action doItAnyway = null)
    {
        var table = localTable.GetTable();
        if (localTable == null)
        {
            Debug.LogError("Not find table: " + localTable);
            return;
        }
        var title = table.GetEntry(entryKey + "_Title")?.GetLocalizedString();
        var message = table.GetEntry(entryKey + "_Message")?.GetLocalizedString();
        var confirmTxt = table.GetEntry(entryKey + "_Confirm")?.GetLocalizedString();
        var declineTxt = table.GetEntry(entryKey + "_Decline")?.GetLocalizedString();
        var altTxt = table.GetEntry(entryKey + "_Alt")?.GetLocalizedString();

        ShowHorizontal(dontCloseUntilChoose, title, message, backInFocus,
            confirmTxt, greenAction, declineTxt, redAction, altTxt, altAction, icon, doItAnyway);
    }

    public void TryCloseModalViewWindow(bool isForceClosure = false)
    {
        if (!dontCloseUntilChoose || isForceClosure)
            modalWindow.CloseModalWindow();
    }

    void CloseUntilChooseOrNot(bool b) => dontCloseUntilChoose = b;
}
