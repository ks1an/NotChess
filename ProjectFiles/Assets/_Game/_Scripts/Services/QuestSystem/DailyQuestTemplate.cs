using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Quests/Daily Quest Template")]
public class DailyQuestTemplate : ScriptableObject
{
    [Header("Basic Info")]
    public string questId;
    [TextArea] public string descriptionTemplate;
    public Sprite sprite;
    public QuestMetric metric;
    public QuestTier tier;

    [Header("Target Range (For Seed Generation)")]
    public int minTarget = 5;
    public int maxTarget = 15;
    public int stepForRandom = 1;

    [Header("Allowed Game Modes")]
    public List<string> allowedGameModes = new()
    {
        MatchStates.GameMode_Matchmaking,
    };

    [Header("Reward")]
    public List<QuestReward> rewards = new();
}