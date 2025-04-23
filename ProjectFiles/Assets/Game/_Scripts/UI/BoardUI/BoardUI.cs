using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BoardUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI teamMoveIndicator;
    [SerializeField] Button restartButton;
    bool isNetMatch;

    void Awake()
    {
        isNetMatch = MatchController.Instance.states.isNetMatch;

        MatchController.Instance.states.OnGameStarted += StartSettings;
        MatchController.Instance.states.OnTeamMoved += ChangeTeamMoveIndicator;
        MatchController.Instance.states.OnGameTied += OnGameEnded;
        MatchController.Instance.states.OnGameWin += OnGameEnded;

        if (isNetMatch)
            restartButton.gameObject.SetActive(false);
    }

    void StartSettings()
    {
        if (MatchController.Instance.states.isMoveOfZero)
            ChangeTeamMoveIndicator(-1, -1, Team.Cross);
        else
            ChangeTeamMoveIndicator(-1, -1, Team.Zero);
    }

    #region TryTo
    public void TryExitToMenu()
    {
        ModalViewWindowController.Instance.ShowHorizontal(false, "Leave?", "Are you sure you want to quit? Progress for the round will be lost!",
    "Cancel", () => {}, "Leave", ExitToMenu);
    }

    public void TryRestartGame()
    {
        ModalViewWindowController.Instance.ShowHorizontal(false, "Restart?", "Are you sure you want to restart?",
"Cancel", () => {}, "Restart", RestartGame);
    }
    #endregion

    void ChangeTeamMoveIndicator(int x, int y, Team teamMoved)
    {
        if (isNetMatch)
        {
            if (MatchController.Instance.player.GetLocalPlayerTeam() == teamMoved)
            {
                teamMoveIndicator.text = "Opponent`s move";
                teamMoveIndicator.fontStyle = FontStyles.Normal;
            }
            else
            {
                teamMoveIndicator.text = "Your move!";
                teamMoveIndicator.fontStyle = FontStyles.Underline;
                teamMoveIndicator.fontStyle = FontStyles.Bold;
            }
        }
        else
        {
            teamMoveIndicator.text = teamMoved == Team.Zero ? Team.Cross.ToString() : Team.Zero.ToString();
            teamMoveIndicator.text += "`s move";
            teamMoveIndicator.fontStyle = FontStyles.Bold;
        }
    }

    void OnGameEnded(int x, int y, Team teamWin)
    {
        teamMoveIndicator.text = teamWin.ToString() + " won";
        teamMoveIndicator.fontStyle = FontStyles.Bold;
    }

    void ExitToMenu()
    {
        MatchController.Instance.states.LeaveFromMatch();
    }

    void RestartGame()
    {
        MatchController.Instance.states.GameRestart();
    }

    private void OnDisable()
    {
        MatchController.Instance.states.OnGameTied -= OnGameEnded;
        MatchController.Instance.states.OnGameWin -= OnGameEnded;
        MatchController.Instance.states.OnGameStarted -= StartSettings;
        MatchController.Instance.states.OnTeamMoved -= ChangeTeamMoveIndicator;
    }
}
