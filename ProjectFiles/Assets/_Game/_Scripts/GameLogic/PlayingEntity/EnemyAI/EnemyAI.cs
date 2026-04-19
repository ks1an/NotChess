using System;
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
[RequireComponent(typeof(HTN_PlanRunner))]
public sealed class EnemyAI : MonoBehaviour
{
    [HideInInspector] public PlayingEntity myEntity;
    [HideInInspector] public EnemyAI_Sensors sensor;

    public EnemyAI_SO dataBase;

    bool isWorking;
    float DemonstrateChanceOfSkipMove;
    bool isDemontrate;

    HTN_CardPlanner cardAI;
    TileEstimatorForAI estimator;
    MatchStates states;
    readonly MathOperations mathOp = MathOperations.GetInstance();

    #region BeforeStartMyTurn
    public void LoadEnemy(Team botTeam, PlayingEntity playingSideEntity)
    {
        isWorking = true;
        DemonstrateChanceOfSkipMove = dataBase.chanceOfSkipTheMostValuableMove * 2;
        isDemontrate = GameController.Instance.states.isDemonstrationMatchAiVsAi;
        states = GameController.Instance.states;

        sensor = new();
        sensor.Init(playingSideEntity);

        cardAI = new();
        HTN_PlanRunner runnerCardPlan = GetComponent<HTN_PlanRunner>();
        cardAI.Init(runnerCardPlan, this);
        estimator = gameObject.AddComponent<TileEstimatorForAI>();
        estimator.Init(this, dataBase);

        myEntity = playingSideEntity;
        myEntity.SetTeam(botTeam);

        GameController.Instance.states.OnTurnEnded += DoSomeOnTurnEnded;
        GameController.Instance.states.OnGameStarted += IsMyTurnOrNot;
    }


    void IsMyTurnOrNot()
    {
        if ((states.isMoveOfZero && sensor.myTeam == Team.Zero)
            || (!states.isMoveOfZero && sensor.myTeam == Team.Cross))
        {
            if (isWorking)
                StartEnemyTurn();
        }
    }

    void DoSomeOnTurnEnded(int x, int y, Team team) => IsMyTurnOrNot();
    #endregion

    public void StartEnemyTurn()
    {
        if (states.isDemonstrationMatchAiVsAi)
            sensor.myTeam = states.isMoveOfZero ? Team.Zero : Team.Cross;

        estimator.StartEstimate();
    }

    public void DoMove(Dictionary<Vector2Int, int> tilesCost)
    {
        int tileCountX = GameController.Instance.matchSettings.tileCountX;
        int tileCountY = GameController.Instance.matchSettings.tileCountY;
        Piece[,] pieces = Board.Instance.piecesController.pieces;

        HTNWorldState worldState = sensor.GetWorldState();
        bool isMoving = true;
        bool canSkipMostValuableByRandom = true;

#if UNITY_EDITOR
        foreach (var tile in tilesCost)
            Board.Instance.tilesController.tiles[tile.Key.x, tile.Key.y].SetScoreValueTxt(tile.Value);
#endif

        foreach (var tile in tilesCost.OrderBy(k => k.Value).Reverse())
        {
            if (isDemontrate && GameController.Instance.states.turnCount == 0)
            {
                EndEnemyTurn(() => states.move.TryCreateUnitOnBoard((int)mathOp.GetSafeRandom(0, tileCountX - 1), (int)mathOp.GetSafeRandom(0, tileCountY - 1), sensor.myTeam));
                isMoving = false;
                break;
            }

            if ((canSkipMostValuableByRandom && mathOp.GetSafeRandom(0, 100) <= dataBase.chanceOfSkipTheMostValuableMove) ||
                (isDemontrate && mathOp.GetSafeRandom(0, 100) <= DemonstrateChanceOfSkipMove))
                continue;

            int x = tile.Key.x;
            int y = tile.Key.y;
            canSkipMostValuableByRandom = false;
            worldState.SetValue(TargetTile_HTN_WorldKey.Key, tile.Key);

            //Tile empty
            if (pieces[x, y] == null)
            {
                worldState.SetValue(TargetIsEnemy_HTN_WorldKey.Key, false);

                if (Board.Instance.tilesController.tiles[x, y].Stats.CurrentStats.CanPutOnTile)
                {
                    EndEnemyTurn(() => states.move.TryCreateUnitOnBoard(x, y, sensor.myTeam));
                    isMoving = false;
                    break;
                }
                continue;
            }

            Dictionary<Vector2Int, Team> myDiagonalNeighbours = estimator.GetDiagonalNeighborsTeams(x, y);
            //Enemy on tile
            if (pieces[x, y].team != sensor.myTeam && Board.Instance.tilesController.tiles[x, y].Stats.CurrentStats.CanAttackTile)
            {
                worldState.SetValue(TargetIsEnemy_HTN_WorldKey.Key, true);
                if (cardAI.GetPlan())
                {
                    StartEnemyTurn();
                    return;
                }

                #region TryToAttackByDiagonal
                Vector2Int bestMove = new();
                int minValue = 1000000000;
                foreach (var neighbour in myDiagonalNeighbours)
                    if (neighbour.Value == sensor.myTeam && tilesCost[neighbour.Key] < minValue && tilesCost[neighbour.Key] < tile.Value
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
            if (pieces[x, y].team == sensor.myTeam)
            {
                worldState.SetValue(TargetIsEnemy_HTN_WorldKey.Key, false);

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
                    EndEnemyTurn(() => states.move.TryCreateUnitOnBoard(bestMove.x, bestMove.y, sensor.myTeam));
                    break;
                }
                #endregion

                continue;
            }

        }

        if (isMoving)
        {
            Debug.LogError($"Critical error! Enemy AI could not find an available move.\n Early end of move. My team: {sensor.myTeam}");
            EndEnemyTurn(() => { states.GameEnd(GameController.Instance.player.GetLocalPlayerTeam(), 0, 0); });
        }
    }

    public void EndEnemyTurn(Action action)
    {
        estimator.StopEstimate();
        action.Invoke();
    }

    public void StopEnemy()
    {
        isWorking = false;
        estimator.StopEstimate();
        sensor.Destroy();
        cardAI.KillPlanner();
    }

    void OnDisable()
    {
        GameController.Instance.states.OnTurnEnded -= DoSomeOnTurnEnded;
        GameController.Instance.states.OnGameStarted -= IsMyTurnOrNot;
    }
}
