using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    None = 0,
    Zero = 1,
    Cross = 2
}

public class Piece : MonoBehaviour
{
    public int currentX, currentY;
    public bool canAttackTeammate;
    public Team team;
    [SerializeField] float durationSetPos = 0.5f;

    public void SetCanAttackTeammate(bool canAttack) => canAttackTeammate = canAttack;

    public virtual void SetPos(Vector3 targetPos, bool force = false)
    {
        transform.DOKill();
        if (force)
            transform.position = targetPos;
        else
            transform.DOMove(targetPos, durationSetPos).SetEase(Ease.InOutBack);
    }

    public List<Vector2Int> GetAvailableMoves(ref Piece[,] board, int countX, int countY)
    {
        List<Vector2Int> r = new();

        #region AttackMove

        if (countX - 1 >= currentX + 1)
        {
            if (countY - 1 >= currentY + 1)
            {
                if (IsAvailabeMove(ref board, currentX + 1, currentY + 1))
                    r.Add(new Vector2Int(currentX + 1, currentY + 1));
            }

            if (0 <= currentY - 1)
            {
                if (IsAvailabeMove(ref board, currentX + 1, currentY - 1))
                    r.Add(new Vector2Int(currentX + 1, currentY - 1));
            }
        }

        if (0 <= currentX - 1)
        {
            if (countY - 1 >= currentY + 1)
            {
                if (IsAvailabeMove(ref board, currentX - 1, currentY + 1))
                    r.Add(new Vector2Int(currentX - 1, currentY + 1));
            }

            if (0 <= currentY - 1)
            {
                if (IsAvailabeMove(ref board, currentX - 1, currentY - 1))
                    r.Add(new Vector2Int(currentX - 1, currentY - 1));
            }
        }

        #endregion

        return r;
    }

    bool IsAvailabeMove(ref Piece[,] board, int x, int y)
    {
        if (board[x, y] != null && Board.Instance.tilesController.tiles[x, y].Stats.CurrentStats.CanAttackTile 
            && (board[x, y].team != team || (canAttackTeammate && board[x, y].team == team)))
            return true;

        return false;
    }
}
