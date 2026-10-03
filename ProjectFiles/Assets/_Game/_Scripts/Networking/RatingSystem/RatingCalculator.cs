using System;
using System.Collections.Generic;
using UnityEngine;

public class RatingCalculator : MonoBehaviour
{
    public static RatingCalculator Instance;

    [System.Serializable]
    public struct TierConfig
    {
        [Tooltip("Название лиги (для удобства в инспекторе)")]
        public string tierName;

        [Tooltip("Минимальный MMR для входа в эту лигу")]
        public int minMmr;

        [Tooltip("Начисление MMR за победу")]
        public int winReward;

        [Tooltip("Штраф MMR за поражение (положительное число)")]
        public int lossPenalty;
    }

    [Header("Progress Settings")]
    public float mmrPerLevel = 1000f;

    [Header("Calibration Settings")]
    [SerializeField] private int maxCalibrationGames = 5;
    [SerializeField] private float calibrationBoostMultiplier = 1.25f;

    [Header("Matchmaking Level Delta Multipliers")]
    [SerializeField] private float highDiffMultiplier = 0.20f;
    [SerializeField] private float lowDiffMultiplier = 0.10f; 

    [Header("Tier & MMR Configs")]
    [SerializeField]
    private List<TierConfig> tiers = new()
    {
        new() { tierName = "1",  minMmr = 0,     winReward = 750, lossPenalty = 100 },
        new() { tierName = "11", minMmr = 10000, winReward = 700, lossPenalty = 200 },
        new() { tierName = "21",  minMmr = 20000, winReward = 650, lossPenalty = 350 },
        new() { tierName = "31",   minMmr = 30000, winReward = 600, lossPenalty = 450 },
        new() { tierName = "41",  minMmr = 40000, winReward = 500, lossPenalty = 500 }
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            Debug.LogError("RatingCalculator > 1 in scene");
        }
    }

    public int CalculateDelta(int myMmr, int oppMmr, ResultMatch result, int gamesPlayedInSeason)
    {
        if (result == ResultMatch.Draw) return 0;

        TierConfig currentTier = GetTierForMmr(myMmr);
        bool isWin = result == ResultMatch.Win;
        float baseDelta = isWin ? currentTier.winReward : -Math.Abs(currentTier.lossPenalty);

        float calibrationMult = (gamesPlayedInSeason < maxCalibrationGames) ? calibrationBoostMultiplier : 1.0f;
        float oppStrengthMult = GetOpponentStrengthMultiplier(myMmr, oppMmr, isWin);
        float finalDelta = baseDelta * calibrationMult * oppStrengthMult;

        int roundedDelta = (int)Math.Round(finalDelta);

        if (roundedDelta == 0) roundedDelta = isWin ? 1 : -1;

        return roundedDelta;
    }

    public TierConfig GetTierForMmr(int mmr)
    {
        if (tiers == null || tiers.Count == 0)
        {
            Debug.LogWarning("[RatingCalculator] Tiers list is empty! Using default fallback.");
            return new TierConfig { tierName = "Default", minMmr = 0, winReward = 500, lossPenalty = 500 };
        }

        TierConfig selectedTier = tiers[0];

        for (int i = 0; i < tiers.Count; i++)
        {
            if (mmr >= tiers[i].minMmr)
                selectedTier = tiers[i];
            else
                break;
        }

        return selectedTier;
    }

    public int GetTierFloorMMR(int currentMmr)
    {
        return GetTierForMmr(currentMmr).minMmr;
    }

    public int GetLevelMMR(int myMmr) => (int)(myMmr / mmrPerLevel) + 1;

    public string GetCurrentSeasonKey(DateTime utcNow) => utcNow.ToString("yyyy-MM");

    private float GetOpponentStrengthMultiplier(int myMmr, int oppMmr, bool isWin)
    {
        int myLevel = GetLevelMMR(myMmr);
        int oppLevel = GetLevelMMR(oppMmr);
        int levelDiff = oppLevel - myLevel;

        if (isWin)
        {
            if (levelDiff >= 10) return 1.0f + highDiffMultiplier; // +20% 
            if (levelDiff >= 5) return 1.0f + lowDiffMultiplier;  // +10%
            if (levelDiff <= -10) return 1.0f - highDiffMultiplier; // -20% 
            if (levelDiff <= -5) return 1.0f - lowDiffMultiplier;  // -10%
        }
        else 
        {
            if (levelDiff >= 10) return 1.0f - highDiffMultiplier; // -20% штрафа
            if (levelDiff >= 5) return 1.0f - lowDiffMultiplier;  // -10% штрафа
            if (levelDiff <= -10) return 1.0f + highDiffMultiplier; // +20% штрафа
            if (levelDiff <= -5) return 1.0f + lowDiffMultiplier;  // +10% штрафа
        }

        return 1.0f;
    }
}

public enum ResultMatch
{
    Defeat = 0,
    Draw = 1,
    Win = 2
}
