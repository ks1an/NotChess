using System;
using UnityEngine;

public sealed class ModalViewWindowController : MonoBehaviour
{
    public static ModalViewWindowController Instance;
    public ModalViewWindow modalWindow;
    public bool isInImportantChoice;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void ShowHorizontal(string title, string message, string confirmTxt, string declineTxt, Action greenAction,
        Action redAction, Sprite icon = null, string altTxt = null, Action altAction = null)
    {
        modalWindow.gameObject.SetActive(true);
        modalWindow.ShowHorizontal(title, message, confirmTxt, declineTxt, greenAction, redAction, icon, altTxt, altAction);
    }

   /* public void ShowWarningRestart()
    {
        modalWindow.gameObject.SetActive(true);
        modalWindow.ShowHorizontal("Переиграть?", "Вы уверены, что хотите переиграть раунд? \n Прогресс за этот раунд будет утрачен!",
            "Отмена", "Переиграть", greenAction: CloseModalWindow, redAction: gameBoard.StartGameWithCurrentBoard);
    }*/

    public void ShowCrossWin()
    {
        /*isInImportantChoice = true;
        modalWindow.gameObject.SetActive(true);
        modalWindow.ShowHorizontallNoChoice("<color=#FFD700>Крестики</color> победили!", null, gameBoard.StartGameWithCurrentBoard);*/
    }

    public void ShowZeroWin()
    {
        /*isInImportantChoice = true;
        modalWindow.gameObject.SetActive(true);
        modalWindow.ShowHorizontallNoChoice("<color=#FFD700>Нолики</color> победили!", null, gameBoard.StartGameWithCurrentBoard);*/
    }

    public void CloseModalWindow()
    {
        if (!isInImportantChoice)
            modalWindow.CloseModalWindow();
    }
}
