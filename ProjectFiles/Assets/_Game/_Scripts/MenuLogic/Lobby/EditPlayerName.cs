using System;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public sealed class EditPlayerName : MonoBehaviour
{
    public static EditPlayerName Instance { get; private set; }


    public event EventHandler OnNameChanged;


    [SerializeField] private TextMeshProUGUI playerNameText;


    private string playerName = "Player45510";


    void Awake()
    {
        Instance = this;

        GetComponent<Button>().onClick.AddListener(() =>
        {
            ModalInputWindow.Instance.Show("Set player name", "0<Name<=20", () => { },
            (string newName) =>
            {
                playerName = newName;

                playerNameText.text = playerName;

                OnNameChanged?.Invoke(this, EventArgs.Empty);
            },
            20);
        });

        playerNameText.text = playerName;
        OnNameChanged += EditPlayerName_OnNameChanged;
    }

    void EditPlayerName_OnNameChanged(object sender, EventArgs e)
    {
        if(UnityServices.State == ServicesInitializationState.Initialized)
            LobbyManager.Instance.UpdatePlayerName(GetPlayerName());
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    void OnDisable()
    {
        OnNameChanged -= EditPlayerName_OnNameChanged;
    }
}
