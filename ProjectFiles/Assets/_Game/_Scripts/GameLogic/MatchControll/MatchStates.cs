using System;
using System.Collections.Generic;
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
    EnemyAI enemyBot;

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
        game.enemy = new();
        if (!isNetMatch)
        {
            enemyBot = Instantiate(game.botPrefab).GetComponent<EnemyAI>();
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
            enemyBot.LoadEnemy(isMoveOfZero ? Team.Zero : Team.Cross);
            game.player.SetPlayerTeam(Team.None);
        }
        else if (!isNetMatch)
        {
            game.player.SetPlayerTeam(isMoveOfZero ? Team.Zero : Team.Cross);
            enemyBot.LoadEnemy(isMoveOfZero ? Team.Cross : Team.Zero);
        }

        if (game.player.GetLocalPlayerTeam() == Team.Zero)
            game.enemy.SetTeam(Team.Cross);
        else if (game.player.GetLocalPlayerTeam() == Team.Cross)
            game.enemy.SetTeam(Team.Zero);

        OnSetSettings?.Invoke();
    }

    public void GameRestart()
    {
        if (!isNetMatch)
        {
            enemyBot.EndEnemyTurn(() => { });
        }

        SetSettings();
        OnGameRestarted?.Invoke();
        GameStart();
    }

    public void GameStart()
    {
        PlayerDeck.Instance.SetDefaultSettings();
        EnemyDeck.Instance.SetDefaultSettings();
        WaitingWindowController.Instance.Hide();

        turnCount = 0;
        if (game.player.GetLocalPlayerTeam() != Team.None)
        {
            game.player.SetStartMana();
            game.enemy.SetStartMana();
            PlayerDeck.Instance.DrawHandRandomFromDeck(game.matchSettings.startCards, true);
            EnemyDeck.Instance.DrawHandRandomFromDeck(game.matchSettings.startCards, true);
        }

        isGameStarted = true;
        OnGameStarted?.Invoke();
    }
    #endregion

    public void TeamMoved(int x, int y, Team team)
    {
        turnCount += 1;
        List<Vector2Int> winTiles = board.CheckWin(x, y);
        if (winTiles.Count == game.matchSettings.piecesWinSequence)
        {
            board.tilesController.HighlighTiles(winTiles);
            GameEnd(team, x, y);
            return;
        }

        if (game.player.GetLocalPlayerTeam() != Team.None)
        {
            if (PlayerDeck.Instance.hand.CardsInHand.Count < game.matchSettings.defaultCardsInHand)
                PlayerDeck.Instance.DrawHandRandomFromDeck(game.matchSettings.defaultCardsInHand);

            if (game.player.GetLocalPlayerTeam() != team && turnCount > game.matchSettings.piecesWinSequence)
                game.player.IncreaseMana(game.matchSettings.manaPerTurn);
        }

        isMoveOfZero = !isMoveOfZero;
        OnTurnEnded?.Invoke(x, y, team);
    }

    #region AfterPlay(End)
    void GameEnd(Team winTeam, int x, int y)
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

            enemyBot.StopEnemy();
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
            enemyBot.StopEnemy();
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
        enemyBot.StopEnemy();

        Board.onBoardGenerated -= GameStart;
        board.DestroyBoard();
    }
    #endregion
}
