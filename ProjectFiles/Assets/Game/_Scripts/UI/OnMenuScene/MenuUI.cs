using System;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class MenuUI : MonoBehaviour
{
    [SerializeField] Button lobbyListBttn, matchmakingBttn, exitFromAppBttn;

    void Awake()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            matchmakingBttn.onClick.AddListener(LobbyManager.Instance.QuickJoinLobby);
            exitFromAppBttn.onClick.AddListener(() =>
                ModalViewWindowController.Instance.ShowHorizontal(false, "See you?", "Do you want to go out \n but promise to come back?",
                true, "I'm staying!",() => ModalViewWindowController.Instance.TryCloseModalViewWindow(), "I'll be back..\nAhem-hem-hem", Application.Quit)
                );

            lobbyListBttn.interactable = true;
            matchmakingBttn.interactable = true;
        }
        else
        {
            lobbyListBttn.interactable = false;
            matchmakingBttn.interactable = false;
        }
    }

    public void OnSingleplayButton() => MatchController.Instance.CreateGame(false);
    public void OnLobbyListBttnClicked() => LobbyManager.Instance.SetActiveLobbyList(true);
}
