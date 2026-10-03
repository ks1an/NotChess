using System;
using System.IO;
using UnityEngine;

public class PlayerDataModel
{
    readonly PlayerData _data;

    public ReactiveProperty<string> PlayerName = new();

    public PlayerDataModel(PlayerData data)
    {
        _data = data;

        PlayerName.Value = _data.playerName;

        UpdateData();
    }

    void UpdateData()
    {
        _data.playerName = PlayerName.Value;
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
}