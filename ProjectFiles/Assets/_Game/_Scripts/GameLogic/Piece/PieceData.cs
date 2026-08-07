using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    None = 0,
    Zero = 1,
    Cross = 2
}

public abstract class PieceData : ScriptableObject
{
    public bool canAttackTeammate;
    public float durationSetPos = 0.5f;

    [HideInInspector] public PieceView view;
    [HideInInspector] public int currentX, currentY;
    [HideInInspector] public Team team;

    public abstract List<Vector2Int> GetAvailableMoves(PieceData[,] board);

    #region Set
    public void SetCanAttackTeammate(bool canAttack) => canAttackTeammate = canAttack;
    public void SetPos(Vector3 targetPos, bool force = false) { if (view != null) view.SetPos(targetPos, force); }
    #endregion

    public virtual bool IsAvailabeMove(ref PieceData[,] board, int x, int y)
    {
        if (x < 0 || y < 0 ||
    x > GameController.Instance.matchSettings.tileCountX - 1 || y > GameController.Instance.matchSettings.tileCountY - 1)
            return false;

        if (board[x, y] != null && Board.Instance.tilesController.tiles[x, y].Stats.CurrentStats.CanAttackTile
            && (board[x, y].team != team || (canAttackTeammate && board[x, y].team == team)))
            return true;

        return false;
    }
}
