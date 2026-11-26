using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerDataModel
{
    readonly PlayerData _data;

    public ReactiveProperty<string> PlayerName = new();
    public bool TutorialCompleted;

    public ReactiveProperty<List<DailyQuestData>> ActiveQuests = new();
    public string LastQuestDate;
    public bool QuestsViewed;

    public ReactiveProperty<int> MagicCrystals = new();

    public PlayerDataModel(PlayerData data)
    {
        _data = data;

        PlayerName.Value = _data.playerName;
        TutorialCompleted = _data.tutorialCompleted;
        LastQuestDate = _data.lastQuestDate;
        ActiveQuests.Value = _data.activeQuests ?? new List<DailyQuestData>();
        QuestsViewed = _data.questsViewed;

        MagicCrystals.Value = _data.magicCrystals;

        UpdateData();
    }

    void UpdateData()
    {
        _data.playerName = PlayerName.Value;
        _data.tutorialCompleted = TutorialCompleted;

        _data.lastQuestDate = LastQuestDate;
        _data.activeQuests = ActiveQuests.Value;
        _data.questsViewed = QuestsViewed;

        _data.magicCrystals = MagicCrystals.Value;
    }

    public void SaveData()
    {
        UpdateData();
        string jsonData = JsonUtility.ToJson(_data, true);
        File.WriteAllText(Application.persistentDataPath + "/playerData.json", jsonData);
    }
}

[Serializable]
public class PlayerData
{
    public string playerName;

    public string lastQuestDate;
    public List<DailyQuestData> activeQuests = new();
    public bool tutorialCompleted;
    public bool questsViewed;

    public int magicCrystals;
}