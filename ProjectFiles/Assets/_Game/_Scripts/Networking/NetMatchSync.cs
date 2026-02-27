using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization;

public class NetMatchSync : NetworkBehaviour
{
    [SerializeField] LocalizedStringTable localTable;
    [HideInInspector] public CardSystemSync cardSync;
    [HideInInspector] public UnitSystemSync unitSync;
    [HideInInspector] public LandSync landSync;
    MatchStates states;

    bool preStartCalled;
    bool isConnected;
    bool isWantedToRevenge;
    int readyPlayers;
    int wantRevengePlayers;

    void Awake()
    {
        if (cardSync == null)
            cardSync = gameObject.AddComponent<CardSystemSync>();
        if (unitSync == null)
            unitSync = gameObject.AddComponent<UnitSystemSync>();
        if(landSync == null)
            landSync = gameObject.AddComponent<LandSync>();
    }

    #region BeforePlay

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isConnected = true;
        states = GameController.Instance.states;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += DisconnectedRpc;
        }
        states.OnGamePreStart += OnPreStart;

        GetNetMatchSyncRpc(NetworkObject.NetworkObjectId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void GetNetMatchSyncRpc(ulong id)
    {
        GameController.Instance.netMatch = GetNetworkObject(id).gameObject.GetComponent<NetMatchSync>();
        if (!preStartCalled)
        {
            states.PreStart();
            preStartCalled = true;
        }
    }

    void OnPreStart()
    {
        unitSync.SetSettings();
        isWantedToRevenge = false;

        #region SetTeam
        if (NetworkManager.ConnectedClientsIds[0] == NetworkManager.LocalClientId) //HostTeam
        {
            if (states.isMoveOfZero)
                GameController.Instance.player.SetPlayerTeam(Team.Zero);
            else
                GameController.Instance.player.SetPlayerTeam(Team.Cross);
        }
        else if (NetworkManager.ConnectedClientsIds[1] == NetworkManager.LocalClientId)
        {
            if (states.isMoveOfZero)
                GameController.Instance.player.SetPlayerTeam(Team.Cross);
            else
                GameController.Instance.player.SetPlayerTeam(Team.Zero);
        }
        else
            GameController.Instance.player.SetPlayerTeam(Team.None);
        #endregion

        readyPlayers = 0;
        ReadyToStartRpc();
    }

    [Rpc(SendTo.Server)]
    void ReadyToStartRpc()
    {
        readyPlayers++;
        if (readyPlayers >= 2)
            AllPlayersReadyRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void AllPlayersReadyRpc() => states.GameStart();

    #endregion

    [Rpc(SendTo.ClientsAndHost)]
    public void OnTeamMovedRpc(int x, int y, int teamNum)
    {
        states.TeamMoved(x, y, (Team)teamNum);
    }

    [Rpc(SendTo.NotMe)]
    public void OnPlayerManaChangeRpc(int mana)
    {
        if(mana >= 0)
            GameController.Instance.enemy.IncreaseMana(mana);
        else
            GameController.Instance.enemy.DeacreaseMana(mana*-1);
    }

    #region OnLeaved
    public void LeaveNetMatch()
    {
        isConnected = false;

        if (IsServer)
        {
            DisconnectedRpc(NetworkManager.ServerClientId);
            NetworkManager.Singleton.OnClientDisconnectCallback -= DisconnectedRpc;
        }

        states.OnGamePreStart -= OnPreStart;
        preStartCalled = false;
        readyPlayers = 0;
        wantRevengePlayers = 0;

        NetworkManager.Singleton.Shutdown();
    }

    void OnHostLeaved() =>
        ModalViewWindowController.Instance.ShowHorizontalWithLocalize(
            localTable, "HostLeft",
            false, false, states.OnLeaveFromMatchTrigger, states.OnLeaveFromMatchTrigger
            );

    void OnPlayerLeavedGame() =>
        ModalViewWindowController.Instance.ShowHorizontalWithLocalize(
            localTable, "PlayerLeft",
            false, false, states.OnLeaveFromMatchTrigger, states.OnLeaveFromMatchTrigger
            );
    #endregion

    #region Revenge
    public void OfferRevenge()
    {
        if (isWantedToRevenge) return;
        isWantedToRevenge = true;
        OfferRevengeRpc();
    }

    [Rpc(SendTo.Server)]
    void OfferRevengeRpc()
    {
        wantRevengePlayers++;
        if (wantRevengePlayers == 2) //== maxPlayers
            RevengeRpc();
        else
            NotificateRevengeRpc(wantRevengePlayers);
    }

    [Rpc(SendTo.Everyone)]
    void NotificateRevengeRpc(int wantRevengePlayers)
    {
        if (isWantedToRevenge)
            NotificationPanelConroller.Instance.ShowNotification($"{wantRevengePlayers}/{2} ready for a rematch", () => { });
        else
            NotificationPanelConroller.Instance.ShowNotification($"{wantRevengePlayers}/{2} ready for a rematch", () => states.RevengeOffer());
    }

    [Rpc(SendTo.ClientsAndHost)]
    void RevengeRpc()
    {
        isWantedToRevenge = false;
        wantRevengePlayers = 0;
        states.GameRestart();
        ModalViewWindowController.Instance.TryCloseModalViewWindow(true);
    }
    #endregion

    [Rpc(SendTo.ClientsAndHost)]
    void DisconnectedRpc(ulong IdOfDisconnectedClient)
    {
        if (!isConnected)
            return;

        if (IdOfDisconnectedClient == NetworkManager.ServerClientId)
            OnHostLeaved();
        else
            OnPlayerLeavedGame();
    }
}
