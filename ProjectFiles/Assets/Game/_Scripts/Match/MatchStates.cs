using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MatchStates : MonoBehaviour
{
    public bool isNetMatch;
    public bool isMoveOfZero;
    [HideInInspector] public Board board;

    #region Events
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
        if (isNetMatch)
            net = MatchController.Instance.netMatch;

        if (!isNetMatch)
        {
            SceneManager.sceneLoaded += OnBoardSceneLoad;
            Board.onBoardGenerated += GameStart;
        }
        else
        {
            Board.onBoardGenerated += GameStart;
            if (net.IsServer)
                PreStart();
            else
                NetMatchSync.OnNetSpawned += PreStart;

            SceneManager.LoadSceneAsync("BoardScene");
        }
    }

    void OnBoardSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "BoardScene")
            return;

        PreStart();
    }

    public void PreStart()
    {
        SetSettings();
        board.GenerateBoard();
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

    void GameStart()
    {
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
            GameEnd(MatchController.Instance.player.GetLocalPlayerTeam(), x, y);
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
            ModalViewWindowController.Instance.ShowHorizontal($"Winner: {winTeam}", "Victory. Nothing to add or take away.", "Restart", "Exit", GameRestart, LeaveFromMatch);
            OnGameWin?.Invoke(x, y, winTeam);
        }
        else
        {
            OnNetGameEndTrigger((int)winTeam, x, y);
        }
    }

    public void OnNetGameEndTrigger(int winTeamNum, int x, int y)
    {
        var winTeam = (Team)winTeamNum;

        if (MatchController.Instance.player.GetLocalPlayerTeam() == winTeam)
            OnGameWin?.Invoke(x, y, winTeam);
        else
            OnGameTied?.Invoke(x, y, winTeam);

        Debug.Log("Testing restart...");
        GameRestart();
    }

    public void LeaveFromMatch()
    {
        if (isNetMatch)
        {
            net.LeaveNetMatch();
            return;
        }
        else
            OnLeaveFromMatchTrigger();
    }

    public void OnLeaveFromMatchTrigger()
    {
        OnLeaveMatch?.Invoke();

        if (!isNetMatch)
        {
            SceneManager.sceneLoaded -= OnBoardSceneLoad;
            Board.onBoardGenerated -= GameStart;
        }
        else
            Board.onBoardGenerated -= GameStart;

        SceneManager.LoadSceneAsync("MenuScene");
    }
    #endregion
}
