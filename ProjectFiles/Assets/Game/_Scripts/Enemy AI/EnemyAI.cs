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
    [SerializeField, Range(0.5f, 5)] float maxDelayBeforeDoMove;

    [Header("Value params")]
    [SerializeField] Complexity complexity;

    [Space(15)]
    [SerializeField, Range(-10, 10), Tooltip("How much value to add to a tile if there is an enemy next to it vertically or horizontally")]
    int valueIfEnemyVertOrHoriz;
    [SerializeField, Range(-10, 10)]
    int valueIfFriendVertOrHoriz;

    [Space(10)]
    [SerializeField, Range(-10, 10)]
    int valueIfEnemyDiagonal;
    [SerializeField, Range(-10, 10)]
    int valueIfFriendDiagonal;
    [SerializeField, Range(-10, 10)]
    int valueIfBoardBorder;

    MatchStates states;
    Piece[,] pieces;

    public Dictionary<Vector2Int, int> tilesCost = new();
    int tileCountX, tileCountY;
    Team myTeam;
    #endregion

    #region BeforeStartMyTurn
    public void LoadEnemy(Team enemyTeam)
    {
        states = MatchController.Instance.states;
        tileCountX = MatchController.Instance.settings.tileCountX;
        tileCountY = MatchController.Instance.settings.tileCountY;
        SetMyTeam(enemyTeam);

        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tilesCost.Add(new Vector2Int(x, y), 0);

        MatchController.Instance.states.OnTurnEnded += DoSomeOnTurnEnded;
        MatchController.Instance.states.OnGameStarted += IsMyTurnOrNot;
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


        if(states.isMatchAiVsAi)
            myTeam = states.isMoveOfZero ? Team.Zero : Team.Cross;

        StartCoroutine(EstimateCostOfTiles());
    }

    #region EstimateCost
    IEnumerator EstimateCostOfTiles()
    {
        //EstimateCostLines();
        EstimateCostIndividualTiles();
        yield return new WaitForSeconds(/*UnityEngine.Random.Range(0.5f, */maxDelayBeforeDoMove/*)*/);
        DoMove();
    }

    /*void EstimateCostLines() 
    {

    }*/

    void EstimateCostIndividualTiles()
    {
        foreach (Vector2Int tile in tilesCost.Keys.ToList())
        {
            #region Check Horiz&&Vert Tiles
            if (tile.x + 1 < tileCountX && pieces[tile.x + 1, tile.y] != null)
                AddValueForVertsOrHoriz(tile.x + 1, tile.y, tile);

            if (tile.x - 1 >= 0 && pieces[tile.x - 1, tile.y] != null)
                AddValueForVertsOrHoriz(tile.x - 1, tile.y, tile);

            if (tile.y + 1 < tileCountY && pieces[tile.x, tile.y + 1] != null)
                AddValueForVertsOrHoriz(tile.x, tile.y + 1, tile);

            if (tile.y - 1 >= 0 && pieces[tile.x, tile.y - 1] != null)
                AddValueForVertsOrHoriz(tile.x, tile.y - 1, tile);
            #endregion

            #region Check Diagonal Tiles
            if (tile.x - 1 >= 0)
            {
                if (tile.y - 1 >= 0 && pieces[tile.x - 1, tile.y - 1] != null)
                    AddValueForDiagonal(tile.x - 1, tile.y - 1, tile);

                if (tile.y + 1 < tileCountY && pieces[tile.x - 1, tile.y + 1] != null)
                    AddValueForDiagonal(tile.x - 1, tile.y + 1, tile);
            }

            if (tile.x + 1 < tileCountX)
            {
                if (tile.y - 1 >= 0 && pieces[tile.x + 1, tile.y - 1] != null)
                    AddValueForDiagonal(tile.x + 1, tile.y - 1, tile);

                if (tile.y + 1 < tileCountY && pieces[tile.x + 1, tile.y + 1] != null)
                    AddValueForDiagonal(tile.x + 1, tile.y + 1, tile);
            }
            #endregion

            #region CheckTileIsBorder
            if (tile.x == 0 || tile.y == 0 || tile.x == tileCountX-1 || tile.y == tileCountY-1)
                tilesCost[tile] += valueIfBoardBorder;
            #endregion
        }
    }

    #region AddValue
    void AddValueForVertsOrHoriz(int xNeighbourTile, int yNeighbourTile, Vector2Int targetTile)
    {
        if (pieces[xNeighbourTile, yNeighbourTile].team != myTeam)
            tilesCost[targetTile] += valueIfEnemyVertOrHoriz;
        else
            tilesCost[targetTile] += valueIfFriendVertOrHoriz;
    }

    void AddValueForDiagonal(int xNeighbourTile, int yNeighbourTile, Vector2Int targetTile)
    {
        if (pieces[xNeighbourTile, yNeighbourTile].team != myTeam)
            tilesCost[targetTile] += valueIfEnemyDiagonal;
        else
            tilesCost[targetTile] += valueIfFriendDiagonal;
    }
    #endregion

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
            //Enemy on tile
            else if (pieces[x, y].team != myTeam)
            {
                if (x - 1 >= 0)
                {
                    if (y - 1 >= 0 && GetTeamOnTile(x - 1, y - 1) == myTeam)
                    {
                        EndEnemyTurn(() => states.MoveUnit(x - 1, y - 1, x, y));
                        isMoving = false;
                        break;
                    }

                    if (y + 1 < tileCountY && GetTeamOnTile(x - 1, y + 1) == myTeam)
                    {

                        EndEnemyTurn(() => states.MoveUnit(x - 1, y + 1, x, y));
                        isMoving = false;
                        break;
                    }
                }

                if (x + 1 < tileCountX)
                {
                    if (y - 1 >= 0 && GetTeamOnTile(x + 1, y - 1) == myTeam)
                    {
                        EndEnemyTurn(() => states.MoveUnit(x + 1, y - 1, x, y));
                        isMoving = false;
                        break;
                    }
                    if (y + 1 < tileCountY && GetTeamOnTile(x + 1, y + 1) == myTeam)
                    {
                        EndEnemyTurn(() => states.MoveUnit(x + 1, y + 1, x, y));
                        isMoving = false;
                        break;
                    }
                }

            }
            //Friend on tile
            else if (pieces[x, y].team == myTeam)
            {
                continue;
            }
        }

        if (isMoving)
        {
            Debug.Log(myTeam);
            Debug.LogError($"Critical error! Enemy AI could not find an available move.\n Early end of move.");
            EndEnemyTurn(() => { });
        }
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

    private void OnDestroy()
    {
        MatchController.Instance.states.OnTurnEnded -= DoSomeOnTurnEnded;
        MatchController.Instance.states.OnGameStarted -= IsMyTurnOrNot;
    }
}
