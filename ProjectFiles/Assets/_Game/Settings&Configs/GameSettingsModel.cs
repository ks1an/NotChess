using System;
using System.IO;
using UnityEngine;

public class GameSettingsModel
{
    readonly GameSettingsData _data;

    //Player
    public ReactiveProperty<string> PlayerName = new();

    //Game
    public ReactiveProperty<bool> DontTranslateNameOfCard = new();


    //GRAPHIC
    public ReactiveProperty<int> FullscreenMode = new();
    public ReactiveProperty<int> AntiAliasing = new();
    public ReactiveProperty<int> VSync = new();

    //Audio
    public ReactiveProperty<float> MusicVolume = new();
    public ReactiveProperty<float> EffectVolume = new();

    public GameSettingsModel(GameSettingsData data)
    {
        _data = data;

        //Player
        PlayerName.Value = _data.playerName;

        //Game
        DontTranslateNameOfCard.Value = _data.dontTranslateNameOfCard;

        //Graphic
        FullscreenMode.Value = _data.fullscreenMode;
        AntiAliasing.Value = _data.antiAliasing;
        VSync.Value = _data.vSyncCount;

        //Audio
        MusicVolume.Value = _data.musicVolume; if (MusicVolume.Value > 1f) Debug.LogWarning("a number from 0 to 1 is recommended for musicVolume");
        EffectVolume.Value = _data.effectsVolume; if (EffectVolume.Value > 1f) Debug.LogWarning("a number from 0 to 1 is recommended for effectVolume");

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
        UpdateData();
        string jsonData = JsonUtility.ToJson(_data, true);
        File.WriteAllText(Application.persistentDataPath + "/gamesettings.json", jsonData);
    }

    void UpdateData()
    {
        //player
        _data.playerName = PlayerName.Value;

        //game
        _data.dontTranslateNameOfCard = DontTranslateNameOfCard.Value;

        //GRAPHIC
        _data.fullscreenMode = FullscreenMode.Value;
        _data.antiAliasing = AntiAliasing.Value;
        _data.vSyncCount = VSync.Value;

        //Audio
        _data.musicVolume = MusicVolume.Value;
        _data.effectsVolume = EffectVolume.Value;
    }
}

[Serializable]
public class GameSettingsData
{
    //player
    public string playerName;

    //game
    public bool dontTranslateNameOfCard;

    //GRAPHIC
    public int fullscreenMode,
        antiAliasing,
        vSyncCount;

    //Audio
    public float musicVolume, effectsVolume;
}
