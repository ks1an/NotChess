using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public sealed class LobbyListUI : MonoBehaviour
{
    public static LobbyListUI Instance { get; private set; }

    [SerializeField] Transform lobbySingleTemplate;
    [SerializeField] Transform container;
    [SerializeField] Button refreshButton, createLobbyButton;
    [SerializeField] Button exitLobbyListBttn, quickJoinLobbyBttn;


    void Awake()
    {
        Instance = this;

        lobbySingleTemplate.gameObject.SetActive(false);

        exitLobbyListBttn.onClick.AddListener(Hide);
        refreshButton.onClick.AddListener(LobbyManager.Instance.RefreshLobbyList);
        createLobbyButton.onClick.AddListener(CreateLobbyUI.Instance.Show);
        quickJoinLobbyBttn.onClick.AddListener(LobbyManager.Instance.QuickJoinLobby);
    }

    void Start()
    {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnMatchmakerCancelled += LobbyManager_OnMatchmakerCancelled;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;

        Hide();
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e)
    {
        Show();
    }

    void LobbyManager_OnMatchmakerCancelled(object sender, EventArgs e)
    {
        Hide();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        try
        {
            foreach (Transform child in container)
            {
                if (child == lobbySingleTemplate) continue;

                Destroy(child.gameObject);
            }

            foreach (Lobby lobby in lobbyList)
            {
                Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
                lobbySingleTransform.gameObject.SetActive(true);
                LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
                lobbyListSingleUI.UpdateLobby(lobby);
            }
        }
        catch (Exception) { }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
        LobbyManager.Instance.RefreshLobbyList();
    }
}
