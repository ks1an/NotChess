using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public sealed class MenuUI : MonoBehaviour
{
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
            ModalViewWindowController.Instance.ShowHorizontal(true, "Warning!", "Network services were not implemented. Network functions were limited.", 
                true, null, null, "Ok", () => { });
            lobbyListBttn.interactable = false;
            matchmakingBttn.interactable = false;
        }
    }

    public void TryExitFromApp() => ModalViewWindowController.Instance.ShowHorizontal(false, "See you?", "Do you want to go out \n but promise to come back?",
            true, "I'm staying!", () => ModalViewWindowController.Instance.TryCloseModalViewWindow(), "I'll be back..\nAhem-hem-hem", Application.Quit);
}
