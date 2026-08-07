using System;
using System.Collections;
using UnityEngine;

public enum Complexity
{
    easy,
    normal,
    hard
}
[Serializable]
[RequireComponent(typeof(HTN_PlanRunner))]

[RequireComponent(typeof(EnemyAIPlanner))]
public sealed class EnemyAI : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    [HideInInspector] public PlayingEntity myEntity;
    [HideInInspector] public EnemyAI_Sensors sensor;

    public EnemyAI_SO dataBase;

    bool isWorking;
    float DemonstrateChanceOfSkipMove;
    bool isDemontrate;
    bool isTakingTurn = false;

    HTN_CardPlanner cardAI;
    TileEstimatorForAI estimator;
    MatchStates states;
    readonly MathOperations mathOp = MathOperations.GetInstance();

    EnemyAIPlanner planner;

    #region BeforeStartMyTurn
    public void LoadEnemy(Team botTeam, PlayingEntity playingSideEntity)
    {
        isTakingTurn = false;
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

        planner = GetComponent<EnemyAIPlanner>();
        planner.Init(this, sensor);

        
        GameController.Instance.states.OnTurnEnded += DoSomeOnTurnEnded;
        GameController.Instance.states.OnGameStarted += OnGameReset;
    }


    void IsMyTurnOrNot()
    {
        if ((states.isMoveOfZero && sensor.myTeam == Team.Zero)
            || (!states.isMoveOfZero && sensor.myTeam == Team.Cross))
        {
            if (isWorking && !isTakingTurn)
            {
                StartEnemyTurn();
            }
        }
    }

    void DoSomeOnTurnEnded(int x, int y, Team team) => IsMyTurnOrNot();
    #endregion

    public void StartEnemyTurn()
    {
        if (isTakingTurn) return;
        isTakingTurn = true;

        if (states.isDemonstrationMatchAiVsAi)
            sensor.myTeam = states.isMoveOfZero ? Team.Zero : Team.Cross;

        StartCoroutine(GetMoveWithWait());
    }

    private IEnumerator GetMoveWithWait()
    {
        yield return _waitForSeconds1;

        var task = planner.GetBestMoveAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsCanceled || task.IsFaulted || task.Result.Equals(default(EnemyAIAction)))
        {
            yield break;
        }

        EnemyAIAction bestMove = task.Result;
        ExecuteMove(bestMove);
    }

    private void ExecuteMove(EnemyAIAction bestMove)
    {
        switch (bestMove.Type)
        {
            case ActionType.PlacePawn:
                var pawn = myEntity.GetLocalPlayerTeam() == Team.Zero
                    ? GameController.Instance.player.zeroPawnPrefab
                    : GameController.Instance.player.crossPawnPrefab;
                EndEnemyTurn(() => states.move.TryCreateUnitOnBoard(
                    bestMove.TargetCell.x, bestMove.TargetCell.y, sensor.myTeam, pawn));
                break;

            case ActionType.AttackPawn:
                EndEnemyTurn(() => states.move.MoveUnit(
                    bestMove.SourceCell.x, bestMove.SourceCell.y,
                    bestMove.TargetCell.x, bestMove.TargetCell.y));
                break;

            default:
                Debug.LogError($"Error move! BestMoveType: {bestMove.Type}");
                EndEnemyTurn(() => { });
                break;
        }
    }

    public void DoMove()
    {
        EnemyAIAction bestMove = planner.GetBestMove();
        switch (bestMove.Type)
        {
            case ActionType.PlacePawn:
                var pawn = myEntity.GetLocalPlayerTeam() == Team.Zero
                    ? GameController.Instance.player.zeroPawnPrefab
                    : GameController.Instance.player.crossPawnPrefab;
                EndEnemyTurn(() => states.move.TryCreateUnitOnBoard(
                    bestMove.TargetCell.x, bestMove.TargetCell.y, sensor.myTeam, pawn));
                break;

            case ActionType.AttackPawn:
                EndEnemyTurn(() => states.move.MoveUnit(
                    bestMove.SourceCell.x, bestMove.SourceCell.y,
                    bestMove.TargetCell.x, bestMove.TargetCell.y));
                break;

            default:
                Debug.LogError($"Error move! BestMoveType: {bestMove.Type}");
                EndEnemyTurn(() => { });
                break;
        }
    }

    public void EndEnemyTurn(Action action)
    {
        isTakingTurn = false;
        estimator.StopEstimate();
        action.Invoke();
    }

    public void StopEnemy()
    {
        isTakingTurn = false;
        isWorking = false;
        estimator.StopEstimate();
        sensor.Destroy();
        cardAI.KillPlanner();
    }

    void OnDisable()
    {
        GameController.Instance.states.OnTurnEnded -= DoSomeOnTurnEnded;
        GameController.Instance.states.OnGameStarted -= OnGameReset;
    }

    public void OnGameReset()
    {
        StopAllCoroutines();
        if (planner != null)
        {
            planner.CancelPendingSearch();
            IsMyTurnOrNot();
        }
    }

    private void OnDestroy()
    {
        
    }
}
