using System.Collections.Generic;
using UnityEngine;

public class DailyQuestItemViewModel
{
    public string QuestId { get; }
    public string Description { get; }
    public QuestTier Tier { get; }
    public int CurrentProgress { get; }
    public int TargetProgress { get; }
    public bool IsCompleted { get; }
    public float ProgressNormalized => (float)CurrentProgress / TargetProgress;
    public Sprite SpriteOfQuest => DailyQuestManager.Instance != null ? DailyQuestManager.Instance.GetQuestSprite(QuestId) : null;

    public List<QuestReward> rewards = new();

    public DailyQuestItemViewModel(DailyQuestData data)
    {
        QuestId = data.questId;
        Description = data.GetFormattedDescription();
        Tier = data.tier;
        CurrentProgress = data.currentProgress;
        TargetProgress = data.targetProgress;
        IsCompleted = data.isCompleted;

        rewards = DailyQuestManager.Instance != null ? DailyQuestManager.Instance.GetQuestRewardList(QuestId) : null;
    }
}