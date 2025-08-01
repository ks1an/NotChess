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

    [Space(5)]
    [SerializeField, Range(-10, 100)]
    int valueForEach_Enemy_InTheLine,
        valueForEach_Friend_InTheLine;

    MatchStates states;
    Piece[,] pieces;

    public Dictionary<Vector2Int, int> tilesCost = new();
    int tileCountX, tileCountY;
    Team myTeam;
    #endregion

    #region BeforeStartMyTurn
    public void LoadEnemy(Team enemyTeam)
    {
        tilesCost.Clear();
        states = GameController.Instance.states;
        tileCountX = GameController.Instance.settings.tileCountX;
        tileCountY = GameController.Instance.settings.tileCountY;
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
            || (!states.isMoveOfZero && myTeam == Team.Cross) || states.isMatchAiVsAi)
        {
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


        if (states.isMatchAiVsAi)
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
        yield return new WaitForSeconds(MathOperations.GetInstance().GetRandom(minDelayBeforeDoMove, delay / 5));

        DoMove();
    }

    void EstimateCostLines()
    {
        List<Vector2Int> tilesInLine = new(),
maxTiles_Enemy_lineInLine = new(),
maxTiles_Friend_lineInLine = new();

        int amount_Enemy_inLine,
            amount_Friend_inLine;

        #region Check Col
        for (int x = 0; x < tileCountX; x++)
        {
            tilesInLine.Clear();
            maxTiles_Enemy_lineInLine.Clear();
            maxTiles_Friend_lineInLine.Clear();
            amount_Enemy_inLine = 0;
            amount_Friend_inLine = 0;
            int maxFriendInLine = 0,
                maxEnemyInLine = 0;

            for (int y = 0; y < tileCountY; y++)
            {
                tilesInLine.Add(new Vector2Int(x, y));
                Team tileTeam = GetTeamOnTile(x, y);

                if (tileTeam == Team.None)
                {

                }
                else if (tileTeam == myTeam)
                {
                    amount_Friend_inLine += 1;
                }
                else
                {
                    amount_Enemy_inLine += 1;
                }
            }

            AddValueForTilesInLine(tilesInLine, maxTiles_Enemy_lineInLine, maxTiles_Friend_lineInLine, amount_Friend_inLine, amount_Enemy_inLine);
        }
        #endregion
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

    void AddValueForTilesInLine(List<Vector2Int> tilesInLine, List<Vector2Int> maxTiles_Enemy_lineInLine, 
        List<Vector2Int> maxTiles_Friend_lineInLine, int amountFriend, int amoutEnemy)
    {
        foreach (Vector2Int tile in tilesInLine)
        {
            tilesCost[tile] += (amoutEnemy * valueForEach_Enemy_InTheLine) +
                (amountFriend * valueForEach_Friend_InTheLine);
        }
    }

    #endregion

    void DoMove()
    {
        bool isMoving = true;
        foreach (var tile in tilesCost.OrderBy(k => k.Value).Reverse())
        {
            int x = tile.Key.x;
            int y = tile.Key.y;

            //Tile empty
            if (pieces[x, y] == null)
            {
                EndEnemyTurn(() => states.TryCreateUnitOnBoard(x, y, myTeam));
                isMoving = false;
                break;
            }

            Dictionary<Vector2Int, Team> myDiagonalNeighbours = GetDiagonalNeighborsTeams(x, y);
            //Enemy on tile
            if (pieces[x, y].team != myTeam)
            {
                #region TryToAttackByDiagonal
                Vector2Int bestMove = new();
                int minValue = 1000000000;
                foreach (var neighbour in myDiagonalNeighbours)
                    if (neighbour.Value == myTeam && tilesCost[neighbour.Key] < minValue && tilesCost[neighbour.Key] < tile.Value)
                    {
                        minValue = tilesCost[neighbour.Key];
                        bestMove = neighbour.Key;
                    }
                if (minValue < 1000000000)
                {
                    isMoving = false;
                    EndEnemyTurn(() => states.MoveUnit(bestMove.x, bestMove.y, x, y));
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
                    if (neighbour.Value == Team.None && tilesCost[neighbour.Key] > maxValue)
                    {
                        maxValue = tilesCost[neighbour.Key];
                        bestMove = neighbour.Key;
                    }
                if (maxValue > -1000000000)
                {
                    isMoving = false;
                    EndEnemyTurn(() => states.TryCreateUnitOnBoard(bestMove.x, bestMove.y, myTeam));
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
    #endregion

    private void OnDisable()
    {
        GameController.Instance.states.OnTurnEnded -= DoSomeOnTurnEnded;
        GameController.Instance.states.OnGameStarted -= IsMyTurnOrNot;
    }
}
