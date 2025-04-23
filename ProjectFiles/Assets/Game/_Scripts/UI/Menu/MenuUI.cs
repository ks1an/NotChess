using UnityEngine;

public sealed class MenuUI : MonoBehaviour
{
    public void OnSingleplayButton()
    {
        MatchController.Instance.CreateGame(false, 8, 8, 1, 5, true);
    }

    public void OnOnlinePlayButton()
    {
        LobbyManager.Instance.SetActiveLobbyList(true);
    }
}
