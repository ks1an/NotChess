using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Complexity
{
    easy,
    normal,
    hard
}

[Serializable]
public sealed class EnemyAI : MonoBehaviour
{
    #region Variables
    [Header("General")]
    [SerializeField, Range(0.1f, 5)] float minDelayBeforeDoMove;
    [SerializeField, Range(0, 100)] int chanceOfSkipTheMostValuableMove;

    [Header("Value params")]
    [SerializeField] Complexity complexity;

    [Space(5)]
    [SerializeField, Range(-10, 10)]
    int valueIf_Enemy_VertOrHoriz,
        valueIf_Friend_VertOrHoriz;

    [Space(5)]
    [SerializeField, Range(-10, 10)]
    int valueIf_Enemy_Diagonal,
        valueIf_Friend_Diagonal,
        valueIfBoardBorder;

    [Header("Enemy_LinesValue")]
    [SerializeField, Range(-10, 100)]
    int valueForEach_Enemy_InTheLine;
    [SerializeField, Range(-10, 100)]
    int valueIf_Enemy_lineCompleted_80_procent,
        valueIf_Enemy_lineCompleted_60_procent,
        valueIf_Enemy_lineCompleted_40_procent,
        valueIf_Enemy_lineCompleted_20_procent;
    [SerializeField, Min(0.1f)]
    float ShiftCells_EnemyLine_ValueCoeffic;

    [Header("Friend_LinesValue")]
    [SerializeField, Range(-10, 100)]
    int valueForEach_Friend_InTheLine;
    [SerializeField, Range(-10, 100)]
    int valueIf_Friend_lineCompleted_80_procent,
    valueIf_Friend_lineCompleted_60_procent,
    valueIf_Friend_lineCompleted_40_procent,
    valueIf_Friend_lineCompleted_20_procent;
    [SerializeField, Min(0.1f)]
    float ShiftCells_FriendLine_ValueCoeffic;

    MatchStates states;
    Piece[,] pieces;

    Dictionary<Vector2Int, int> tilesCost = new();
    int tileCountX, tileCountY;
    Team myTeam;
    #endregion

    bool isWorking;
    float DemonstrateChanceOfSkipMove;
    bool isDemontrate;

    readonly MathOperations mathOp = MathOperations.GetInstance();

    #region BeforeStartMyTurn
    public void LoadEnemy(Team enemyTeam)
    {
        isWorking = true;
        DemonstrateChanceOfSkipMove = chanceOfSkipTheMostValuableMove * 2;
        isDemontrate = GameController.Instance.states.isDemonstrationMatchAiVsAi;
        tilesCost.Clear();
        states = GameController.Instance.states;
        tileCountX = GameController.Instance.matchSettings.tileCountX;
        tileCountY = GameController.Instance.matchSettings.tileCountY;
        SetMyTeam(enemyTeam);

        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tilesCost.Add(new Vector2Int(x, y), 0);

        GameController.Instance.states.OnTurnEnded += DoSomeOnTurnEnded;
        GameController.Instance.states.OnGameStarted += IsMyTurnOrNot;
    }

    public void SetMyTeam(Team newTeam) => myTeam = newTeam;

    void IsMyTurnOrNot()
    {
        if ((states.isMoveOfZero && myTeam == Team.Zero)
            || (!states.isMoveOfZero && myTeam == Team.Cross) || states.isDemonstrationMatchAiVsAi)
        {
            if (isWorking)
                StartEnemyTurn();
        }
    }

    void DoSomeOnTurnEnded(int x, int y, Team team) => IsMyTurnOrNot();
    #endregion

    public void StartEnemyTurn()
    {
        pieces = Board.Instance.piecesController.pieces;
        foreach (var tile in tilesCost.ToList())
            tilesCost[tile.Key] = 0;


        if (states.isDemonstrationMatchAiVsAi)
            myTeam = states.isMoveOfZero ? Team.Zero : Team.Cross;

        StartCoroutine(EstimateCostOfTilesAndDoMove());
    }

