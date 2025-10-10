using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class MatchStates : MonoBehaviour
{
    [SerializeField] LocalizedStringTable localTable;
    [HideInInspector] public Board board;
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

    GameController game;
    NetMatchSync net;
    Player localPlayer;
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
        localPlayer = game.player;

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
        net = game.netMatch;
        SetSettings();
        board.GenerateBoard();

        OnGamePreStart?.Invoke();
    }

    void SetSettings()
    {
        isMoveOfZero = game.settings.firtsMoveZero;
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
            PlayerDeck.Instance.DrawHandRandomFromDeck(game.settings.startCards, true);
            EnemyDeck.Instance.DrawHandRandomFromDeck(game.settings.startCards, true);
        }
        isGameStarted = true;
        OnGameStarted?.Invoke();
    }
    #endregion

    #region DoMove
    public void TryCreateUnitOnBoard(int x, int y, Team team)
    {
        if (board.piecesController.pieces[x, y] != null)
            return;

        if (isNetMatch)
        {
            net.unitSync.CreateUnitOnBoardRpc(x, y, team);
            return;
        }

        board.piecesController.pieces[x, y] = board.piecesController.GeneratePiece(team);
        board.piecesController.SetPositionSinglePiece(x, y, true);
        TeamMoved(x, y, team);
    }

    public void TryDestroyUnit(int x, int y, bool destroyedByUnit, Team teamWhoDestroyed)
    {
        if(teamWhoDestroyed == Team.None)
        {
            Debug.LogError("Player with NONE team trying destroy unit");
            return;
        }

        if (board.piecesController.pieces[x, y] != null)
            if (isNetMatch)
            {
                net.unitSync.DestroyUnitRpc(x, y, destroyedByUnit, (int)teamWhoDestroyed);
                return;
            }
            else
            {
                if (destroyedByUnit && teamWhoDestroyed == localPlayer.GetLocalPlayerTeam())
                    localPlayer.IncreaseMana(game.settings.manaForDestroyEnemy);

                Destroy(board.piecesController.pieces[x, y].gameObject);
            }
    }

    public void MoveUnit(int originalX, int originalY, int toX, int toY)
    {
        Team team = board.piecesController.pieces[originalX, originalY].team;
        if (isNetMatch)
        {
            net.unitSync.DoUnitMoveRpc(originalX, originalY, toX, toY, (int)team);
            return;
        }

        board.piecesController.MoveTo(originalX, originalY, toX, toY);
        TeamMoved(toX, toY, team);
    }

    public void SetUnitPos(int x, int y, Vector3 pos, bool instantly = false)
    {
        if (isNetMatch)
        {
            net.unitSync.SetUnitPosRpc(x, y, pos, instantly);
            return;
        }

        board.piecesController.pieces[x, y].SetPos(pos, instantly);
    }

    public void UseCard(int cardID, List<Vector2Int> moves)
    {
        if (isNetMatch)
        {
            int[] movesX = new int[moves.Count],
                movesY = new int[moves.Count];
            for (int i = 0; i < moves.Count; i++)
            {
                movesX[i] = moves[i][0];
                movesY[i] = moves[i][1];
            }

            net.cardSync.UseCardRpc(cardID, movesX, movesY);
        }
        BoardUI.Singleton.muligan.OnUsedCard();
    }
    #endregion

    public void TeamMoved(int x, int y, Team team)
    {
        turnCount += 1;
        List<Vector2Int> winTiles = board.CheckWin(x, y);
        if (winTiles.Count == game.settings.piecesWinSequence)
        {
            board.tilesController.HighlighTiles(winTiles);
            GameEnd(team, x, y);
            return;
        }

        if (game.player.GetLocalPlayerTeam() != Team.None)
        {
            if (PlayerDeck.Instance.hand.CardsInHand.Count < game.settings.defaultCardsInHand)
                PlayerDeck.Instance.DrawHandRandomFromDeck(game.settings.defaultCardsInHand);

            if (game.player.GetLocalPlayerTeam() != team && turnCount > game.settings.piecesWinSequence)
                game.player.IncreaseMana(game.settings.manaPerTurn);
        }

        isMoveOfZero = !isMoveOfZero;
        OnTurnEnded?.Invoke(x, y, team);
    }

    #region AfterPlay(End)
    void GameEnd(Team winTeam, int x, int y)
    {
        isGameStarted = false;
        EnvironmentManager.Instance.DoSmallBoardFlickeringLight();
        lastWinTeam = winTeam;
        if (!isNetMatch)
        {
            if (isDemonstrationMatchAiVsAi)
            {
                GameRestart();
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
            Action confirmAction = net.OfferRevenge, 
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
            net.LeaveNetMatch();

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
