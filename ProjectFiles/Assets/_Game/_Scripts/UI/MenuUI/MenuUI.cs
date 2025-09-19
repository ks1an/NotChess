using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public sealed class MenuUI : MonoBehaviour
{
    [SerializeField] LocalizedStringTable localTable;
    [SerializeField] Button lobbyListBttn, matchmakingBttn, singleplay;

    void Awake()
    {
        GameController.Instance.CreateDemostrationGame();
        singleplay.onClick.AddListener(() => GameController.Instance.CreateGame(false));
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsAuthorized)
        {
            matchmakingBttn.onClick.AddListener(LobbyManager.Instance.QuickJoinLobby);
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
    }

    public void TryExitFromApp() => ModalViewWindowController.Instance.ShowHorizontalWithLocalize
        (
        localTable, "TryExitFromApp",
        false, true, () => ModalViewWindowController.Instance.TryCloseModalViewWindow(), Application.Quit
        );
}