    #region EstimateCost
    IEnumerator EstimateCostOfTilesAndDoMove()
    {
        EstimateCostLines();
        EstimateCostIndividualTiles();

        int delay = tilesCost.Values.Max();
        if (delay > 10)
            delay = 10;
        yield return new WaitForSeconds(mathOp.GetSafeRandom(minDelayBeforeDoMove, delay / 5));

        DoMove();
    }

    void EstimateCostLines()
    {
        List<Vector2Int> tilesInLine = new(),
            enemyLine = new(),
            friendLine = new(),
            currentLine = new();

        int amount_Enemy_inLine, amount_Friend_inLine;

        #region Check Col
        for (int x = 0; x < tileCountX; x++)
        {
            tilesInLine.Clear();
            enemyLine.Clear();
            friendLine.Clear();
            currentLine.Clear();
            amount_Enemy_inLine = 0;
            amount_Friend_inLine = 0;

            for (int y = 0; y < tileCountY; y++)
            {
                Vector2Int tile = new(x, y);
                Team tileTeam = GetTeamOnTile(x, y);
                tilesInLine.Add(tile);

                if (tileTeam == Team.None)
                {
                    currentLine.Clear();
                    continue;
                }

                if (currentLine.Count > 0 && tileTeam == GetTeamOnTile(currentLine[^1].x, currentLine[^1].y))
                {
                    currentLine.Add(tile);
                }
                else
                {
                    currentLine.Clear();
                    currentLine.Add(tile);
                }

                if (tileTeam == myTeam)
                {
                    amount_Friend_inLine += 1;
                    if (currentLine.Count > friendLine.Count)
                    {
                        friendLine.Clear();
                        friendLine.AddRange(currentLine);
                    }
                }
                else
                {
                    amount_Enemy_inLine += 1;
                    if (currentLine.Count > enemyLine.Count)
                    {
                        enemyLine.Clear();
                        enemyLine.AddRange(currentLine);
                    }
                }
            }
            AddValueForTilesInLine(tilesInLine, enemyLine, friendLine, amount_Friend_inLine, amount_Enemy_inLine);
        }
        #endregion

        #region Check Row
        for (int y = 0; y < tileCountX; y++)
        {
            tilesInLine.Clear();
            enemyLine.Clear();
            friendLine.Clear();
            currentLine.Clear();
            amount_Enemy_inLine = 0;
            amount_Friend_inLine = 0;

            for (int x = 0; x < tileCountY; x++)
            {
                Vector2Int tile = new(x, y);
                Team tileTeam = GetTeamOnTile(x, y);
                tilesInLine.Add(tile);

                if (tileTeam == Team.None)
                {
                    currentLine.Clear();
                    continue;
                }

                if (currentLine.Count > 0 && tileTeam == GetTeamOnTile(currentLine[^1].x, currentLine[^1].y))
                {
                    currentLine.Add(tile);
                }
                else
                {
                    currentLine.Clear();
                    currentLine.Add(tile);
                }

                if (tileTeam == myTeam)
                {
                    amount_Friend_inLine += 1;
                    if (currentLine.Count > friendLine.Count)
                    {
                        friendLine.Clear();
                        friendLine.AddRange(currentLine);
                    }
                }
                else
                {
                    amount_Enemy_inLine += 1;
                    if (currentLine.Count > enemyLine.Count)
                    {
                        enemyLine.Clear();
                        enemyLine.AddRange(currentLine);
                    }
                }
            }

            AddValueForTilesInLine(tilesInLine, enemyLine, friendLine, amount_Friend_inLine, amount_Enemy_inLine);
        }
        #endregion

        // Diagonals LeftoToRight(UP)
        for (int xEdge = 0, yEdge = tileCountY - 1; yEdge >= 0; yEdge--)
            ProcessDiagonal(xEdge, yEdge, 1, 1);
        for (int xEdge = 1, yEdge = 0; xEdge < tileCountX; xEdge++)
            ProcessDiagonal(xEdge, yEdge, 1, 1);

        // Diagonals LeftToRight(Down)
        for (int xEdge = 0, yEdge = tileCountY - 1; yEdge >= 0; yEdge--)
            ProcessDiagonal(xEdge, yEdge, 1, -1);
        for (int xEdge = 1, yEdge = tileCountY - 1; xEdge < tileCountX; xEdge++)
            ProcessDiagonal(xEdge, yEdge, 1, -1);

    }

