using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Localization;

public class MatchStates : MonoBehaviour
{
    [HideInInspector] public Board board;
    [HideInInspector] public MoveExecutor move;
    [HideInInspector] public bool isGameStarted;
    [HideInInspector] public bool isNetMatch;
    [HideInInspector] public bool isMoveOfZero;
    [HideInInspector] public bool isDemonstrationMatchAiVsAi;
    [HideInInspector] public int turnCount;
    [HideInInspector] public Team lastWinTeam;

    #region Events
    public event Action OnGamePreStart;
    public event Action OnSetSettings;
    public event Action OnGameStarted;
    public event Action OnGameRestarted;
    public event Action<int, int, Team> OnTurnEnded;
    public event Action<int, int, Team> OnGameWin;
    public event Action<int, int, Team> OnGameTied;
    public event Action OnLeaveMatch;
    #endregion

    [SerializeField] LocalizedStringTable localTable;

    GameController game;
    List<EnemyAI> enemyBots = new();

    #region BeforePlay
    public void CreateGame(bool isNetMatch, bool isDemontrationMatchAiVsAi)
    {
        isGameStarted = false;
        ModalInputWindow.Instance.Hide();

        this.isNetMatch = isNetMatch;
        this.isDemonstrationMatchAiVsAi = isDemontrationMatchAiVsAi;
        game = GameController.Instance;
        board = game.board;

        if (move == null)
        {
            move = gameObject.AddComponent<MoveExecutor>();
            move.SetSettings(this);
        }

        if (!isDemontrationMatchAiVsAi)
        {
            SceneLoader.Instance.OnBoardSceneLoaded += OnSceneLoaded;
            SceneLoader.Instance.LoadBoardScene(true);
        }
        else
            OnSceneLoaded();
    }

    void OnSceneLoaded()
    {
        enemyBots.Clear();
        if (!isNetMatch)
        {
            enemyBots.Add(Instantiate(game.botPrefab).GetComponent<EnemyAI>());
            if (isDemonstrationMatchAiVsAi)
                enemyBots.Add(Instantiate(game.botPrefab).GetComponent<EnemyAI>());

            Board.onBoardGenerated += GameStart;
            PreStart();
        }
        else
        {
            game.CreateNetSync();
        }
    }

    public void PreStart()
    {
        move.UpdateNet();
        SetSettings();
        board.GenerateBoard();

        OnGamePreStart?.Invoke();
    }

    void SetSettings()
    {
        isMoveOfZero = game.matchSettings.firtsMoveZero;
        lastWinTeam = Team.None;

        if (isDemonstrationMatchAiVsAi)
        {
            enemyBots[0].LoadEnemy(isMoveOfZero ? Team.Zero : Team.Cross, game.player);
            enemyBots[1].LoadEnemy(isMoveOfZero ? Team.Cross : Team.Zero, game.enemy);
        }
        else
        {
            if (!isNetMatch)
            {
                game.player.SetTeam(isMoveOfZero ? Team.Zero : Team.Cross);
                enemyBots[0].LoadEnemy(isMoveOfZero ? Team.Cross : Team.Zero, game.enemy);
            }
            PlayerDeck.Instance.SetCardCollection(game.player.cardCollection);
        }

        if (enemyBots.Count > 0)
            foreach (var enemy in enemyBots)
            {
                var newCollection = Instantiate(enemy.dataBase.CardsCollection);
                newCollection.cardBack = GameController.Instance.globalCards.GlobalCardBacks[UnityEngine.Random.Range(0, GameController.Instance.globalCards.GlobalCardBacks.Count)];

                if (enemy.sensor.myTeam == game.player.GetLocalPlayerTeam())
                    PlayerDeck.Instance.SetCardCollection(newCollection);
                else
                    EnemyDeck.Instance.SetCardCollection(newCollection);
            }


        OnSetSettings?.Invoke();
    }

    public void GameRestart()
    {
        if (!isNetMatch)
        {
            foreach (var enemy in enemyBots)
                enemy.EndEnemyTurn(() => { });
        }

        SetSettings();
        OnGameRestarted?.Invoke();
        GameStart();
    }

    public void GameStart()
    {
        game.player.deck.SetDefaultSettings();
        game.enemy.deck.SetDefaultSettings();
        WaitingWindowController.Instance.Hide();

        turnCount = 0;

        game.player.SetStartMana();
        game.enemy.SetStartMana();
        game.player.deck.DrawHandRandomFromDeck(game.matchSettings.startCards, true);
        game.enemy.deck.DrawHandRandomFromDeck(game.matchSettings.startCards, true);

        isGameStarted = true;
        OnGameStarted?.Invoke();
        board.GenerateLandscape();
    }
    #endregion

