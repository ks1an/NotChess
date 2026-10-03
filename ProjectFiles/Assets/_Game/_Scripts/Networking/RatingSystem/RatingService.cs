using System;
using System.IO;
using UnityEngine;

public sealed class RatingService : MonoBehaviour
{
    public static RatingService Instance { get; private set; }
    public PlayerRatingData Data { get; private set; }
    public int OpponentMmr { get; private set; } = PlayerRatingData.BaseMmr;

    public event Action<int, int> OnMmrChanged;  // oldMmr, newMmr
    public event Action OnSeasonReset;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
            InitMMRSeason();
        }
        else Destroy(gameObject);
    }

    public void UpdateOldMmrToCur()
    {
        Data.OldMmr = Data.CurMmr;
        Save();
    }

    public void AddMmr(int delta, string reason = null)
    {
        InitMMRSeason();
        Debug.Log(delta);
        if (delta == 0) return;

        UpdateOldMmrToCur();
        Data.CurMmr = Math.Max(0, Data.OldMmr + delta);

        if (!string.IsNullOrEmpty(reason))
            Debug.Log($"[RatingService] MMR {Data.OldMmr} → {Data.CurMmr} (delta {delta}, reason: {reason})");

        OnMmrChanged?.Invoke(Data.OldMmr, Data.CurMmr);
        Save();
    }

    public void SetMmr(int newMmr)
    {
        InitMMRSeason();
        Debug.Log(newMmr);
        UpdateOldMmrToCur();
        Data.CurMmr = Math.Max(0, newMmr);

        if (Data.OldMmr == Data.CurMmr) return;

        OnMmrChanged?.Invoke(Data.OldMmr, Data.CurMmr);
        Save();
    }

    #region Storage
    public void Load()
    {
        try
        {
            Data = JsonUtility.FromJson<PlayerRatingData>(File.ReadAllText(Application.persistentDataPath + "/playerRatingData.json"));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[RatingService] Load failed: {e.Message}");
        }

        if(Data == null)
        {
            Data = new PlayerRatingData();
        }
    }

    public void Save()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(Data, true);
            File.WriteAllText(Application.persistentDataPath + "/playerRatingData.json", jsonData);
        }
        catch (Exception e)
        {
            Debug.LogError($"[RatingService] Save failed: {e.Message}");
        }
    }
    #endregion

    #region Season
    public void InitMMRSeason()
    {
        if (Data == null) Load();

        string currentSeason = RatingCalculator.Instance.GetCurrentSeasonKey(DateTime.UtcNow);

        if (string.IsNullOrEmpty(Data.SeasonKey))
        {
            Data.SeasonKey = currentSeason;
            Save();
            return;
        }

        if (Data.SeasonKey == currentSeason) return;

        Data.OldMmr = Data.CurMmr;
        Data.CurMmr = PlayerRatingData.BaseMmr;
        Data.SeasonGamesPlayed = 0; 
        Data.SeasonKey = currentSeason;

        Save();
        OnSeasonReset?.Invoke();
    }
    #endregion

    #region Match result
    public void ApplyMatchResult(ResultMatch result)
    {
        int opponentMmr = OpponentMmr > 0 ? OpponentMmr : PlayerRatingData.BaseMmr;

        InitMMRSeason();
        UpdateOldMmrToCur();

        int oldMmr = Data.CurMmr;
        int delta = RatingCalculator.Instance.CalculateDelta(oldMmr, opponentMmr, result, Data.SeasonGamesPlayed);
        Data.SeasonGamesPlayed++;

        int targetMmr = oldMmr + delta;
        if (delta < 0)
        {
            int floorMmr = RatingCalculator.Instance.GetTierFloorMMR(oldMmr);
            targetMmr = Math.Max(floorMmr, targetMmr);
        }

        Data.CurMmr = Math.Max(0, targetMmr);

        if (Data.OldMmr == Data.CurMmr)
        {
            Save();
            return;
        }

        OnMmrChanged?.Invoke(Data.OldMmr, Data.CurMmr);
        Save();
    }
    #endregion


    public void CacheOpponentMmr()
    {
        OpponentMmr = GetOpponentMmrFromLobby();
    }

    int GetOpponentMmrFromLobby()
    {
        var lobby = LobbyManager.Instance.GetJoinedLobby();
        if (lobby == null)
        {
            Debug.LogWarning("[RatingService] No joined lobby. Using baseline MMR.");
            return PlayerRatingData.BaseMmr;
        }

        if (lobby.Players == null)
        {
            Debug.LogWarning("[RatingService] Lobby has no players list. Using baseline MMR.");
            return PlayerRatingData.BaseMmr;
        }

        string myId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
        foreach (var player in lobby.Players)
        {
            if (player.Id == myId) continue;

            if (player.Data != null
                && player.Data.TryGetValue(LobbyManager.KEY_PLAYER_MMR, out var obj)
                && int.TryParse(obj.Value, out int mmr))
            {
                return mmr;
            }
        }

        Debug.LogError("Lobby can`t find opponent MMR!");
        return PlayerRatingData.BaseMmr;
    }
}