    void ProcessDiagonal(int startX, int startY, int xStep, int yStep)
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

            if (team == myTeam)
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

                if (diag.Value != myTeam)
                    tilesCost[tile] += valueIf_Enemy_Diagonal;
                else
                    tilesCost[tile] += valueIf_Friend_Diagonal;
            }
            #endregion

            #region CheckTileIsBorder
            if (tile.x == 0 || tile.y == 0 || tile.x == tileCountX - 1 || tile.y == tileCountY - 1)
                tilesCost[tile] += valueIfBoardBorder;
            #endregion
        }
    }

    void AddValueForVertsOrHoriz(int xNeighbourTile, int yNeighbourTile, Vector2Int targetTile)
    {
        if (pieces[xNeighbourTile, yNeighbourTile].team != myTeam)
            tilesCost[targetTile] += valueIf_Enemy_VertOrHoriz;
        else
            tilesCost[targetTile] += valueIf_Friend_VertOrHoriz;
    }

    void AddValueForTilesInLine(List<Vector2Int> generalLine, List<Vector2Int> enemyLine,
        List<Vector2Int> friendLine, int friendCount, int enemyCount)
    {
        foreach (Vector2Int tile in generalLine)
            tilesCost[tile] += (enemyCount * valueForEach_Enemy_InTheLine) +
                (friendCount * valueForEach_Friend_InTheLine);

        if (enemyLine.Count > 1)
            ProccesLineAddValue(enemyLine, true, true, ShiftCells_EnemyLine_ValueCoeffic);

        if (friendLine.Count > 1)
            ProccesLineAddValue(friendLine, false, true, ShiftCells_FriendLine_ValueCoeffic);
    }

    void ProccesLineAddValue(List<Vector2Int> line, bool isEnemyLine, bool needAddShiftValue = false, float shiftValueCoeffic = 1.0f)
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

    #endregion

    void DoMove()
    {
        bool isMoving = true;
        bool canSkipMostValuableByRandom = true;

        foreach (var tile in tilesCost.OrderBy(k => k.Value).Reverse())
        {
            if (isDemontrate && GameController.Instance.states.turnCount == 0)
            {
                EndEnemyTurn(() => states.move.TryCreateUnitOnBoard((int)mathOp.GetSafeRandom(0, tileCountX - 1), (int)mathOp.GetSafeRandom(0, tileCountY - 1), myTeam));
                isMoving = false;
                break;
            }

            if ((canSkipMostValuableByRandom && mathOp.GetSafeRandom(0, 100) <= chanceOfSkipTheMostValuableMove) ||
                (isDemontrate && mathOp.GetSafeRandom(0, 100) <= DemonstrateChanceOfSkipMove))
                continue;

            int x = tile.Key.x;
            int y = tile.Key.y;
            canSkipMostValuableByRandom = false;

            //Tile empty
            if (pieces[x, y] == null)
            {
                if (Board.Instance.tilesController.tiles[x, y].Stats.CurrentStats.CanPutOnTile)
                {
                    EndEnemyTurn(() => states.move.TryCreateUnitOnBoard(x, y, myTeam));
                    isMoving = false;
                    break;
                }
                continue;
            }

            Dictionary<Vector2Int, Team> myDiagonalNeighbours = GetDiagonalNeighborsTeams(x, y);
            //Enemy on tile
            if (pieces[x, y].team != myTeam && Board.Instance.tilesController.tiles[x, y].Stats.CurrentStats.CanAttackTile)
            {
                #region TryToAttackByDiagonal
                Vector2Int bestMove = new();
                int minValue = 1000000000;
                foreach (var neighbour in myDiagonalNeighbours)
                    if (neighbour.Value == myTeam && tilesCost[neighbour.Key] < minValue && tilesCost[neighbour.Key] < tile.Value
                        && Board.Instance.tilesController.tiles[neighbour.Key.x, neighbour.Key.y].Stats.CurrentStats.CanLeaveFromTile)
                    {
                        minValue = tilesCost[neighbour.Key];
                        bestMove = neighbour.Key;
                    }
                if (minValue < 1000000000)
                {
                    isMoving = false;
                    EndEnemyTurn(() => states.move.MoveUnit(bestMove.x, bestMove.y, x, y));
                    break;
                }
                #endregion

                continue;
            }

            //Friend on tile
            if (pieces[x, y].team == myTeam)
            {
                #region TryToCreateFriendUnitOnDiagonal
                Vector2Int bestMove = new();
                int maxValue = -1000000000;
                foreach (var neighbour in myDiagonalNeighbours)
                    if (neighbour.Value == Team.None && tilesCost[neighbour.Key] > maxValue
                        && Board.Instance.tilesController.tiles[neighbour.Key.x, neighbour.Key.y].Stats.CurrentStats.CanPutOnTile)
                    {
                        maxValue = tilesCost[neighbour.Key];
                        bestMove = neighbour.Key;
                    }
                if (maxValue > -1000000000)
                {
                    isMoving = false;
                    EndEnemyTurn(() => states.move.TryCreateUnitOnBoard(bestMove.x, bestMove.y, myTeam));
                    break;
                }
                #endregion

                continue;
            }

        }

        if (isMoving)
        {
            Debug.LogError($"Critical error! Enemy AI could not find an available move.\n Early end of move. My team: {myTeam}");
            EndEnemyTurn(() => { });
        }
    }

    #region Get
    Dictionary<Vector2Int, Team> GetDiagonalNeighborsTeams(int x, int y)
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

    Team GetTeamOnTile(int x, int y)
    {
        if (pieces[x, y] == null)
            return Team.None;
        else
            return pieces[x, y].team;
    }

    public void EndEnemyTurn(Action action)
    {
        StopAllCoroutines();
        action.Invoke();
    }

    public int GetValueForAddInLineCombination(int lineLenght, bool isEnemyLine)
    {
        int needToAddValueOnTile;

        switch ((float)lineLenght / GameController.Instance.matchSettings.piecesWinSequence * 100)
        {
            case >= 80:
                needToAddValueOnTile = isEnemyLine ? valueIf_Enemy_lineCompleted_80_procent : valueIf_Friend_lineCompleted_80_procent;
                break;
            case >= 60:
                needToAddValueOnTile = isEnemyLine ? valueIf_Enemy_lineCompleted_60_procent : valueIf_Friend_lineCompleted_60_procent;
                break;
            case >= 40:
                needToAddValueOnTile = isEnemyLine ? valueIf_Enemy_lineCompleted_40_procent : valueIf_Friend_lineCompleted_40_procent;
                break;
            case >= 20:
                needToAddValueOnTile = isEnemyLine ? valueIf_Enemy_lineCompleted_20_procent : valueIf_Friend_lineCompleted_20_procent;
                break;
            default:
                needToAddValueOnTile = 0;
                break;
        }

        return needToAddValueOnTile;
    }
    #endregion

    public void StopEnemy()
    {
        isWorking = false;
        StopAllCoroutines();
    }

    void OnDisable()
    {
        GameController.Instance.states.OnTurnEnded -= DoSomeOnTurnEnded;
        GameController.Instance.states.OnGameStarted -= IsMyTurnOrNot;
    }
}
