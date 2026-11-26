using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance { get; private set; }

    [Header("Quest Templates Pools")]
    [SerializeField] private List<DailyQuestTemplate> easyTemplates = new();
    [SerializeField] private List<DailyQuestTemplate> mediumTemplates = new();
    [SerializeField] private List<DailyQuestTemplate> hardTemplates = new();

    [Header("Settings")]
    [SerializeField] int countEasyQuest;
    [SerializeField] int countMediumQuest, countHardQuest;

    [Header("For testing")]
    [SerializeField] private bool useTestQuestOverride = false;
    [SerializeField] private DailyQuestTemplate testQuestTemplate;
    [SerializeField] private int testTargetOverride = 10;
    [SerializeField] private QuestTier testTierOverride = QuestTier.Easy;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CheckAndLoadDailyQuests();
    }

    public void CheckAndLoadDailyQuests()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (useTestQuestOverride && testQuestTemplate != null)
        {
            ApplyTestQuest();
            return;
        }
#endif

        int dateSeed = DateTime.UtcNow.Year * 10000 + DateTime.UtcNow.Month * 100 + DateTime.UtcNow.Day;
        string currentDateStr = dateSeed.ToString();

        //GameController.Instance.playerData.LastQuestDate = "0";

        if (GameController.Instance.playerData.LastQuestDate != currentDateStr)
        {
            GenerateDailyQuests(dateSeed);
            GameController.Instance.playerData.LastQuestDate = currentDateStr;
            GameController.Instance.playerData.SaveData(); 
        }
    }

    private void GenerateDailyQuests(int seed)
    {
        UnityEngine.Random.InitState(seed);
        List<DailyQuestData> newQuests = new();

        int easyCount = Mathf.Min(countEasyQuest, easyTemplates.Count);
        int mediumCount = Mathf.Min(countMediumQuest, mediumTemplates.Count);
        int hardCount = Mathf.Min(countHardQuest, hardTemplates.Count);

        PickQuestsFromPool(easyTemplates, easyCount, newQuests);
        PickQuestsFromPool(mediumTemplates, mediumCount, newQuests);
        PickQuestsFromPool(hardTemplates, hardCount, newQuests);

        GameController.Instance.playerData.ActiveQuests.Value = newQuests;
        GameController.Instance.playerData.QuestsViewed = false;
        GameController.Instance.playerData.SaveData();
    }

    private void PickQuestsFromPool(List<DailyQuestTemplate> pool, int count, List<DailyQuestData> resultList)
    {
        if (pool == null || pool.Count == 0) return;

        List<DailyQuestTemplate> available = new(pool);

        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, available.Count);
            DailyQuestTemplate template = available[randomIndex];
            available.RemoveAt(randomIndex);

            int minMultiplier = Mathf.CeilToInt((float)template.minTarget / template.stepForRandom);
            int maxMultiplier = Mathf.FloorToInt((float)template.maxTarget / template.stepForRandom);

            int target = UnityEngine.Random.Range(minMultiplier, maxMultiplier + 1) * template.stepForRandom;
            resultList.Add(new DailyQuestData(template, target));
        }
    }

    public void OnMatchFinished(PlayerMatchStats stats)
    {
        var quests = GameController.Instance.playerData.ActiveQuests.Value;
        bool hasChanges = false;

        foreach (var quest in quests)
        {
            if (quest.isCompleted) continue;
            if (!quest.IsAllowedForGameMode(stats.gamemode)) continue;

            int amountToAdd = quest.ExtractValueFromStats(stats);
            if (amountToAdd > 0)
            {
                quest.currentProgress += amountToAdd;
                hasChanges = true;

                if (quest.currentProgress >= quest.targetProgress)
                {
                    quest.currentProgress = quest.targetProgress;
                    quest.isCompleted = true;
                    GiveRewardsToPlayer(GetQuestRewardList(quest.questId));
                }
            }
        }

        if (hasChanges)
        {
            GameController.Instance.playerData.ActiveQuests.Value = quests;
            GameController.Instance.playerData.QuestsViewed = false;
            GameController.Instance.playerData.SaveData();
        }
    }

    [ContextMenu("Apply Test Quest Now")]
    public void ApplyTestQuest()
    {
        if (testQuestTemplate == null)
        {
            Debug.LogWarning("[DailyQuestManager] Не выбран testQuestTemplate для принудительного теста!");
            return;
        }

        DailyQuestData testQuest = new DailyQuestData(testQuestTemplate, testTargetOverride)
        {
            tier = testTierOverride
        };

        List<DailyQuestData> testList = new() { testQuest };

        GameController.Instance.playerData.ActiveQuests.Value = testList;
        GameController.Instance.playerData.SaveData();    }

    public void MarkQuestsAsViewed()
    {
        if (GameController.Instance.playerData != null && !GameController.Instance.playerData.QuestsViewed)
        {
            GameController.Instance.playerData.QuestsViewed = true;
            GameController.Instance.playerData.SaveData();
        }
    }

    private void GiveRewardsToPlayer(List<QuestReward> questRewards)
    {
        if (questRewards == null || questRewards.Count == 0) return;

        foreach (var reward in questRewards)
        {
            switch (reward.resourceType)
            {
                case ResourceType.MagicCrystals:
                    GameController.Instance.playerData.MagicCrystals.Value += reward.amount;
                    Debug.Log($"[Economy] Получено: {reward.amount} Маг. кристаллов.");
                    break;
            }
        }

        GameController.Instance.playerData.SaveData();
    }

    #region Get
    public Sprite GetQuestSprite(string questId)
    {
        if (useTestQuestOverride && testQuestTemplate != null && testQuestTemplate.questId == questId)
        {
            return testQuestTemplate.sprite;
        }

        var template = FindTemplate(questId);
        return template != null ? template.sprite : null;
    }

    public List<QuestReward> GetQuestRewardList(string questId)
    {
        if (useTestQuestOverride && testQuestTemplate != null && testQuestTemplate.questId == questId)
        {
            return testQuestTemplate.rewards;
        }

        var template = FindTemplate(questId);
        return template != null ? template.rewards : null;
    }

    public DailyQuestTemplate FindTemplate(string questId)
    {
        foreach (var t in easyTemplates) if (t != null && t.questId == questId) return t;
        foreach (var t in mediumTemplates) if (t != null && t.questId == questId) return t;
        foreach (var t in hardTemplates) if (t != null && t.questId == questId) return t;
        return null;
    }
    #endregion
}