using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public enum Complexity
{
    easy,
    normal,
    hard
}
[Serializable]
[RequireComponent(typeof(EnemyAIPlanner))]
public sealed class EnemyAI : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds = new(1f);
    private Coroutine currentTurnCoroutine;
    private CancellationTokenSource myTurnCts;
    [HideInInspector] public PlayingEntity myEntity;
    public CardCollection cardsCollection;

    bool isWorking;
    bool isTakingTurn = false;

    MatchStates states;

    EnemyAIPlanner planner;

    #region BeforeStartMyTurn
    public void LoadEnemy(Team botTeam, PlayingEntity playingSideEntity, AICharacterProfile profileToAssign = null)
    {
        isTakingTurn = false;
        isWorking = true;
        states = GameController.Instance.states;

        myEntity = playingSideEntity;
        myEntity.SetTeam(botTeam);

        planner = GetComponent<EnemyAIPlanner>();
        if (planner != null && profileToAssign != null)
        {
            planner.ApplyProfile(profileToAssign);
        }

        GameController.Instance.states.OnTurnEnded += DoSomeOnTurnEnded;
        GameController.Instance.states.OnGameStarted += OnGameReset;
    }


    void IsMyTurnOrNot()
    {
        if ((states.isMoveOfZero && myEntity.GetTeam() == Team.Zero)
            || (!states.isMoveOfZero && myEntity.GetTeam() == Team.Cross))
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

        var task = planner.GetBestMoveAsync(myEntity.GetTeam(), token);

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
            Debug.Log("Enemy is canceled move. My team: " + myEntity.GetTeam());
            yield break;
        }

        EnemyAIAction bestMove = task.Result;

        if (bestMove.Type == ActionType.None)
        {
            Debug.LogError("[MinMax] bot valid moves = 0!");
            yield break;
        }

        ExecuteMove(bestMove);
        planner.RecordBoardState(planner.CaptureFastState(myEntity.GetTeam()));
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
                var pawn = myEntity.GetTeam() == Team.Zero
                    ? GameController.Instance.player.zeroPawnPrefab
                    : GameController.Instance.player.crossPawnPrefab;
                EndEnemyTurn(() => states.move.TryCreateUnitOnBoard(
                    bestMove.TargetCell.x, bestMove.TargetCell.y, myEntity.GetTeam(), pawn));
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
            cardMove.CardToPlay.Init(myEntity.GetTeam());
            cardMove.CardToPlay.UseCard(cardMove.CardTargets, false);
            if (myEntity.GetTeam() == GameController.Instance.player.GetTeam())
            {
                GameController.Instance.player.DecreaseMana(cardMove.CardToPlay.ManaCost);
                GameController.Instance.player.DecreaseGraveTokens(cardMove.CardToPlay.GraveTokensCost);
            }
            else
            {
                GameController.Instance.enemy.DecreaseMana(cardMove.CardToPlay.ManaCost);
                GameController.Instance.enemy.DecreaseGraveTokens(cardMove.CardToPlay.GraveTokensCost);
            }
        }
        StartCoroutine(ContinueTurnRoutine());
    }

    private IEnumerator ContinueTurnRoutine()
    {
        yield return _waitForSeconds;

        if (isWorking && ((states.isMoveOfZero && myEntity.GetTeam() == Team.Zero) ||
                          (!states.isMoveOfZero && myEntity.GetTeam() == Team.Cross)))
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
        action.Invoke();
    }

    public void StopEnemy()
    {
        isTakingTurn = false;
        isWorking = false;
        OnGameReset();
    }

    void OnDisable()
    {
        GameController.Instance.states.OnTurnEnded -= DoSomeOnTurnEnded;
        GameController.Instance.states.OnGameStarted -= OnGameReset;
        OnGameReset();
    }
}
