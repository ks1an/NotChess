using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;
    public bool hasConsented = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError("AnalyticsManager > 1 on scene");
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();

        if (hasConsented)
        {
            AnalyticsService.Instance.StartDataCollection();
        }
        else
        {
            Debug.Log("Analytics data collection NOT started. Waiting for user consent.");
        }
    }

    bool IsReady()
    {
        return AnalyticsService.Instance != null && hasConsented;
    }

    //####PUBLIC METHODS####

    public void LogMatchStarted(string playerTeam, string opponentType, string gameMode)
    {
        if (!IsReady()) return;

        var e = new CustomEvent("matchStarted")
        {
            { "playerTeam", playerTeam },
            { "opponentType", opponentType },
            { "gameMode", gameMode },
        };
        AnalyticsService.Instance.RecordEvent(e);
    }

    public void LogMatchFinished(string playerTeam, string winnerTeam,
        float durationMin, int movesTotal, string winCondition, string[] deck, string gameMode)
    {
        if (!IsReady()) return;

        var e = new CustomEvent("matchFinished")
        {
            { "playerTeam", playerTeam },
            { "winnerTeam", winnerTeam },
            { "durationMatchMinutes", durationMin },
            { "movesTotal", movesTotal },
            { "winCondition", winCondition },
            { "gameMode", gameMode }
        };
        AnalyticsService.Instance.RecordEvent(e);
        foreach (var cardName in deck)
            LogCardInMatch(cardName, winnerTeam == playerTeam, gameMode);
    }

    public void LogCardPlayed(string cardName, int numberOfTurnWhenUsed)
    {
        if (!IsReady()) return;

        var e = new CustomEvent("cardPlayed")
        {
            {"cardName", cardName },
            {"numberOfTurnWhenUsed",  numberOfTurnWhenUsed}
        };
        AnalyticsService.Instance.RecordEvent(e);
    }

    void LogCardInMatch(string cardName, bool playerWon, string currentGameMode)
    {
        if (!IsReady()) return;

        var e = new CustomEvent("cardInMatch")
        {
            { "cardName", cardName },
            { "playerWon", playerWon },
            { "gameMode", currentGameMode },
        };
        AnalyticsService.Instance.RecordEvent(e);
    }

    public void LogMatchesSeriesEnded(float durationMin, int matchesPlayed, string endReason, int countOfWins, 
        int playerMMR, int enemyMMR)
    {
        if (!IsReady()) return;

        var e = new CustomEvent("matchesSeriesEnded")
        {
            { "durationSeconds", durationMin },
            { "matchesPlayed", matchesPlayed },
            { "endReason", endReason },
            { "countOfWins", countOfWins },
            { "playerMMR", playerMMR },
            { "enemyMMR", enemyMMR },
        };
        AnalyticsService.Instance.RecordEvent(e);
    }
}
