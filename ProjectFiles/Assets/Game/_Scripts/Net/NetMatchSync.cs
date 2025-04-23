using Unity.Netcode;
using UnityEngine;

public class NetMatchSync : NetworkBehaviour
{
    bool preStartCalled;
    bool isConnected;
    int readyPlayers;
    int wantRevengePlayers;
    MatchStates states;
    Board board;

    #region BeforePlay

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isConnected = true;
        states = MatchController.Instance.states;
        board = MatchController.Instance.board;

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
        MatchController.Instance.netMatch = GetNetworkObject(id).gameObject.GetComponent<NetMatchSync>();
        if (!preStartCalled)
        {
            states.PreStart();
            preStartCalled = true;
        }
    }

    void OnPreStart()
    {
        #region SetTeam
        if (NetworkManager.ConnectedClientsIds[0] == NetworkManager.LocalClientId) //HostTeam
        {
            if (states.isMoveOfZero)
                MatchController.Instance.player.SetPlayerTeam(Team.Zero);
            else
                MatchController.Instance.player.SetPlayerTeam(Team.Cross);
        }
        else if (NetworkManager.ConnectedClientsIds[1] == NetworkManager.LocalClientId)
        {
            if (states.isMoveOfZero)
                MatchController.Instance.player.SetPlayerTeam(Team.Cross);
            else
                MatchController.Instance.player.SetPlayerTeam(Team.Zero);
        }
        else
            MatchController.Instance.player.SetPlayerTeam(Team.None);
        #endregion

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

    #region DoMove

    [Rpc(SendTo.Server)]
    public void CreateUnitOnBoardRpc(int x, int y, Team team)
    {
        board.piecesController.pieces[x, y] = board.piecesController.GeneratePiece(team);
        board.piecesController.SetPositionSinglePiece(x, y, true);
        NetworkObject netObj = board.piecesController.pieces[x, y].gameObject.GetComponent<NetworkObject>();
        netObj.Spawn();

        UpdateUnitArrayOnClientsRpc(x, y, netObj.NetworkObjectId, team);
        OnTeamMovedRpc(x, y, (int)team);
    }

    [Rpc(SendTo.Server)]
    public void DoUnitMoveRpc(int origX, int origY, int toX, int toY, int team)
    {
        board.piecesController.MoveTo(origX, origY, toX, toY);
        OnTeamMovedRpc(toX, toY, team);
    }

    [Rpc(SendTo.Server)]
    public void SetUnitPosRpc(int x, int y, Vector3 pos, bool instantly = false)
    {
        board.piecesController.pieces[x, y].SetPos(pos, instantly);
    }

    [Rpc(SendTo.Server)]
    public void DestroyUnitRpc(int x, int y)
    {
        Destroy(board.piecesController.pieces[x, y].gameObject);
        UpdateUnitArrayOnClientsRpc(x, y, 0, Team.None);
    }
    #endregion

    #region AfterMove

    [Rpc(SendTo.NotServer)]
    public void UpdateUnitArrayOnClientsRpc(int x, int y, ulong idObj, Team team)
    {
        if (team == Team.None)
        {
            board.piecesController.pieces[x, y] = null;
            return;
        }

        NetworkObject obj = GetNetworkObject(idObj);
        if (obj == null)
        {
            board.piecesController.pieces[x, y] = null;
            return;
        }

        Piece piece = obj.gameObject.GetComponent<Piece>();
        piece.currentX = x;
        piece.currentY = y;
        piece.team = team;
        board.piecesController.pieces[x, y] = piece;
    }

    [Rpc(SendTo.ClientsAndHost)]
    void OnTeamMovedRpc(int x, int y, int teamNum)
    {
        states.TeamMoved(x, y, (Team)teamNum);
    }
    #endregion

    #region EndPlay

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

    void OnHostLeaved() => ModalViewWindowController.Instance.ShowHorizontal(false, "Host left!", "You will have to go to the menu too, bye-bye! <3", 
            "Exit", states.OnLeaveFromMatchTrigger, "Ok(", states.OnLeaveFromMatchTrigger);
    void OnPlayerLeavedGame() => ModalViewWindowController.Instance.ShowHorizontal(false, "Player left!", "You broke him and he ran away! Now go back to the menu, strategist",
            "Exit", states.OnLeaveFromMatchTrigger, "Boo-ha-ha-ha!", states.OnLeaveFromMatchTrigger);
    #endregion

    #region Revenge

    [Rpc(SendTo.Server)]
    public void OfferRevengeRpc()
    {
        wantRevengePlayers++;
        if (wantRevengePlayers == 2) //== maxPlayers
            RevengeRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void RevengeRpc()
    {
        wantRevengePlayers = 0;
        states.GameRestart();
        ModalViewWindowController.Instance.CloseModalWindow(true);
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

    #endregion
}
