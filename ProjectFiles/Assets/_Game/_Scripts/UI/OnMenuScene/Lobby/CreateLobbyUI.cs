using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CreateLobbyUI : MonoBehaviour
{
    public static CreateLobbyUI Instance { get; private set; }

    [SerializeField] Button createButton;
    [SerializeField] Button backButton;
    [SerializeField] Button lobbyNameButton;
    [SerializeField] Button publicPrivateButton;
    [SerializeField] Button maxPlayersButton;

    [SerializeField] TextMeshProUGUI lobbyNameText;
    [SerializeField] TextMeshProUGUI publicPrivateText;
    [SerializeField] TextMeshProUGUI maxPlayersText;

    string lobbyName;
    bool isPrivate;
    int maxPlayers;

    void Awake()
    {
        Instance = this;

        #region ButtonActions
        backButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.SetActiveLobbyList(true);
            Hide();
        });

        createButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.CreateLobby(
                lobbyName,
                maxPlayers,
                isPrivate
            );
            Hide();
        });

        lobbyNameButton.onClick.AddListener(() =>
        {
            ModalInputWindow.Instance.Show("Lobby Name", "input name...", () => { },
                (string lobbyName) =>
                {
                    this.lobbyName = lobbyName;
                    UpdateText();
                }, 20);
        });

        publicPrivateButton.onClick.AddListener(() =>
        {
            isPrivate = !isPrivate;
            UpdateText();
        });

        maxPlayersButton.onClick.AddListener(() =>
        {
            ModalInputWindow.Instance.Show("Max Players", "default 2", () => { },
                (string maxPlayers) =>
                {
                    int mp = 0;
                    if (int.TryParse(maxPlayers, out int _i))
                        mp = _i;
                    else
                        mp = 2;

                    if (mp == 0) mp = 2;
                    this.maxPlayers = mp;
                    UpdateText();

                }, validCharacters: "0123456789");
        });
        #endregion

        Hide();
    }

    private void UpdateText()
    {
        lobbyNameText.text = lobbyName;
        publicPrivateText.text = isPrivate ? "Private" : "Public";
        maxPlayersText.text = maxPlayers.ToString();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        LobbyManager.Instance.SetActiveLobbyList(false);

        lobbyName = "MyLobby";
        isPrivate = false;
        maxPlayers = 2;

        UpdateText();
    }
}
