using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public partial class MenuUI : MonoBehaviour
{
    [SerializeField] LocalizedStringTable localTable;
    [SerializeField] Button lobbyListBttn, matchmakingBttn, singleplay, myDeckBttn;
    [SerializeField] TabMenu tabMenu;
    //[SerializeField] MenuBook menuBook;

    void Awake()
    {
        SetActiveTabMenu(false);
        /*menuBook.OnBackClosed += () => 
        {
            menuBook.ClosedFront();
            menuBook.gameObject.transform.DOLocalMoveX(100, 2f)
            .OnComplete(() => { menuBook.gameObject.SetActive(false); });
            SetActiveMainMenuUI(true);
        };
        menuBook.gameObject.SetActive(false);*/

        Application.targetFrameRate = GameController.Instance.gameSettings.TargetFrameRate_Menu.Value;
        GameController.Instance.CreateDemostrationGame();

        singleplay.onClick.AddListener(() => GameController.Instance.CreateGame(MatchStates.GameMode_SinglePlayer, false));
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsAuthorized)
        {
            matchmakingBttn.onClick.AddListener(() =>
            {
                WaitingWindowController.Instance.Show("Matchmaking",
                    onBttnExit: () => {LobbyManager.Instance.CancelMatchmaker();}, enableTimer: true);
                LobbyManager.Instance.Matchmaker();
            });
            lobbyListBttn.onClick.AddListener(() => LobbyManager.Instance.SetActiveLobbyList(true));

            lobbyListBttn.interactable = true;
            matchmakingBttn.interactable = true;
        }
        else
        {
            ModalViewWindowController.Instance.ShowHorizontalWithLocalize
                (
                    localTable, "NetworkServicesNotInit",
                    true, true, null, () => { }
                );
            lobbyListBttn.interactable = false;
            matchmakingBttn.interactable = false;
        }

        settingsBttn.onClick.AddListener(() =>
        {
            SetActiveTabMenu(true);
            tabMenu.ActiveSettingsPage();
        });

        if (Application.isMobilePlatform) Destroy(leaveBttn.gameObject);
        else
        {
            leaveBttn.onClick.AddListener(() =>
            {
                TryExitFromApp();
            });
        }
    }

    public void TryExitFromApp() => ModalViewWindowController.Instance.ShowHorizontalWithLocalize
        (
        localTable, "TryExitFromApp",
        false, true, () => ModalViewWindowController.Instance.TryCloseModalViewWindow(), Application.Quit
        );

    /*public void OpenBookMenu_SettingsPage()
    {
        if (!menuBook.gameObject.activeSelf)
        {
            SetActiveMainMenuUI(false);
            menuBook.gameObject.transform.DOKill();
            menuBook.gameObject.SetActive(true);
            menuBook.gameObject.transform.DOMoveX(0.5f, 0.35f);
            menuBook.OpenPage(4);
        }
    }*/

    public void SetActiveTabMenu(bool enableTab)
    {
        if (enableTab)
        {
            tabMenu.gameObject.SetActive(enableTab);
        }
        else if(tabMenu.gameObject.activeSelf)
        {
            tabMenu.DisableAllTab();
        }

        SetActiveMainMenuUI(!enableTab);
    }

    public void SetActiveMainMenuUI(bool b)
    {
        lobbyListBttn.gameObject.SetActive(b);
        matchmakingBttn.gameObject.SetActive(b);
        singleplay.gameObject.SetActive(b);
        gameTitle.gameObject.SetActive(b);
        if(leaveBttn != null) leaveBttn.gameObject.SetActive(b);
        settingsBttn.gameObject.SetActive(b);
        PlayerField.SetActive(b);
        MMRView.SetActive(b);
        myDeckBttn.gameObject.SetActive(b);
        //menuBook.gameObject.SetActive(false);
    }
}


//SetActive 
public partial class MenuUI
{
    [SerializeField] TextMeshProUGUI gameTitle;
    [SerializeField] Button leaveBttn, settingsBttn;
    [SerializeField] GameObject PlayerField, MMRView;
}