using System;
using System.IO;
using UnityEngine;

public class GameSettingsModel
{
    readonly GameSettingsData _data;

    //Game
    public ReactiveProperty<bool> DontTranslateNameOfCard = new();


    //GRAPHIC
    public ReactiveProperty<int> FullscreenMode = new();
    public ReactiveProperty<int> AntiAliasing = new();
    public ReactiveProperty<int> VSync = new();

    public GameSettingsModel(GameSettingsData data)
    {
        _data = data;

        DontTranslateNameOfCard.Value = _data.dontTranslateNameOfCard;

        FullscreenMode.Value = _data.fullscreenMode;
        AntiAliasing.Value = _data.antiAliasing;
        VSync.Value = _data.vSyncCount;

        ApplySettings();
    }

    public void ApplySettings()
    {
        //GRAPHIC
        Screen.fullScreenMode = (FullScreenMode)FullscreenMode.Value;
        QualitySettings.antiAliasing = AntiAliasing.Value;
        QualitySettings.vSyncCount = VSync.Value;

        UpdateData();
        SaveData();
    }

    public void SaveData()
    {
        string jsonData = JsonUtility.ToJson(_data, true);
        File.WriteAllText(Application.persistentDataPath + "/gamesettings.json", jsonData);
    }

    void UpdateData()
    {
        _data.dontTranslateNameOfCard = DontTranslateNameOfCard.Value;

        //GRAPHIC
        _data.fullscreenMode = FullscreenMode.Value;
        _data.antiAliasing = AntiAliasing.Value;
        _data.vSyncCount = VSync.Value;
    }
}

[Serializable]
public class GameSettingsData
{
    public bool dontTranslateNameOfCard;

    //GRAPHIC
    public int fullscreenMode,
        antiAliasing,
        vSyncCount;
}
