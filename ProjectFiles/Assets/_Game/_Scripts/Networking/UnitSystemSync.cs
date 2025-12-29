using Unity.Netcode;
using UnityEngine;

public sealed class UnitSystemSync : NetworkBehaviour
{
    Board board;
    NetMatchSync net;
    public void SetSettings()
    {
        board = Board.Instance;
        net = GameController.Instance.netMatch;
    }

    /////////DO MOVE
    [Rpc(SendTo.Server)]
    public void CreateUnitOnBoardRpc(int x, int y, Team team)
    {
        Debug.LogError("CREATE UNIT");
        if ((team == Team.Zero && !GameController.Instance.states.isMoveOfZero) ||
            (team == Team.Cross && GameController.Instance.states.isMoveOfZero))
            return;
        board.piecesController.pieces[x, y] = board.piecesController.GeneratePiece(team);
        board.piecesController.SetPositionSinglePiece(x, y, true);
        NetworkObject netObj = board.piecesController.pieces[x, y].gameObject.GetComponent<NetworkObject>();
        netObj.Spawn();
        Debug.LogError("CREATED UNIT");

        UpdateUnitArrayOnClientsRpc(x, y, netObj.NetworkObjectId, team);
        net.OnTeamMovedRpc(x, y, (int)team);
    }

    [Rpc(SendTo.Server)]
    public void DoUnitMoveRpc(int origX, int origY, int toX, int toY, int team)
    {
        board.piecesController.MoveTo(origX, origY, toX, toY);
        net.OnTeamMovedRpc(toX, toY, team);
    }

    [Rpc(SendTo.Server)]
    public void SetUnitPosRpc(int x, int y, Vector3 pos, bool instantly = false)
    {
        board.piecesController.pieces[x, y].SetPos(pos, instantly);
    }

    [Rpc(SendTo.Server)]
    public void DestroyUnitRpc(int x, int y, bool destroyedByUnit, int teamWhoDestroy)
    {
        if (destroyedByUnit)
            IncreaseManaForDestroyRpc(teamWhoDestroy);

        Destroy(board.piecesController.pieces[x, y].gameObject);
        UpdateUnitArrayOnClientsRpc(x, y, 0, Team.None);
    }

    ////////////AfterDoMove
    [Rpc(SendTo.ClientsAndHost)]
    public void IncreaseManaForDestroyRpc(int playerWhoDestroy)
    {
        int playerTeam = (int)GameController.Instance.player.GetLocalPlayerTeam();
        if (playerTeam == playerWhoDestroy && playerTeam != (int)Team.None)
        {
            GameController.Instance.player.IncreaseMana(GameController.Instance.matchSettings.manaForDestroyEnemy);
        }
    }

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
}
