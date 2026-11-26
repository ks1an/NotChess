using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public partial class MenuUI : MonoBehaviour
{
    [SerializeField] LocalizedStringTable localTable;
    [SerializeField] Button lobbyListBttn, matchmakingBttn, singleplay;
    [SerializeField] TabMenu tabMenu;
    [SerializeField] DailyQuestsWindow dailyQuestsWindow;

    void Awake()
    {
        SetActiveTabMenu(false);

        Application.targetFrameRate = GameController.Instance.gameSettings.TargetFrameRate_Menu.Value;

        if (GameController.Instance.playerData.TutorialCompleted)
            GameController.Instance.CreateDemostrationGame();
        else
            GameController.Instance.StartTutorial();


        singleplay.onClick.AddListener(() => GameController.Instance.CreateGame(MatchStates.GameMode_SinglePlayer, false));
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsAuthorized)
        {
            matchmakingBttn.onClick.AddListener(() =>
            {
                WaitingWindowController.Instance.Show("Matchmaking",
                    onBttnExit: () => { LobbyManager.Instance.CancelMatchmaker(); }, enableTimer: true);
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

        questsBttn.onClick.AddListener(() =>
        {
            SetActiveMainMenuUI(false);
            SetActiveQuestMenu(true);
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

    public void SetActiveTabMenu(bool enableTab)
    {
        if (enableTab)
        {
            tabMenu.gameObject.SetActive(enableTab);
        }
        else if (tabMenu.gameObject.activeSelf)
        {
            tabMenu.DisableAllTab();
        }

        SetActiveMainMenuUI(!enableTab);
    }

    public void SetActiveMainMenuUI(bool b)
    {
        UpperLeftGroup.SetActive(b);
        downLeftGroup.SetActive(b);
        downRightGroup.SetActive(b);
        upperRightGroup.SetActive(b);

        middleLeftGroup.SetActive(b);
        if (b) curDeckView.UpdateView();
    }

    public void SetActiveQuestMenu(bool b)
    {
        dailyQuestsWindow.gameObject.SetActive(b);
    }
}


//SetActive 
public partial class MenuUI
{
    [SerializeField] Button leaveBttn, settingsBttn, questsBttn;
    [SerializeField] GameObject UpperLeftGroup, downLeftGroup, downRightGroup, upperRightGroup, middleLeftGroup;
    [SerializeField] ResourceViewGroup resourceViewGroup;
    [SerializeField] CurrentSelectedDeckView curDeckView;
}