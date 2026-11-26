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
    public const string GameMode_Demo = "Demo";
    public const string GameMode_Tutorial = "Tutorial";


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

        if (isNetMatch)
        {
            game.CreateNetSync();
            return;
        }

        if (!isNetMatch && gamemode != GameMode_Tutorial)
        {
            enemyBots.Add(Instantiate(game.botPrefab).GetComponent<EnemyAI>());
            if (isDemonstrationMatchAiVsAi)
                enemyBots.Add(Instantiate(game.botPrefab).GetComponent<EnemyAI>());
        }

        Board.onBoardGenerated += GameStart;
        PreStart();
    }

    private AICharacterProfile GetAppropriateProfile()
    {
        var profiles = GameController.Instance.aiProfiles;
        if (profiles.Count == 0) return null;
        return profiles[UnityEngine.Random.Range(0, profiles.Count)];
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
        stats.gamemode = gamemode;

        if (gamemode == GameMode_Tutorial)
        {
            game.player.SetTeam(Team.Zero);
            game.enemy.SetTeam(Team.Cross);

            PlayerDeck.Instance.BindDeckService(
                TutorialController.Instance.playerCardCollection
                    .BuildService(GameController.Instance.globalCards, GetMaxDeckSize()));
            EnemyDeck.Instance.BindDeckService(
                TutorialController.Instance.enemyCardCollection
                    .BuildService(GameController.Instance.globalCards, GetMaxDeckSize()));

            isMoveOfZero = false;
            turnCount = 0;
            lastWinTeam = Team.None;
            OnSetSettings?.Invoke();
            return;
        }

        if (isDemonstrationMatchAiVsAi)
        {
            enemyBots[0].LoadEnemy(isMoveOfZero ? Team.Zero : Team.Cross, game.player, GetAppropriateProfile());
            enemyBots[1].LoadEnemy(isMoveOfZero ? Team.Cross : Team.Zero, game.enemy, GetAppropriateProfile());
        }
        else
        {
            if (!isNetMatch)
            {
                if (needSwapTeam && lastWinTeam != Team.None)
                    game.player.SetTeam(game.player.GetTeam() == Team.Zero ? Team.Cross : Team.Zero);
                else
                    game.player.SetTeam(GetRandomTeam());

                enemyBots[0].LoadEnemy(
                    game.player.GetTeam() == Team.Zero ? Team.Cross : Team.Zero,
                    game.enemy, GetAppropriateProfile());
            }
            else //isNetMatch
            {
                if (needSwapTeam)
                {
                    game.player.SetTeam(game.player.GetTeam() == Team.Zero ? Team.Cross : Team.Zero);
                }
                EnemyDeck.Instance.BindDeckService(BuildStubDeckService(GetMaxDeckSize()));
            }
            //For isNetMatch check NetMatchSync.OnPreStart()

            var playerDeckService = game.player.UniqueDeck ?? BuildStubDeckService(GetMaxDeckSize());
            if (game.player.UniqueDeck == null)
                Debug.LogWarning("MatchStates: player.UniqueDeck is null — using stub. Check GameController init order.");
            PlayerDeck.Instance.BindDeckService(playerDeckService);
        }

        if (enemyBots.Count > 0)
            foreach (var enemy in enemyBots)
            {
                var preset = Instantiate(enemy.cardsCollection);   
                var ds = preset.BuildService(GameController.Instance.globalCards, GetMaxDeckSize());
                if (enemy.myEntity.GetTeam() == game.player.GetTeam())
                    PlayerDeck.Instance.BindDeckService(ds);
                else
                    EnemyDeck.Instance.BindDeckService(ds);
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
            AnalyticService.Instance.LogMatchStarted(
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

        if (game.player.GetTeam() != Team.None && !isDemonstrationMatchAiVsAi)
        {
            if (PlayerCardHand.Instance.CardsInHand.Count < game.matchSettings.defaultCardsInHand)
                game.player.deck.DrawHandRandomFromDeck(game.matchSettings.defaultCardsInHand);

            if (game.player.GetTeam() != whoMoved && turnCount > game.matchSettings.piecesWinSequence)
                game.player.IncreaseMana(game.matchSettings.manaPerTurn);
        }

        if (!isNetMatch && enemyBots.Count > 0)
        {
            foreach (var bot in enemyBots)
            {
                if (bot.myEntity.GetTeam() != whoMoved)
                {
                    if (turnCount > game.matchSettings.piecesWinSequence)
                        bot.myEntity.IncreaseMana(game.matchSettings.manaPerTurn);

                    if (bot.myEntity.GetTeam() == game.player.GetTeam())
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
        if (game.player.GetTeam() != Team.None)
            result = game.player.GetTeam() == winTeam ? ResultMatch.Win : ResultMatch.Defeat;

        if (isDemonstrationMatchAiVsAi)
        {
            game.secTimer.StartTimer(3, out SecondTimerSubscriber sub, () => { GameRestart(); });
            return;
        }
        else
        {
            if (result == ResultMatch.Win) stats.isWinner = true;
            DailyQuestManager.Instance.OnMatchFinished(stats);

            #region Analytic
            string stringWinTeam = PieceData.NoneTeamName;
            switch (lastWinTeam)
            {
                case Team.Cross: stringWinTeam = PieceData.CrossTeamName; break;
                case Team.Zero: stringWinTeam = PieceData.ZeroTeamName; break;
                default: stringWinTeam = PieceData.NoneTeamName; break;
            }

            var uniqueCards = game.player.UniqueDeck != null
                ? game.player.UniqueDeck.Cards
                : (IReadOnlyList<Card>)Array.Empty<Card>();

            string[] deck = new string[uniqueCards.Count];
            for (int i = 0; i < deck.Length; i++)
                deck[i] = uniqueCards[i].originalCardName;

            float durationMin = (float)(DateTime.UtcNow - matchStartTime).TotalMinutes;

            AnalyticService.Instance.LogMatchFinished(GameController.Instance.player.GetStringPlayerTeam(), stringWinTeam
                , durationMin, turnCount, "PiecesWinSequence", deck, gamemode);
            #endregion
        }

        if (!isNetMatch)
        {
            foreach (var enemy in enemyBots)
                enemy.StopEnemy();
        }


        if (gamemode == GameMode_Matchmaking)
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
        else if (game.player.GetTeam() != Team.None)
        {
            string entryKey;
            Action confirmAction = game.netMatch.OfferRevenge,
                declineAction = LeaveFromMatch;

            if (game.player.GetTeam() == lastWinTeam)
            {
                entryKey = "WinNetMatch";
            }
            else if (lastWinTeam != Team.None && game.player.GetTeam() != lastWinTeam)
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

    #region Deck helpers

    int GetMaxDeckSize() => Mathf.Max(1, game != null && game.matchSettings != null
        ? game.matchSettings.maxDeck
        : DeckService.DefaultMaxSize);

    DeckService BuildStubDeckService(int maxSize)
    {
        var global = GameController.Instance.globalCards;
        var ds = new DeckService(global, maxSize);

        if (global.GlobalCardBacks != null && global.GlobalCardBacks.Count > 0)
        {
            var back = global.GlobalCardBacks[UnityEngine.Random.Range(0, global.GlobalCardBacks.Count)];
            if (back != null) ds.SetCardBack(back.GetComponent<SpriteRenderer>());
        }
        return ds;
    }

    #endregion
}