using System;
using TMPro;
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
            ModalInputWindow.Instance.Show("Player Name", "New name...", () => { },
            (string newName) =>
            {
                playerName = newName;

                playerNameText.text = playerName;

                OnNameChanged?.Invoke(this, EventArgs.Empty);
            },
            20);
        });

        playerNameText.text = playerName;
    }

    void Start()
    {
        OnNameChanged += EditPlayerName_OnNameChanged;
    }

    void EditPlayerName_OnNameChanged(object sender, EventArgs e)
    {
        LobbyManager.Instance.UpdatePlayerName(GetPlayerName());
    }

    public string GetPlayerName()
    {
        return playerName;
    }
}
