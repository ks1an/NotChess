using System;
using System.Collections;
using System.Threading;
using Unity.VisualScripting;
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
    private static WaitForSeconds _waitForSeconds = new(1f);
    private Coroutine currentTurnCoroutine;
    private CancellationTokenSource myTurnCts;
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
                MakeTurn();
            }
        }
    }

    void DoSomeOnTurnEnded(int x, int y, Team team) => IsMyTurnOrNot();
    #endregion

    public void MakeTurn()
    {
        if (!gameObject.activeInHierarchy || !enabled)
            return;

        CancelMySearch();

        myTurnCts = new CancellationTokenSource();
        currentTurnCoroutine = StartCoroutine(GetMoveWithWait(myTurnCts.Token));
    }

    private IEnumerator GetMoveWithWait(CancellationToken token)
    {
        yield return _waitForSeconds;

        if (token.IsCancellationRequested) yield break;

        // Передаем локальный токен конкретного бота
        var task = planner.GetBestMoveAsync(sensor.myTeam, token);

        while (!task.IsCompleted)
        {
            if (token.IsCancellationRequested) yield break;
            yield return null;
        }

        if (task.IsFaulted && task.Exception != null)
        {
            Debug.LogError($"[MinMax Crash]: {task.Exception.InnerException}");
            yield break;
        }

        if (token.IsCancellationRequested || task.IsCanceled)
        {
            Debug.Log("Enemy is canceled move. My team: " + myEntity.GetLocalPlayerTeam());
            yield break;
        }

        EnemyAIAction bestMove = task.Result;

        if (bestMove.Type == ActionType.None)
        {
            Debug.LogError("[MinMax] bot valid moves = 0!");
            yield break;
        }

        ExecuteMove(bestMove);
        planner.RecordBoardState(planner.CaptureFastState(myEntity.GetLocalPlayerTeam()));
    }

    private void ExecuteMove(EnemyAIAction bestMove)
    {
        switch (bestMove.Type)
        {
            case ActionType.None:
                Debug.LogWarning("[EnemyAI] Bot has no valid move. Skipping turn.");
                EndEnemyTurn(() => { });
                break;
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
            case ActionType.PlayCard:
                ApplyCardAndContinueTurn(bestMove);
                break;
            default:
                Debug.LogError($"Error move! BestMoveType: {bestMove.Type}");
                EndEnemyTurn(() => { });
                break;
        }
    }

    private void ApplyCardAndContinueTurn(EnemyAIAction cardMove)
    {
        if (cardMove.CardToPlay != null)
        {
            cardMove.CardToPlay.Init(myEntity.GetLocalPlayerTeam());
            cardMove.CardToPlay.UseCard(cardMove.CardTargets, false);
            if (myEntity.GetLocalPlayerTeam() == GameController.Instance.player.GetLocalPlayerTeam())
            {
                GameController.Instance.player.DeacreaseMana(cardMove.CardToPlay.ManaCost);
                GameController.Instance.player.DeacreaseGraveTokens(cardMove.CardToPlay.GraveTokensCost);
            }
            else
            {
                GameController.Instance.enemy.DeacreaseMana(cardMove.CardToPlay.ManaCost);
                GameController.Instance.enemy.DeacreaseGraveTokens(cardMove.CardToPlay.GraveTokensCost);
            }
        }
        StartCoroutine(ContinueTurnRoutine());
    }

    private IEnumerator ContinueTurnRoutine()
    {
        yield return _waitForSeconds;

        if (isWorking && ((states.isMoveOfZero && sensor.myTeam == Team.Zero) ||
                          (!states.isMoveOfZero && sensor.myTeam == Team.Cross)))
        {
            MakeTurn();
        }
    }

    public void CancelMySearch()
    {
        if (myTurnCts != null)
        {
            try
            {
                if (!myTurnCts.IsCancellationRequested)
                    myTurnCts.Cancel();
            }
            catch { }
            finally
            {
                myTurnCts.Dispose();
                myTurnCts = null;
            }
        }

        if (currentTurnCoroutine != null)
        {
            StopCoroutine(currentTurnCoroutine);
            currentTurnCoroutine = null;
        }
    }

    public void OnGameReset()
    {
        CancelMySearch();
        IsMyTurnOrNot();
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
        OnGameReset();
    }

    void OnDisable()
    {
        GameController.Instance.states.OnTurnEnded -= DoSomeOnTurnEnded;
        GameController.Instance.states.OnGameStarted -= OnGameReset;
        OnGameReset();
    }
}
