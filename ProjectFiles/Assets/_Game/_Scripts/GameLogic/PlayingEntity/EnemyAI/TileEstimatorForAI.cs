using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class TileEstimatorForAI : MonoBehaviour
{
    EnemyAI_SO data;
    EnemyAI brain;
    Piece[,] pieces;

    readonly MathOperations mathOp = MathOperations.GetInstance();
    Dictionary<Vector2Int, int> tilesCost = new();
    int tileCountX, tileCountY;

    public void Init(EnemyAI brain, EnemyAI_SO dataBase)
    {
        this.brain = brain;
        data = dataBase;

        #region Tiles
        tilesCost.Clear();
        tileCountX = GameController.Instance.matchSettings.tileCountX;
        tileCountY = GameController.Instance.matchSettings.tileCountY;
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tilesCost.Add(new Vector2Int(x, y), 0);
        #endregion 
    }

    public void StartEstimate()
    {
        pieces = Board.Instance.piecesController.pieces;
        foreach (var tile in tilesCost.ToList())
            tilesCost[tile.Key] = 0;
        StartCoroutine(EstimateCostOfTilesAndDoMove());
    }
    public void StopEstimate() { StopAllCoroutines(); }


    IEnumerator EstimateCostOfTilesAndDoMove()
    {
        if (!Board.Instance.landOnBoardReady)
        {
            WaitForSeconds waiting = new(1f);
            while (!Board.Instance.landOnBoardReady)
                yield return waiting;
        }

        EstimateCostLines();
        EstimateCostIndividualTiles();

        int delay = tilesCost.Values.Max();
        if (delay > 10)
            delay = 10;
        yield return new WaitForSeconds(mathOp.GetSafeRandom(data.minDelayBeforeDoMove, delay / 5) + 0.15f);

        brain.DoMove(tilesCost);
    }

    #region EstimateLines
    void EstimateCostLines()
    {
        for (int xEdge = 0, yEdge = tileCountY - 1; yEdge >= 0; yEdge--) //Horizontal
            ProcessLine(xEdge, yEdge, 1, 0);
        for (int xEdge = tileCountX - 1, yEdge = 0; xEdge >= 0; xEdge--) //Vertical
            ProcessLine(xEdge, yEdge, 0, 1);

        // Diagonals LeftoToRight(UP)
        for (int xEdge = 0, yEdge = tileCountY - 1; yEdge >= 0; yEdge--)
            ProcessLine(xEdge, yEdge, 1, 1);
        for (int xEdge = 1, yEdge = 0; xEdge < tileCountX; xEdge++)
            ProcessLine(xEdge, yEdge, 1, 1);

        // Diagonals LeftToRight(Down)
        for (int xEdge = 0, yEdge = tileCountY - 1; yEdge >= 0; yEdge--)
            ProcessLine(xEdge, yEdge, 1, -1);
        for (int xEdge = 1, yEdge = tileCountY - 1; xEdge < tileCountX; xEdge++)
            ProcessLine(xEdge, yEdge, 1, -1);
    }

    void ProcessLine(int startX, int startY, int xStep, int yStep)
    {
        List<Vector2Int> tilesInLine = new(),
            enemyLine = new(),
            friendLine = new(),
            currentLine = new();

        int friendCount = 0, enemyCount = 0;
        Team? previousTeam = null;

        for (int x = startX, y = startY; x < tileCountX && x >= 0 && y < tileCountY && y >= 0; x += xStep, y += yStep)
        {
            Vector2Int tile = new(x, y);
            Team team = GetTeamOnTile(x, y);
            tilesInLine.Add(tile);

            if (team == Team.None)
            {
                currentLine.Clear();
                previousTeam = null;
                continue;
            }

            if (previousTeam == team)
            {
                currentLine.Add(tile);
            }
            else
            {
                currentLine = new List<Vector2Int> { tile };
                previousTeam = team;
            }

            if (team == brain.sensor.myTeam)
            {
                friendCount++;
                if (currentLine.Count > friendLine.Count)
                {
                    friendLine.Clear();
                    friendLine.AddRange(currentLine);
                }
            }
            else
            {
                enemyCount++;
                if (currentLine.Count > enemyLine.Count)
                {
                    enemyLine.Clear();
                    enemyLine.AddRange(currentLine);
                }
            }
        }
        AddValueForTilesInLine(tilesInLine, enemyLine, friendLine, friendCount, enemyCount);
    }

    void ProccesLineAddValue(List<Vector2Int> generalLine, List<Vector2Int> line, bool isEnemyLine, bool needAddShiftValue = false, float shiftValueCoeffic = 1.0f)
    {
        int needToAddValue = GetValueForAddInLineCombination(line.Count, isEnemyLine);
        if (needAddShiftValue && line.Count > 1)
        {
            Vector2Int posShift = new(line[1].x - line[0].x, line[1].y - line[0].y);
            Vector2Int potentialExtentionLine_1;
            Vector2Int potentialExtentionLine_2;

            if (line[0].x < line[^1].x)
            {
                potentialExtentionLine_1 = line[0] - posShift;
                potentialExtentionLine_2 = line[^1] + posShift;
            }
            else if (line[0].x > line[^1].x)
            {
                potentialExtentionLine_1 = line[0] + posShift;
                potentialExtentionLine_2 = line[^1] - posShift;
            }
            else
            {
                if (line[0].y < line[^1].y)
                {
                    potentialExtentionLine_1 = line[0] - posShift;
                    potentialExtentionLine_2 = line[^1] + posShift;
                }
                else
                {
                    potentialExtentionLine_1 = line[0] + posShift;
                    potentialExtentionLine_2 = line[^1] - posShift;
                }
            }

            if (0 <= potentialExtentionLine_1.x && potentialExtentionLine_1.x < tileCountX &&
                0 <= potentialExtentionLine_1.y && potentialExtentionLine_1.y < tileCountY)
                tilesCost[potentialExtentionLine_1] += (int)(needToAddValue * shiftValueCoeffic);

            if (0 <= potentialExtentionLine_2.x && potentialExtentionLine_2.x < tileCountX &&
                 0 <= potentialExtentionLine_2.y && potentialExtentionLine_2.y < tileCountY)
                tilesCost[potentialExtentionLine_2] += (int)(needToAddValue * shiftValueCoeffic);
        }

        foreach (Vector2Int tile in line)
        {
            tilesCost[tile] += needToAddValue;
        }
    }

    public int GetValueForAddInLineCombination(int lineLenght, bool isEnemyLine)
    {
        int needToAddValueOnTile;

        switch ((float)lineLenght / GameController.Instance.matchSettings.piecesWinSequence * 100)
        {
            case >= 80:
                needToAddValueOnTile = isEnemyLine ? data.valueIf_Enemy_lineCompleted_80_procent : data.valueIf_Friend_lineCompleted_80_procent;
                break;
            case >= 60:
                needToAddValueOnTile = isEnemyLine ? data.valueIf_Enemy_lineCompleted_60_procent : data.valueIf_Friend_lineCompleted_60_procent;
                break;
            case >= 40:
                needToAddValueOnTile = isEnemyLine ? data.valueIf_Enemy_lineCompleted_40_procent : data.valueIf_Friend_lineCompleted_40_procent;
                break;
            case >= 20:
                needToAddValueOnTile = isEnemyLine ? data.valueIf_Enemy_lineCompleted_20_procent : data.valueIf_Friend_lineCompleted_20_procent;
                break;
            default:
                needToAddValueOnTile = 0;
                break;
        }

        return needToAddValueOnTile;
    }

    #endregion

    #region EstimateInidividual
    void EstimateCostIndividualTiles()
    {
        foreach (Vector2Int tile in tilesCost.Keys.ToList())
        {
            #region Check Horiz&Vert Tiles
            if (tile.x + 1 < tileCountX && GetTeamOnTile(tile.x + 1, tile.y) != Team.None)
                AddValueForVertsOrHoriz(tile.x + 1, tile.y, tile);

            if (tile.x - 1 >= 0 && GetTeamOnTile(tile.x - 1, tile.y) != Team.None)
                AddValueForVertsOrHoriz(tile.x - 1, tile.y, tile);

            if (tile.y + 1 < tileCountY && GetTeamOnTile(tile.x, tile.y + 1) != Team.None)
                AddValueForVertsOrHoriz(tile.x, tile.y + 1, tile);

            if (tile.y - 1 >= 0 && GetTeamOnTile(tile.x, tile.y - 1) != Team.None)
                AddValueForVertsOrHoriz(tile.x, tile.y - 1, tile);
            #endregion

            #region Check Diagonal Tiles
            var diagonalNeighbour = GetDiagonalNeighborsTeams(tile.x, tile.y);
            foreach (var diag in diagonalNeighbour)
            {
                if (diag.Value == Team.None)
                    continue;

                if (diag.Value != brain.sensor.myTeam)
                    tilesCost[tile] += data.valueIf_Enemy_Diagonal;
                else
                    tilesCost[tile] += data.valueIf_Friend_Diagonal;
            }
            #endregion

            #region CheckTileIsBorder
            if (tile.x == 0 || tile.y == 0 || tile.x == tileCountX - 1 || tile.y == tileCountY - 1)
                tilesCost[tile] += data.valueIfBoardBorder;
            #endregion
        }
    }

    void AddValueForVertsOrHoriz(int xNeighbourTile, int yNeighbourTile, Vector2Int targetTile)
    {
        if (pieces[xNeighbourTile, yNeighbourTile].team != brain.sensor.myTeam)
            tilesCost[targetTile] += data.valueIf_Enemy_VertOrHoriz;
        else
            tilesCost[targetTile] += data.valueIf_Friend_VertOrHoriz;
    }

    void AddValueForTilesInLine(List<Vector2Int> generalLine, List<Vector2Int> enemyLine,
        List<Vector2Int> friendLine, int friendCount, int enemyCount)
    {
        foreach (Vector2Int tile in generalLine)
            tilesCost[tile] += (enemyCount * data.valueForEach_Enemy_InTheLine) +
                (friendCount * data.valueForEach_Friend_InTheLine);

        if (enemyLine.Count > 1)
            ProccesLineAddValue(generalLine, enemyLine, true, true, data.ShiftCells_EnemyLine_ValueCoeffic);

        if (friendLine.Count > 1)
            ProccesLineAddValue(generalLine, friendLine, false, true, data.ShiftCells_FriendLine_ValueCoeffic);
    }
    #endregion

    public Dictionary<Vector2Int, Team> GetDiagonalNeighborsTeams(int x, int y)
    {
        Dictionary<Vector2Int, Team> neighbors = new();

        if (x - 1 >= 0)
        {
            if (y - 1 >= 0)
                neighbors.Add(new Vector2Int(x - 1, y - 1), GetTeamOnTile(x - 1, y - 1));

            if (y + 1 < tileCountY)
                neighbors.Add(new Vector2Int(x - 1, y + 1), GetTeamOnTile(x - 1, y + 1));
        }
        if (x + 1 < tileCountX)
        {
            if (y - 1 >= 0)
                neighbors.Add(new Vector2Int(x + 1, y - 1), GetTeamOnTile(x + 1, y - 1));

            if (y + 1 < tileCountY)
                neighbors.Add(new Vector2Int(x + 1, y + 1), GetTeamOnTile(x + 1, y + 1));
        }

        return neighbors;
    }

    public Team GetTeamOnTile(int x, int y)
    {
        if (pieces[x, y] == null)
            return Team.None;
        else
            return pieces[x, y].team;
    }

}
