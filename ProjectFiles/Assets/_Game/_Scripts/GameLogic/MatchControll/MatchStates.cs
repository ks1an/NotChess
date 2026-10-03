using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class MatchStates : MonoBehaviour
{
    public const string OpponentTypeName_AI = "AI";
    public const string OpponentTypeName_Human = "Human";
    //GameMode
    public const string GameMode_Matchmaking = "Matchmaking";
    public const string GameMode_SinglePlayer = "SinglePlayer";
    public const string GameMode_CustomLobby = "CustomLobby";
    public const string GameMode_Demo= "Demo";


    [HideInInspector] public Board board;
    [HideInInspector] public MoveExecutor move;
    [HideInInspector] public bool isGameStarted;
    [HideInInspector] public bool isNetMatch;
    [HideInInspector] public bool isMoveOfZero;
    [HideInInspector] public bool isDemonstrationMatchAiVsAi;
    [HideInInspector] public int turnCount;
    [HideInInspector] public Team lastWinTeam;
    public PlayerMatchStats stats;
    public string gamemode;
    DateTime matchStartTime;
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
    public void CreateGame(bool isNetMatch, bool isDemontrationMatchAiVsAi, string gamemode)
    {
        this.gamemode = gamemode;
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
        if (!isDemonstrationMatchAiVsAi)
            NotificationPanelConroller.Instance.ShowNotification($"Ряд из 5 юнитов = победа", () => { });
    }

    public Team GetRandomTeam()
    {
        int t = UnityEngine.Random.Range(0, 2);
        if (t == 0) return Team.Zero;
        else return Team.Cross;
    }

    void SetSettings(bool needSwapTeam = true)
    {
        stats = new();
        isMoveOfZero = game.matchSettings.firtsMoveZero;

        if (isDemonstrationMatchAiVsAi)
        {
            enemyBots[0].LoadEnemy(isMoveOfZero ? Team.Zero : Team.Cross, game.player);
            enemyBots[1].LoadEnemy(isMoveOfZero ? Team.Cross : Team.Zero, game.enemy);
        }
        else
        {
            if (!isNetMatch)
            {
                if (needSwapTeam && lastWinTeam != Team.None)
                    game.player.SetTeam(game.player.GetLocalPlayerTeam() == Team.Zero ? Team.Cross : Team.Zero);
                else
                    game.player.SetTeam(GetRandomTeam());

                enemyBots[0].LoadEnemy(
                    game.player.GetLocalPlayerTeam() == Team.Zero ? Team.Cross : Team.Zero,
                    game.enemy);
            }
            else //isNetMatch
            {
                if (needSwapTeam)
                {
                    game.player.SetTeam(game.player.GetLocalPlayerTeam() == Team.Zero ? Team.Cross : Team.Zero);
                }
            }
            //For isNetMatch check NetMatchSync.OnPreStart()

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

        lastWinTeam = Team.None;
        OnSetSettings?.Invoke();
    }

    public void GameRestart(bool needSwapTeam = true)
    {
        if (!isNetMatch)
        {
            foreach (var enemy in enemyBots)
                enemy.EndEnemyTurn(() => { });
        }

        SetSettings(needSwapTeam);
        OnGameRestarted?.Invoke();
        GameStart();
    }

    public void GameStart()
    {
        game.player.deck.SetDefaultSettings();
        game.enemy.deck.SetDefaultSettings();
        WaitingWindowController.Instance.Hide();

        turnCount = 0;

        game.player.SetStartManaAndGraveTokens();
        game.player.deck.DrawHandRandomFromDeck(game.matchSettings.startCards, true);
        game.enemy.SetStartManaAndGraveTokens();
        if (!isNetMatch)
            game.enemy.deck.DrawHandRandomFromDeck(game.matchSettings.startCards, true);


        isGameStarted = true;
        OnGameStarted?.Invoke();
        board.GenerateLandscape();

        if (!isDemonstrationMatchAiVsAi)
        {
            matchStartTime = DateTime.UtcNow;
            string oppType = enemyBots.Count > 0 ? OpponentTypeName_AI : OpponentTypeName_Human;
            AnalyticsManager.Instance.LogMatchStarted(
                GameController.Instance.player.GetStringPlayerTeam(), oppType, gamemode);
        }
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

        if (game.player.GetLocalPlayerTeam() != Team.None && !isDemonstrationMatchAiVsAi)
        {
            if (PlayerCardHand.Instance.CardsInHand.Count < game.matchSettings.defaultCardsInHand)
                game.player.deck.DrawHandRandomFromDeck(game.matchSettings.defaultCardsInHand);

            if (game.player.GetLocalPlayerTeam() != whoMoved && turnCount > game.matchSettings.piecesWinSequence)
                game.player.IncreaseMana(game.matchSettings.manaPerTurn);
        }

        if (!isNetMatch)
        {
            foreach (var bot in enemyBots)
            {
                if (bot.myEntity.GetLocalPlayerTeam() != whoMoved)
                {
                    if (turnCount > game.matchSettings.piecesWinSequence)
                        bot.myEntity.IncreaseMana(game.matchSettings.manaPerTurn);

                    if (bot.myEntity.GetLocalPlayerTeam() == game.player.GetLocalPlayerTeam())
                    {
                        if (PlayerCardHand.Instance.CardsInHand.Count < game.matchSettings.defaultCardsInHand)
                            game.player.deck.DrawHandRandomFromDeck(game.matchSettings.defaultCardsInHand);
                    }
                    else
                    {
                        if (EnemyDeck.Instance.hand.CardGameobjectsInHand.Count < game.matchSettings.defaultCardsInHand)
                            game.enemy.deck.DrawHandRandomFromDeck(game.matchSettings.defaultCardsInHand);
                    }
                }

            }

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
        stats.turnsCount++;
    }

    #region AfterPlay(End)
    public void GameEnd(Team winTeam, int x, int y)
    {
        isGameStarted = false;
        EnvironmentManager.Instance.DoBoardFlickeringLight(3);
        lastWinTeam = winTeam;

        ResultMatch result = ResultMatch.Defeat;
        if (game.player.GetLocalPlayerTeam() != Team.None)
            result = game.player.GetLocalPlayerTeam() == winTeam ? ResultMatch.Win : ResultMatch.Defeat;

        if (isDemonstrationMatchAiVsAi)
        {
            game.secTimer.StartTimer(3, out SecondTimerSubscriber sub, () => { GameRestart(); });
            return;
        }
        else
        {
            string stringWinTeam = PieceData.NoneTeamName;
            switch (lastWinTeam)
            {
                case Team.Cross: stringWinTeam= PieceData.CrossTeamName; break;
                case Team.Zero: stringWinTeam= PieceData.ZeroTeamName; break;
                default: stringWinTeam= PieceData.NoneTeamName; break;
            }
            string[] deck = new string[PlayerDeck.Instance.cardCollection.CardsInCollection.Count];
            for (int i = 0; i < deck.Length; i++)
                deck[i] = PlayerDeck.Instance.cardCollection.CardsInCollection[i].originalCardName;
            float durationMin = (float)(DateTime.UtcNow - matchStartTime).TotalMinutes;

            AnalyticsManager.Instance.LogMatchFinished(GameController.Instance.player.GetStringPlayerTeam(), stringWinTeam
                , durationMin, turnCount, "PiecesWinSequence", deck, gamemode);
        }

        if (!isNetMatch)
        {
            foreach (var enemy in enemyBots)
                enemy.StopEnemy();
        }
        else
        {
            RatingService.Instance.ApplyMatchResult(result);
        }

        RevengeOfferMenu();
        switch (result)
        {
            case ResultMatch.Win:
                OnGameWin?.Invoke(x, y, winTeam);
                break;

            case ResultMatch.Defeat:
                OnGameTied?.Invoke(x, y, winTeam);
                break;
        }
    }

    public void RevengeOfferMenu()
    {
        if (!isNetMatch)
        {
            ModalViewWindowController.Instance.ShowHorizontal(false, $"Winner: <color=#FFD700>{lastWinTeam}</color>", "Victory. Nothing to add or take away.",
    false, "Restart", () => { GameRestart(); }, "Exit", LeaveFromMatch);
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
        ModalViewWindowController.Instance.ShowSubPannelInfo("Turns count", stats.turnsCount.ToString());
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
