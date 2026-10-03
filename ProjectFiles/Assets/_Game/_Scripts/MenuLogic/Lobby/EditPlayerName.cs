using System;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public sealed class EditPlayerName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    private string playerName = "Player45510";


    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            ModalInputWindow.Instance.Show("Set player name", "0<Name length<=20", () => { },
            (string newName) =>
            {
                playerName = newName;

                playerNameText.text = playerName;

                EditPlayerName_OnNameChanged();
            },
            20);
        });
        playerName = GameController.Instance.playerData.PlayerName.Value;
        playerNameText.text = playerName;
    }

    void EditPlayerName_OnNameChanged()
    {
        GameController.Instance.playerData.PlayerName.Value = playerName;

        if(UnityServices.State == ServicesInitializationState.Initialized)
            LobbyManager.Instance.UpdatePlayerName(GetPlayerName());

        GameController.Instance.playerData.SaveData();
    }

    public string GetPlayerName()
    {
        return playerName;
    }
}
