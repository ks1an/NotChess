using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KingData", menuName = "Piece/Create King")]
public class KingData : PieceData
{
    public override List<Vector2Int> GetAvailableMoves(PieceData[,] board)
    {
        List<Vector2Int> r = new();

        #region AttackZone
        Vector2Int workspace = Vector2Int.zero;
        workspace.x = currentX - 1;
        for (int i = -1; i < 2; i++)
        {
            workspace.y = currentY + i;
            if (IsAvailabeMove(ref board, workspace.x, workspace.y))
                r.Add(workspace);
        }

        workspace.x = currentX + 1;
        for (int i = -1; i < 2; i++)
        {
            workspace.y = currentY + i;
            if (IsAvailabeMove(ref board, workspace.x, workspace.y))
                r.Add(workspace);
        }

        workspace.x = currentX;
        for (int i = -1; i < 2; i++)
        {
            workspace.y = currentY + i;
            if (workspace.x == currentX && workspace.y == currentY) continue;
            if (IsAvailabeMove(ref board, workspace.x, workspace.y))
                r.Add(workspace);
        }
        #endregion

        return r;
    }
}
