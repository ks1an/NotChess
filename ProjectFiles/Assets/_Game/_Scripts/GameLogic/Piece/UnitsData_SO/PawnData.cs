using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PawnData", menuName = "Piece/Create Pawn")]
public class PawnData : PieceData
{
    public override List<Vector2Int> GetAvailableMoves(PieceData[,] board)
    {
        List<Vector2Int> r = new();
        int countX = GameController.Instance.matchSettings.tileCountX,
            countY = GameController.Instance.matchSettings.tileCountY;

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
}
