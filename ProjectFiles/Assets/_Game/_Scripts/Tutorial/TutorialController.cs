using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class TutorialController : MonoBehaviour
{

    public static TutorialController Instance { get; private set; }

    [Header("Script")]
    [SerializeField] List<TutorialStep> steps = new();
    [SerializeField] public CardCollection playerCardCollection;
    [SerializeField] public CardCollection enemyCardCollection;

    [SerializeField] float baseDelayAfterStep = 0.3f;

    int currentStep = -1;
    bool waitingForPlayer;
    bool waitingForContinue;

    List<Vector2Int> trackedTutorialHighlights = new();
    Action actionOnClick;
    bool playerSelectedCorrectCard;
    private static WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);

    public bool IsActive { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Debug.LogError("Tutorial > 1 on scene");
            Destroy(gameObject);
            return;
        }

        if (GameController.Instance != null && GameController.Instance.states != null)
        {
            GameController.Instance.states.OnGameStarted += OnGameStarted;
            SceneLoader.Instance.OnSomeSceneStartLoading += AbortTutorial;
        }
    }

    public void AbortTutorial()
    {
        if (!IsActive) return; // уже завершён

        Debug.Log("[Tutorial] Aborting");

        IsActive = false;
        waitingForPlayer = false;
        waitingForContinue = false;
        playerSelectedCorrectCard = false;

        // Останавливаем все корутины
        StopAllCoroutines();

        // Убираем то, что мы поставили
        try
        {
            if (Board.Instance != null)
            {
                ClearTileHighlight();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Tutorial] Cleanup warning: {e.Message}");
        }

        // Закрываем модалку, если она открыта
        if (ModalViewWindowController.Instance != null)
            ModalViewWindowController.Instance.TryCloseModalViewWindow();

        // Уничтожаем себя
        if (Instance == this)
            Instance = null;

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this && GameController.Instance.states != null)
        {
            GameController.Instance.states.OnGameStarted -= OnGameStarted;
            SceneLoader.Instance.OnSomeSceneStartLoading -= AbortTutorial;
        }
    }


    void OnGameStarted()
    {
        if (GameController.Instance.states.gamemode != MatchStates.GameMode_Tutorial)
            return;
        GameController.Instance.states.OnGameStarted -= OnGameStarted;

        SetupInitialBoard();
        GameController.Instance.player.IncreaseMana(5);
        GameController.Instance.enemy.IncreaseMana(7);
        StartTutorial();
    }

    void SetupInitialBoard()
    {
        PlacePawn(new Vector2Int(2, 2), GameController.Instance.enemy.GetTeam());
        PlacePawn(new Vector2Int(3, 2), GameController.Instance.enemy.GetTeam());
        PlacePawn(new Vector2Int(4, 1), GameController.Instance.enemy.GetTeam());

        PlacePawn(new Vector2Int(1, 2), GameController.Instance.player.GetTeam());
        PlacePawn(new Vector2Int(3, 4), GameController.Instance.player.GetTeam());
        PlacePawn(new Vector2Int(6, 3), GameController.Instance.player.GetTeam());
        PlacePawn(new Vector2Int(6, 1), GameController.Instance.player.GetTeam());
        PlacePawn(new Vector2Int(7, 0), GameController.Instance.player.GetTeam());
    }

    void PlacePawn(Vector2Int coord, Team team)
    {
        PieceView prefab = team == Team.Zero
            ? GameController.Instance.player.zeroPawnPrefab
            : GameController.Instance.player.crossPawnPrefab;

        GameController.Instance.states.move.TryCreateUnitOnBoard(
            coord.x, coord.y, team, prefab, false);
    }

    public void StartTutorial()
    {
        IsActive = true;
        currentStep = -1;
        actionOnClick = OnContinueClicked;
        NextStep();
    }

    void OnContinueClicked()
    {
        if (waitingForContinue)
            waitingForContinue = false;
    }

    void NextStep()
    {
        currentStep++;
        if (currentStep >= steps.Count)
        {
            FinishTutorial();
            return;
        }
        StartCoroutine(RunStep(steps[currentStep]));
    }

    IEnumerator RunStep(TutorialStep step)
    {
        ClearTileHighlight();
        ModalViewWindowController.Instance.TryCloseModalViewWindow();

        if (step.delayBeforeWork > 0f)
            yield return new WaitForSeconds(step.delayBeforeWork);

        if (!string.IsNullOrEmpty(step.message))
        {
            ShowMessage(step.message, showContinue: true);
            waitingForContinue = true;
            yield return new WaitUntil(() => !waitingForContinue);
            ModalViewWindowController.Instance.TryCloseModalViewWindow();
        }

        switch (step.type)
        {
            case TutorialActionType.ShowMessage:
                NextStep();
                break;

            case TutorialActionType.EnemyMove:
                yield return ExecuteEnemyMove(step);
                NextStep();
                break;

            case TutorialActionType.EnemyPlayCard:
                yield return ExecuteEnemyPlayCard(step);
                NextStep();
                break;

            case TutorialActionType.PlayerPlacePiece:
                if (PlayerCardHand.Instance.CurrentSelectCard != null)
                    PlayerCardHand.Instance.ResetCurrentSelectCard(PlayerCardHand.Instance.CurrentSelectCard);
                Board.Instance.ClearPieceSelection();

                ShowTileHighlight(step.targetTiles);
                yield return WaitForPlayerAction();
                ClearTileHighlight();
                NextStep();
                break;

            case TutorialActionType.PlayerSelectPiece:
            case TutorialActionType.PlayerCapture:
                if (PlayerCardHand.Instance.CurrentSelectCard != null)
                    PlayerCardHand.Instance.ResetCurrentSelectCard(PlayerCardHand.Instance.CurrentSelectCard);

                ShowTileHighlight(step.targetTiles);
                yield return WaitForPlayerAction();
                ClearTileHighlight();
                NextStep();
                break;

            case TutorialActionType.PlayerPlayCard:
                ShowTileHighlight(step.targetTiles);
                yield return WaitForPlayerAction();
                ClearTileHighlight();
                NextStep();
                break;

            case TutorialActionType.Finish:
                FinishTutorial();
                break;
        }
    }

    IEnumerator WaitForPlayerAction()
    {
        waitingForPlayer = true;
        yield return new WaitUntil(() => !waitingForPlayer);
        yield return new WaitForSeconds(baseDelayAfterStep);
    }

    // =========================================================================
    // ENEMY ACTIONS
    IEnumerator ExecuteEnemyMove(TutorialStep step)
    {
        if (step.targetTiles == null || step.targetTiles.Count == 0)
        {
            Debug.LogError("[Tutorial] EnemyMove без targetTiles");
            yield break;
        }

        if (step.targetTiles.Count == 2)
        {
            Vector2Int from = step.targetTiles[0];
            Vector2Int to = step.targetTiles[1];

            GameController.Instance.states.move.MoveUnit(from.x, from.y, to.x, to.y);

            yield return new WaitForSeconds(baseDelayAfterStep);
            yield break;
        }

        Vector2Int target = step.targetTiles[0];

        Team enemyTeam = GameController.Instance.enemy.GetTeam();
        PieceView prefabPawn = enemyTeam == Team.Zero
            ? GameController.Instance.player.zeroPawnPrefab
            : GameController.Instance.player.crossPawnPrefab;

        GameController.Instance.states.move.TryCreateUnitOnBoard(
            target.x, target.y, enemyTeam, prefabPawn);

        yield return new WaitForSeconds(baseDelayAfterStep);
    }

    IEnumerator ExecuteEnemyPlayCard(TutorialStep step)
    {
        if (step.playCardType == null)
        {
            Debug.LogWarning("[Tutorial] EnemyPlayCard без enemyCardType");
            yield return new WaitForSeconds(baseDelayAfterStep);
            yield break;
        }

        Card cardToPlay = EnemyCardHand.Instance.GetCardFromHand(step.playCardType);
        cardToPlay.Init(GameController.Instance.enemy.GetTeam());
        cardToPlay.UseCard(step.targetTiles, false);
        GameController.Instance.enemy.DecreaseMana(cardToPlay.ManaCost);

        if (step.targetTiles != null)
        {
            foreach (var t in step.targetTiles)
            {
                var tile = Board.Instance.tilesController.tiles[t.x, t.y];
                if (tile != null)
                    Board.Instance.tilesController.AccentTiles(
                        new List<Vector2Int> { t }, dark: true);
            }
        }

        yield return new WaitForSeconds(baseDelayAfterStep);

        if (step.targetTiles != null)
        {
            foreach (var t in step.targetTiles)
            {
                Board.Instance.tilesController.RemoveAccentTiles(
                    new List<Vector2Int> { t }, dark: true);
            }
        }
    }

    public bool ShouldBlockSelectionReset()
    {
        if (!IsActive) return false;

        var step = steps[currentStep];
        return step.type == TutorialActionType.PlayerSelectPiece
            || step.type == TutorialActionType.PlayerCapture;
    }

    // =========================================================================
    // HIGHLIGHT

    void ShowTileHighlight(List<Vector2Int> tiles)
    {
        if (tiles == null || tiles.Count == 0) return;
        if (Board.Instance == null) return;

        Board.Instance.tilesController.HighlighTiles(tiles);
        trackedTutorialHighlights.AddRange(tiles);
    }

    void ClearTileHighlight()
    {
        if (trackedTutorialHighlights.Count == 0) return;
        if (Board.Instance == null) return;

        Board.Instance.tilesController.RemoveHighlightTiles(trackedTutorialHighlights);
        trackedTutorialHighlights.Clear();
    }

    // =========================================================================
    // FEEDBACK
    // =========================================================================
    public void NotifyPlayerPlacedPiece(Vector2Int tile)
    {
        if (!IsActive || !waitingForPlayer) return;
        var step = steps[currentStep];
        if (step.type != TutorialActionType.PlayerPlacePiece) return;
        if (step.targetTiles == null || step.targetTiles.Count == 0) return;
        if (tile != step.targetTiles[0]) return;
        waitingForPlayer = false;
    }

    public void NotifyPlayerSelectedPiece(Vector2Int tile)
    {
        if (!IsActive || !waitingForPlayer) return;
        var step = steps[currentStep];
        if (step.type != TutorialActionType.PlayerSelectPiece) return;
        if (step.targetTiles == null || step.targetTiles.Count == 0) return;
        if (tile != step.targetTiles[0]) return;
        waitingForPlayer = false;
    }

    public void NotifyPlayerCapturedPiece(Vector2Int tile)
    {
        if (!IsActive || !waitingForPlayer) return;
        var step = steps[currentStep];
        if (step.type != TutorialActionType.PlayerCapture) return;
        if (step.targetTiles == null || step.targetTiles.Count == 0) return;
        if (tile != step.targetTiles[0]) return;
        waitingForPlayer = false;
    }

    public void NotifyCardUsed()
    {
        if (!IsActive || !waitingForPlayer) return;
        var step = steps[currentStep];
        if (step.type != TutorialActionType.PlayerPlayCard) return;
        waitingForPlayer = false;
        playerSelectedCorrectCard = false;
    }
    public void NotifyWrongTarget(Vector2Int tile)
    {
        var t = Board.Instance.tilesController.tiles[tile.x, tile.y];
        if (t == null) return;

        StartCoroutine(FlashTileWrong(t));
    }

    IEnumerator FlashTileWrong(Tile tile)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", new Color(1f, 0.2f, 0.2f, 1f));
        tile.render.SetPropertyBlock(block);

        yield return _waitForSeconds0_5;

        tile.render.SetPropertyBlock(null);
    }

    // =========================================================================
    // MESSAGE

    void ShowMessage(string text, bool showContinue)
    {
        ModalViewWindowController.Instance.ShowHorizontal(showContinue, "Tutorial", text,
            confirmTxt: "ok", greenAction: actionOnClick);
    }

    // =========================================================================
    // HOOKS FOR BOARD / CARDS
    public bool TrySelectCard(Card card)
    {
        if (!IsActive) return true;

        var step = steps[currentStep];
        if (step.type != TutorialActionType.PlayerPlayCard)
            return false;

        if (step.playCardType == null) return false;
        if (card.GetType() != step.playCardType.GetType())
            return false;

        playerSelectedCorrectCard = true;
        return true;
    }

    public bool TryTapTile(Vector2Int tile)
    {
        if (!IsActive) return true;
        if (!waitingForPlayer) return false;

        var step = steps[currentStep];

        switch (step.type)
        {
            case TutorialActionType.PlayerPlacePiece:
            case TutorialActionType.PlayerSelectPiece:
            case TutorialActionType.PlayerCapture:
                if (step.targetTiles == null || step.targetTiles.Count == 0) return false;
                if (tile != step.targetTiles[0]) return false;
                return true;

            case TutorialActionType.PlayerPlayCard:
                if (!playerSelectedCorrectCard) return false;
                if (PlayerCardHand.Instance == null) return false;
                if (PlayerCardHand.Instance.CurrentSelectCard == null) return false;
                if (step.targetTiles == null || step.targetTiles.Count == 0) return false;
                if (tile != step.targetTiles[0]) return false;
                return true;

            default:
                return false;
        }
    }

    public bool IsDragAllowed()
    {
        if (!IsActive) return true;

        var step = steps[currentStep];
        return step.type == TutorialActionType.PlayerSelectPiece
            || step.type == TutorialActionType.PlayerCapture;
    }

    public void ResetCardSelection()
    {
        playerSelectedCorrectCard = false;
    }

    // =========================================================================
    // FINISH

    void FinishTutorial()
    {
        GameController.Instance.playerData.TutorialCompleted = true;
        GameController.Instance.playerData.SaveData();
        IsActive = false;
        ModalViewWindowController.Instance.ShowHorizontal(true, "Tutorial", "Поздравляю, но это только начало. Дальше будет сложнее. Куда сложнее", false, confirmTxt: "Ok",
            greenAction: () => { }, doItAnyway: () => 
            {
                ModalInputWindow.Instance.Show("What is your name?", "0<Name length<=20", () => { },
                   newName => { GameController.Instance.playerData.PlayerName.Value = newName; }, 20, doItAnyway: GameController.Instance.states.LeaveFromMatch);
            });
        Destroy(gameObject);
    }
}