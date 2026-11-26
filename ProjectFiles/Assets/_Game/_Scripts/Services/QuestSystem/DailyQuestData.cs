using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DailyQuestData 
{
    public string questId;
    public string descriptionTemplate;
    public QuestMetric metric;
    public QuestTier tier;
    public int targetProgress;
    public int currentProgress;
    public bool isCompleted;
    public int rewardCrystals;

    [SerializeField] List<string> allowedGameModes = new();

    public DailyQuestData(DailyQuestTemplate template, int generatedTarget)
    {
        questId = template.questId;
        descriptionTemplate = template.descriptionTemplate;
        metric = template.metric;
        tier = template.tier;
        targetProgress = generatedTarget;
        currentProgress = 0;
        isCompleted = false;

        allowedGameModes = new List<string>(template.allowedGameModes);
    }

    public bool IsAllowedForGameMode(string gamemode)
    {
        if (string.IsNullOrEmpty(gamemode)) return false;
        return allowedGameModes.Contains(gamemode);
    }

    public string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{current}", currentProgress.ToString())
            .Replace("{target}", targetProgress.ToString());
    }

    public int ExtractValueFromStats(PlayerMatchStats stats)
    {
        return metric switch
        {
            QuestMetric.WinMatches => stats.isWinner ? 1 : 0,
            QuestMetric.PlacePawns => stats.puttedUnits,
            QuestMetric.DestroyUnits => stats.destroyedEnemyUnits,
            QuestMetric.PlayCards => stats.countOfPlayedCards,
            QuestMetric.CompleteMatches => 1,
            QuestMetric.SpentMana => stats.manaSpent,
            QuestMetric.SpentGraveTokens => stats.graveTokensSpent,
            _ => 0
        };
    }
}

public enum QuestMetric
{
    WinMatches,     
    PlacePawns,   
    DestroyUnits,  
    PlayCards,    
    CompleteMatches,
    SpentMana,
    SpentGraveTokens
}

public enum QuestTier
{
    Hard = 0, 
    Medium = 1,   
    Easy = 2,     
}