    public void TeamMoved(int x, int y, Team whoMoved)
    {
        turnCount++;

        List<Vector2Int> winTiles = board.CheckWin(x, y);
        if (winTiles.Count == game.matchSettings.piecesWinSequence)
        {
            board.tilesController.HighlighTiles(winTiles);
            GameEnd(whoMoved, x, y);
            return;
        }

        if (game.player.GetLocalPlayerTeam() != Team.None)
        {
            if (PlayerCardHand.Instance.CardsInHand.Count < game.matchSettings.defaultCardsInHand)
                game.player.deck.DrawHandRandomFromDeck(game.matchSettings.defaultCardsInHand);

            if (game.player.GetLocalPlayerTeam() != whoMoved && turnCount > game.matchSettings.piecesWinSequence)
                game.player.IncreaseMana(game.matchSettings.manaPerTurn);
        }

        if (!isNetMatch)
        {
            foreach (var enemy in enemyBots)
                if (enemy.myEntity.GetLocalPlayerTeam() != whoMoved && turnCount > game.matchSettings.piecesWinSequence)
                    enemy.myEntity.IncreaseMana(game.matchSettings.manaPerTurn);

            if (EnemyDeck.Instance.hand.CardGameobjectsInHand.Count < game.matchSettings.defaultCardsInHand)
                game.enemy.deck.DrawHandRandomFromDeck(game.matchSettings.defaultCardsInHand);
        }


        isMoveOfZero = !isMoveOfZero;
        if (isNetMatch)
        {
            if (GameController.Instance.netMatch.IsServer)
                board.interactLandscapeGenerator.TryGenerateInteractLandscape();
        }
        else
            board.interactLandscapeGenerator.TryGenerateInteractLandscape();

        OnTurnEnded?.Invoke(x, y, whoMoved);
    }

    #region AfterPlay(End)
    public void GameEnd(Team winTeam, int x, int y)
    {
        isGameStarted = false;
        EnvironmentManager.Instance.DoBoardFlickeringLight(3);
        lastWinTeam = winTeam;
        if (!isNetMatch)
        {
            if (isDemonstrationMatchAiVsAi)
            {
                game.secTimer.StartTimer(3, out SecondTimerSubscriber sub, GameRestart);
                return;
            }

            foreach (var enemy in enemyBots)
                enemy.StopEnemy();

            RevengeOffer();
            OnGameWin?.Invoke(x, y, winTeam);
        }
        else
        {
            RevengeOffer();
            if (game.player.GetLocalPlayerTeam() == lastWinTeam)
            {
                OnGameWin?.Invoke(x, y, winTeam);
            }
            else
            {
                OnGameTied?.Invoke(x, y, winTeam);
            }
        }
    }

    public void RevengeOffer()
    {
        if (!isNetMatch)
        {
            ModalViewWindowController.Instance.ShowHorizontal(false, $"Winner: <color=#FFD700>{lastWinTeam}</color>", "Victory. Nothing to add or take away.",
    false, "Restart", GameRestart, "Exit", LeaveFromMatch);
        }
        else if (game.player.GetLocalPlayerTeam() != Team.None)
        {
            string entryKey;
            Action confirmAction = game.netMatch.OfferRevenge,
                declineAction = LeaveFromMatch;

            if (game.player.GetLocalPlayerTeam() == lastWinTeam)
            {
                entryKey = "WinNetMatch";
            }
            else if (lastWinTeam != Team.None && game.player.GetLocalPlayerTeam() != lastWinTeam)
            {
                entryKey = "LostNetMatch";
            }
            else
            {
                entryKey = "TryRevenge";
                declineAction = () => { };
            }

            ModalViewWindowController.Instance.ShowHorizontalWithLocalize(
                localTable, entryKey,
                false, false, confirmAction, declineAction
                );
        }
    }

    public void LeaveFromMatch() => OnLeaveFromMatchTrigger();

    public void OnLeaveFromMatchTrigger()
    {
        isGameStarted = false;
        if (!isNetMatch)
        {
            Board.onBoardGenerated -= GameStart;
            foreach (var enemy in enemyBots)
                enemy.StopEnemy();
        }
        else
            game.netMatch.LeaveNetMatch();

        SceneLoader.Instance.OnBoardSceneLoaded -= OnSceneLoaded;

        OnLeaveMatch?.Invoke();
        PlayerDeck.Instance.DestroyAllCard();
        EnemyDeck.Instance.DestroyAllCard();

        SceneLoader.Instance.LoadMenuScene(true);
    }

    public void EndDemonstrationGame()
    {
        foreach (var enemy in enemyBots)
            enemy.StopEnemy();

        Board.onBoardGenerated -= GameStart;
        board.DestroyBoard();
    }
    #endregion
}
