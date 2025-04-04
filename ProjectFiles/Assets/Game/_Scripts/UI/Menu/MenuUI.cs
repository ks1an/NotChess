using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MenuUI : MonoBehaviour
{
    public void OnSingleplayButton()
    {
        SceneManager.LoadSceneAsync("BoardScene");
        MatchController.Instance.CreateGame(false, 8, 8, 1, 5, true);
    }

    public void OnOnlinePlayButton()
    {
        LobbyManager.Instance.SetActiveLobbyList(true);
    }
}
