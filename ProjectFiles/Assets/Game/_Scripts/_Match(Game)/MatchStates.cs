using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchStates : MonoBehaviour
{
    [HideInInspector] public bool isNetMatch;
    [HideInInspector] public bool isMoveOfZero;
    [HideInInspector] public Board board;

    #region Events
    public event Action OnGamePreStart;
    public event Action OnGameStarted;
    public event Action OnGameRestarted;
    public event Action<int, int, Team> OnTurnEnded;
    public event Action<int, int, Team> OnGameWin;
    public event Action<int, int, Team> OnGameTied;
    public event Action OnLeaveMatch;
    #endregion

    NetMatchSync net;
    Player localPlayer;

    #region BeforePlay
    public void CreateGame(bool isNetMatch)
    {
        board = MatchController.Instance.board;
        this.isNetMatch = isNetMatch;
        localPlayer = MatchController.Instance.player;

        SceneManager.sceneLoaded += OnBoardSceneLoad;
        SceneManager.LoadSceneAsync("BoardScene");
    }

    void OnBoardSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "BoardScene")
            return;

        if (!isNetMatch)
        {
            Board.onBoardGenerated += GameStart;
            PreStart();
        }
        else
        {
            MatchController.Instance.CreateNetSync();
        }
    }

    public void PreStart()
    {
        net = MatchController.Instance.netMatch;
        SetSettings();
        board.GenerateBoard();
        OnGamePreStart?.Invoke();
    }

    void SetSettings()
    {
        isMoveOfZero = MatchController.Instance.settings.firtsMoveZero;

        if (!isNetMatch)
        {
            if (isMoveOfZero)
                MatchController.Instance.player.SetSettings(Team.Zero, true);
            else
                MatchController.Instance.player.SetSettings(Team.Cross, true);
        }
        Deck.Instance.DestroyAllCard();
    }

    public void GameRestart()
    {
        SetSettings();
        GameStart();
        OnGameRestarted?.Invoke();
    }

    public void GameStart()
    {
        WaitingWindowController.Instance.Hide();
        Deck.Instance.DrawHand(MatchController.Instance.settings.startCards);
        OnGameStarted?.Invoke();
    }
    #endregion

    #region DoMove
    public void CreateUnitOnBoard(int x, int y, Team team)
    {
        if (board.piecesController.pieces[x, y] != null)
            return;

        if (isNetMatch)
        {
            net.CreateUnitOnBoardRpc(x, y, team);
            return;
        }

        board.piecesController.pieces[x, y] = board.piecesController.GeneratePiece(team);
        board.piecesController.SetPositionSinglePiece(x, y, true);
        TeamMoved(x, y, team);
    }

    public void TryDestroyUnit(int x, int y, bool destroyedByUnit)
    {
        if (board.piecesController.pieces[x, y] != null)
            if (isNetMatch)
            {
                net.DestroyUnitRpc(x, y, destroyedByUnit);
                return;
            }
            else
            {
                if (destroyedByUnit && board.piecesController.pieces[x, y].team != localPlayer.GetLocalPlayerTeam())
                    localPlayer.IncreaseMana(MatchController.Instance.settings.manaForDestroyEnemy);

                Destroy(board.piecesController.pieces[x, y].gameObject);
            }
    }

    public void MoveUnit(int originalX, int originalY, int toX, int toY)
    {
        Team team = board.piecesController.pieces[originalX, originalY].team;
        if (isNetMatch)
        {
            net.DoUnitMoveRpc(originalX, originalY, toX, toY, (int)team);
            return;
        }

        board.piecesController.MoveTo(originalX, originalY, toX, toY);
        TeamMoved(toX, toY, team);
    }

    public void SetUnitPos(int x, int y, Vector3 pos, bool instantly = false)
    {
        if (isNetMatch)
        {
            net.SetUnitPosRpc(x, y, pos, instantly);
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
            for(int i = 0; i < moves.Count; i++)
            {
                movesX[i] = moves[i][0];
                movesY[i] = moves[i][1];
            }

            net.UseCardRpc(cardID, movesX, movesY);
        }
    }
    #endregion

    #region Play
    public void TeamMoved(int x, int y, Team team)
    {
        if (board.CheckWin(x, y))
        {
            GameEnd(team, x, y);
            return;
        }

        if (!isNetMatch)
        {
            if (isMoveOfZero)
                MatchController.Instance.player.SetSettings(Team.Cross);
            else
                MatchController.Instance.player.SetSettings(Team.Zero);
        }

        if (Deck.Instance.playerHand.CardsInHand.Count < MatchController.Instance.settings.maxCardsInHand)
            Deck.Instance.DrawHand(MatchController.Instance.settings.maxCardsInHand - Deck.Instance.playerHand.CardsInHand.Count);


        isMoveOfZero = !isMoveOfZero;
        OnTurnEnded?.Invoke(x, y, team);
    }
    #endregion

    #region AfterPlay(End)
    void GameEnd(Team winTeam, int x, int y)
    {
        if (!isNetMatch)
        {
            ModalViewWindowController.Instance.ShowHorizontal(false, $"Winner: <color=#FFD700>{winTeam}</color>", "Victory. Nothing to add or take away.",
                false, "Restart", GameRestart, "Exit", LeaveFromMatch);
            OnGameWin?.Invoke(x, y, winTeam);
        }
        else
        {
            if (MatchController.Instance.player.GetLocalPlayerTeam() == winTeam)
            {
                ModalViewWindowController.Instance.ShowHorizontal(true, $"You have won!", "My applause to you. Want to fight your opponent again? Offer a rematch!",
    false,"Revenge!", net.OfferRevengeRpc, "Exit to menu", LeaveFromMatch);
                OnGameWin?.Invoke(x, y, winTeam);
            }
            else
            {
                ModalViewWindowController.Instance.ShowHorizontal(true, $"You lost", "I feel sorry for you. Have you tried? Try your luck again. Challenge your opponent to a rematch!",
false, "Revenge! I'll win.", net.OfferRevengeRpc, "Exit.", LeaveFromMatch);
                OnGameTied?.Invoke(x, y, winTeam);
            }
        }
    }

    public void LeaveFromMatch() => OnLeaveFromMatchTrigger();

    public void OnLeaveFromMatchTrigger()
    {
        if (!isNetMatch)
            Board.onBoardGenerated -= GameStart;
        else
            net.LeaveNetMatch();

        SceneManager.sceneLoaded -= OnBoardSceneLoad;

        OnLeaveMatch?.Invoke();
        Deck.Instance.DestroyAllCard();
        SceneManager.LoadSceneAsync("MenuScene");
    }
    #endregion
}
