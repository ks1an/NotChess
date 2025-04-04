using System;
using Unity.Netcode;
using UnityEngine;

public sealed class NetMatchSync : NetworkBehaviour
{
    public static Action OnNetSpawned;
    MatchStates states;
    Board board;

    #region BeforePlay
    public void Preset()
    {
        DontDestroyOnLoad(this);
        states = MatchController.Instance.states;
        board = MatchController.Instance.board;
        NetworkManager.OnClientStopped += OnPlayerLeavedGame;
    }

    public override void OnNetworkSpawn()
    {
        #region SetTeam
        if (NetworkManager.Singleton.LocalClientId == 0) //HostTeam
        {
            if (states.isMoveOfZero)
                MatchController.Instance.player.SetPlayerTeam(Team.Zero);
            else
                MatchController.Instance.player.SetPlayerTeam(Team.Cross);
        }
        else if (NetworkManager.Singleton.LocalClientId == 1)
        {
            if (states.isMoveOfZero)
                MatchController.Instance.player.SetPlayerTeam(Team.Cross);
            else
                MatchController.Instance.player.SetPlayerTeam(Team.Zero);
        }
        else
            MatchController.Instance.player.SetPlayerTeam(Team.None);
        #endregion

        OnNetSpawned?.Invoke();
    }

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
    void OnPlayerLeavedGame(bool leavePlayerIsHost)
    {
        if (leavePlayerIsHost)
        {
            OnServerLeaveGameRpc();
        }
        else
        {
            OnPlayerLeaveGameRpc();
        }
    }

    [Rpc(SendTo.NotServer)]
    void OnServerLeaveGameRpc()
    {
        ModalViewWindowController.Instance.ShowHorizontal("Host left!", "You will have to go to the menu too, bye-bye :)",
            "Exit", "Ok(", LeaveNetMatch, LeaveNetMatch);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void OnPlayerLeaveGameRpc()
    {
        ModalViewWindowController.Instance.ShowHorizontal("Player left!", "You broke him and he ran away! Now go back to the menu",
    "Exit", "Boo-ha-ha-ha!", LeaveNetMatch, LeaveNetMatch);
    }

    public void LeaveNetMatch()
    {
        NetworkManager.Singleton.Shutdown();
        states.OnLeaveFromMatchTrigger();
    }
    #endregion

    #endregion
}
