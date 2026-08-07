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
    public void CreateUnitOnBoardRpc(int x, int y, Team team, string prefabId, bool needToSkipMove = true)
    {
        board.piecesController.pieces[x, y] = board.piecesController.GeneratePiece(team, prefabId);
        board.piecesController.SetPositionSinglePiece(x, y, true);
        NetworkObject netObj = board.piecesController.pieces[x, y].view.gameObject.GetComponent<NetworkObject>();
        netObj.Spawn();

        UpdateUnitArrayOnClientsRpc(x, y, netObj.NetworkObjectId, team);
        if (needToSkipMove)
            net.OnTeamMovedRpc(x, y, (int)team);
    }

    [Rpc(SendTo.Server)]
    public void TryDestroyAndCreateUnitRpc(int x, int y, bool destroyedByUnit, Team team, string uniquePrefabId, bool needToSkipMove = true)
    {
        DestroyUnitRpc(x, y, destroyedByUnit, (int)team, false, false);
        if (board.piecesController.pieces[x, y] == null)
            CreateUnitOnBoardRpc(x, y, team, uniquePrefabId, needToSkipMove);
        else
            Debug.LogError($"Cant place unit (id: {uniquePrefabId}) on {x},{y} because tile have unit");
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
    public void DestroyUnitRpc(int x, int y, bool destroyedByUnit, int teamWhoDestroy, bool needAddMana = false, bool needAddGraveCoin = true)
    {
        if (destroyedByUnit && needAddMana)
            IncreaseManaForDestroyRpc(teamWhoDestroy);
        if (needAddGraveCoin)
            IncreaseGraveCoinForDestroyRpc(teamWhoDestroy);

        Destroy(board.piecesController.pieces[x, y].view.gameObject);
        board.piecesController.pieces[x, y] = null;
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

    [Rpc(SendTo.ClientsAndHost)]
    public void IncreaseGraveCoinForDestroyRpc(int playerWhoDestroy)
    {
        int playerTeam = (int)GameController.Instance.player.GetLocalPlayerTeam();
        if (playerTeam == playerWhoDestroy && playerTeam != (int)Team.None)
        {
            GameController.Instance.player.IncreaseGraveTokens(GameController.Instance.matchSettings.graveTokensForDestroyEnemy);
        }
    }

    //
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

        PieceView view = obj.gameObject.GetComponent<PieceView>();
        view.Init();
        PieceData piece = view.runtimeData;
        piece.currentX = x;
        piece.currentY = y;
        piece.team = team;
        board.piecesController.pieces[x, y] = piece;
    }
}
