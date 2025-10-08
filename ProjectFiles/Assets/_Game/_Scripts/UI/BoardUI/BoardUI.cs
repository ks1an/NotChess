using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public sealed class BoardUI : MonoBehaviour
{
    [SerializeField] LocalizedStringTable localTable;
    public static BoardUI Singleton;

    public ManaBar manaBar;
    public Muligan muligan;

    [SerializeField] int howManyTurnsAllowMuligan;
    [SerializeField] TextMeshProUGUI teamMoveIndicator;
    [SerializeField] Button restartButton, muliganBttn;

    bool isNetMatch;
    int leftTurndAllowMuligan;

    void Awake()
    {
        muligan = new();
        muliganBttn.onClick.AddListener(muligan.TryDoMuligan);
        if (Singleton == null)
            Singleton = this;
        else
            Destroy(this);

        isNetMatch = GameController.Instance.states.isNetMatch;

        GameController.Instance.states.OnGameStarted += StartSettings;
        GameController.Instance.states.OnTurnEnded += OnTurned;
        GameController.Instance.states.OnGameTied += OnGameEnded;
        GameController.Instance.states.OnGameWin += OnGameEnded;

        restartButton.onClick.RemoveAllListeners();
        if (!isNetMatch)
            restartButton.onClick.AddListener(TryRestartGame);
        else
            restartButton.onClick.AddListener(TryToRevange);
    }

    void StartSettings()
    {
        leftTurndAllowMuligan = howManyTurnsAllowMuligan;
        muligan.SetDefault(muliganBttn.gameObject, localTable);
        muligan.TryShowMuliganButton();

        if (GameController.Instance.states.isMoveOfZero)
            OnTurned(-1, -1, Team.Cross);
        else
            OnTurned(-1, -1, Team.Zero);
    }

    public void TryExitToMenu()
    {
        ModalViewWindowController.Instance.ShowHorizontalWithLocalize
        (
        localTable, "TryExitToMenu",
        false, false, () => { }, ExitToMenu
        );
    }

    public void TryRestartGame()
    {
        ModalViewWindowController.Instance.ShowHorizontalWithLocalize
        (
        localTable, "Restart",
        false, false, () => { }, GameController.Instance.states.GameRestart
        );
    }

    public void TryToRevange()
    {
        ModalViewWindowController.Instance.ShowHorizontalWithLocalize
        (
        localTable, "Revange",
        false, false, () => { }, GameController.Instance.netMatch.OfferRevenge
        );
    }

    #region OnGameState
    void OnTurned(int x, int y, Team teamMoved)
    {
        ChangeTeamMoveIndicator(x, y, teamMoved);
        if (teamMoved == GameController.Instance.player.GetLocalPlayerTeam())
            muligan.HideMuliganButton();
        else
            muligan.TryShowMuliganButton();
    }

    void ChangeTeamMoveIndicator(int x, int y, Team teamMoved)
    {
        if (isNetMatch)
        {
            if (GameController.Instance.player.GetLocalPlayerTeam() == teamMoved)
            {
                teamMoveIndicator.text = "Opponent`s turn";
                teamMoveIndicator.fontStyle = FontStyles.Normal;
            }
            else
            {
                teamMoveIndicator.text = "<b>Your</b> turn!";
                teamMoveIndicator.fontStyle = FontStyles.Underline;
                teamMoveIndicator.fontStyle = FontStyles.Bold;
            }
        }
        else
        {
            teamMoveIndicator.text = teamMoved == Team.Zero ? Team.Cross.ToString() : Team.Zero.ToString();
            teamMoveIndicator.text += "`s turn";
            teamMoveIndicator.fontStyle = FontStyles.Bold;
        }

        if (leftTurndAllowMuligan > 0)
            leftTurndAllowMuligan--;
        else
            muliganBttn.gameObject.SetActive(false);
    }

    void OnGameEnded(int x, int y, Team teamWin)
    {
        teamMoveIndicator.text = teamWin.ToString() + " won";
        teamMoveIndicator.fontStyle = FontStyles.Bold;
    }
    #endregion

    void ExitToMenu() => GameController.Instance.states.LeaveFromMatch();

    private void OnDisable()
    {
        GameController.Instance.states.OnGameTied -= OnGameEnded;
        GameController.Instance.states.OnGameWin -= OnGameEnded;
        GameController.Instance.states.OnGameStarted -= StartSettings;
        GameController.Instance.states.OnTurnEnded -= OnTurned;
    }
}
