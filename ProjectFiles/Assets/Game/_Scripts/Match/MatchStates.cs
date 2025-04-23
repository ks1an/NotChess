using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchStates : MonoBehaviour
{
    public bool isNetMatch;
    public bool isMoveOfZero;
    [HideInInspector] public Board board;

    #region Events
    public event Action OnGamePreStart;
    public event Action OnGameStarted;
    public event Action<int, int, Team> OnTeamMoved;
    public event Action<int, int, Team> OnGameWin;
    public event Action<int, int, Team> OnGameTied;
    public event Action OnLeaveMatch;
    #endregion

    NetMatchSync net;

    #region BeforePlay
    public void CreateGame(bool isNetMatch)
    {
        board = MatchController.Instance.board;
        this.isNetMatch = isNetMatch;

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
                MatchController.Instance.player.SetPlayerTeam(Team.Zero);
            else
                MatchController.Instance.player.SetPlayerTeam(Team.Cross);
        }
    }

    public void GameRestart()
    {
        SetSettings();
        GameStart();
    }

    public void GameStart()
    {
        WaitingWindowController.Instance.Hide();
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

    public void DestroyUnit(int x, int y)
    {
        if (isNetMatch)
        {
            net.DestroyUnitRpc(x, y);
            return;
        }

        Destroy(board.piecesController.pieces[x, y].gameObject);
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
                MatchController.Instance.player.SetPlayerTeam(Team.Cross);
            else
                MatchController.Instance.player.SetPlayerTeam(Team.Zero);
        }

        isMoveOfZero = !isMoveOfZero;
        OnTeamMoved?.Invoke(x, y, team);
    }
    #endregion

    #region AfterPlay(End)
    void GameEnd(Team winTeam, int x, int y)
    {
        if (!isNetMatch)
        {
            ModalViewWindowController.Instance.ShowHorizontal(false, $"Winner: <color=#FFD700>{winTeam}</color>", "Victory. Nothing to add or take away.",
                "Restart", GameRestart, "Exit", LeaveFromMatch);
            OnGameWin?.Invoke(x, y, winTeam);
        }
        else
        {
            if (MatchController.Instance.player.GetLocalPlayerTeam() == winTeam)
            {
                ModalViewWindowController.Instance.ShowHorizontal(true, $"You have won!", "My applause to you. Want to fight your opponent again? Offer a rematch!",
    "Revenge!", net.OfferRevengeRpc, "Exit to menu", LeaveFromMatch);
                OnGameWin?.Invoke(x, y, winTeam);
            }
            else
            {
                ModalViewWindowController.Instance.ShowHorizontal(true, $"You lost", "I feel sorry for you. Have you tried? Try your luck again. Challenge your opponent to a rematch!",
"Revenge! I'll win.", net.OfferRevengeRpc, "Exit.", LeaveFromMatch);
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
        SceneManager.LoadSceneAsync("MenuScene");
    }
    #endregion
}
