using System.Collections.Generic;
using UnityEngine;

public partial class MoveExecutor : MonoBehaviour
{
    Board board;
    MatchStates match;
    NetMatchSync net;

    public void SetSettings(MatchStates match)
    {
        this.match = match;
        UpdateNet();
        board = GameController.Instance.board;
    }

    public void UpdateNet()
    {
        if (match.isNetMatch)
        {
            net = GameController.Instance.netMatch;
        }
    }

    public void UseCard(int cardID, List<Vector2Int> moves, Team teamWhoUsed)
    {
        if (match.isNetMatch)
        {
            int[] movesX = new int[moves.Count],
                movesY = new int[moves.Count];
            for (int i = 0; i < moves.Count; i++)
            {
                movesX[i] = moves[i][0];
                movesY[i] = moves[i][1];
            }

            net.cardSync.UseCardRpc(cardID, movesX, movesY, (int)teamWhoUsed);
        }
        if(BoardUI.Singleton != null)
            BoardUI.Singleton.muligan.OnUsedCard();
    }
}

//Do UNITS
public partial class MoveExecutor
{
    public void TryCreateUnitOnBoard(int x, int y, Team teamWhoMoved)
    {
        if (board.piecesController.pieces[x, y] != null)
            return;

        if (match.isNetMatch)
        {
            net.unitSync.CreateUnitOnBoardRpc(x, y, teamWhoMoved);
            return;
        }

        board.piecesController.pieces[x, y] = board.piecesController.GeneratePiece(teamWhoMoved);
        board.piecesController.SetPositionSinglePiece(x, y, true);
        match.TeamMoved(x, y, teamWhoMoved);
    }

    public void TryDestroyUnit(int x, int y, bool destroyedByUnit, Team teamWhoDestroyed)
    {
        if (teamWhoDestroyed == Team.None)
        {
            Debug.LogError("Player with NONE team trying destroy unit");
            return;
        }

        if (board.piecesController.pieces[x, y] != null)
            if (match.isNetMatch)
            {
                net.unitSync.DestroyUnitRpc(x, y, destroyedByUnit, (int)teamWhoDestroyed);
                return;
            }
            else
            {
                if (destroyedByUnit)
                {
                    if(teamWhoDestroyed == GameController.Instance.player.GetLocalPlayerTeam())
                        GameController.Instance.player.IncreaseMana(GameController.Instance.matchSettings.manaForDestroyEnemy);
                    else
                        GameController.Instance.enemy.IncreaseMana(GameController.Instance.matchSettings.manaForDestroyEnemy);
                }

                Destroy(board.piecesController.pieces[x, y].gameObject);
            }
    }

    public void MoveUnit(int originalX, int originalY, int toX, int toY)
    {
        Team team = board.piecesController.pieces[originalX, originalY].team;
        if (match.isNetMatch)
        {
            net.unitSync.DoUnitMoveRpc(originalX, originalY, toX, toY, (int)team);
            return;
        }

        board.piecesController.MoveTo(originalX, originalY, toX, toY);
        match.TeamMoved(toX, toY, team);
    }

    public void SetUnitPos(int x, int y, Vector3 pos, bool instantly = false)
    {
        if (match.isNetMatch)
        {
            net.unitSync.SetUnitPosRpc(x, y, pos, instantly);
            return;
        }

        board.piecesController.pieces[x, y].SetPos(pos, instantly);
    }
}
