using TMPro;
using UnityEngine;

public sealed class BoardUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI teamMoveIndicator;
    bool isNetMatch;

    void Awake()
    {
        isNetMatch = MatchController.Instance.states.isNetMatch;

        MatchController.Instance.states.OnGameStarted += StartSettings;
        MatchController.Instance.states.OnTeamMoved += ChangeTeamMoveIndicator;
    }

    void StartSettings()
    {
        if (MatchController.Instance.states.isMoveOfZero)
            ChangeTeamMoveIndicator(-1, -1, Team.Cross);
        else
            ChangeTeamMoveIndicator(-1, -1, Team.Zero);
    }

    public void TryExitToMenu()
    {
        ModalViewWindowController.Instance.ShowHorizontal("Leave?", "Are you sure you want to quit? Progress for the round will be lost!",
    "Cancel", "Leave", greenAction: ModalViewWindowController.Instance.CloseModalWindow, redAction: ExitToMenu);
    }

    public void TryRestartGame()
    {
        ModalViewWindowController.Instance.ShowHorizontal("Restart?", "Are you sure you want to restart?",
"Cancel", "Restart", greenAction: ModalViewWindowController.Instance.CloseModalWindow, redAction: RestartGame);
    }

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

    void ExitToMenu()
    {
        MatchController.Instance.states.LeaveFromMatch();
    }

    void RestartGame()
    {
        MatchController.Instance.states.GameRestart();
    }
}
