using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KnightData", menuName = "Piece/Create Knight")]
public class KnightData : PieceData
{
    public override List<Vector2Int> GetAvailableMoves(PieceData[,] board)
    {
        List<Vector2Int> r = new();

        #region AttackZone
        Vector2Int workspace = Vector2Int.zero;
        workspace.y = currentY + 2;
        workspace.x = currentX - 1;
        if (IsAvailabeMove(ref board, workspace.x, workspace.y))
            r.Add(workspace);
        workspace.x = currentX + 1;
        if (IsAvailabeMove(ref board, workspace.x, workspace.y))
            r.Add(workspace);

        workspace.x = currentX + 2;
        workspace.y = currentY - 1;
        if (IsAvailabeMove(ref board, workspace.x, workspace.y))
            r.Add(workspace);
        workspace.y = currentY + 1;
        if (IsAvailabeMove(ref board, workspace.x, workspace.y))
            r.Add(workspace);

        workspace.y = currentY - 2;
        workspace.x = currentX - 1;
        if (IsAvailabeMove(ref board, workspace.x, workspace.y))
            r.Add(workspace);
        workspace.x = currentX + 1;
        if (IsAvailabeMove(ref board, workspace.x, workspace.y))
            r.Add(workspace);

        workspace.x = currentX - 2;
        workspace.y = currentY - 1;
        if (IsAvailabeMove(ref board, workspace.x, workspace.y))
            r.Add(workspace);
        workspace.y = currentY + 1;
        if (IsAvailabeMove(ref board, workspace.x, workspace.y))
            r.Add(workspace);
        #endregion

        return r;
    }
}
