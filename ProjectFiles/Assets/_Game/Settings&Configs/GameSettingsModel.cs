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
    public ReactiveProperty<int> TargetFrameRate_Board = new();
    public ReactiveProperty<int> TargetFrameRate_Menu = new();

    //Audio
    public ReactiveProperty<float> MusicVolume = new();
    public ReactiveProperty<float> EffectVolume = new();

    public GameSettingsModel(GameSettingsData data)
    {
        _data = data;

        //Game
        DontTranslateNameOfCard.Value = _data.dontTranslateNameOfCard;

        //Graphic
        FullscreenMode.Value = _data.fullscreenMode;
        AntiAliasing.Value = _data.antiAliasing;
        //
        VSync.Value = _data.vSyncCount;
        TargetFrameRate_Board.Value = _data.targetFrameRate_BoardScene;
        TargetFrameRate_Menu.Value = _data.targetFrameRate_MenuScene;

        //Audio
        MusicVolume.Value = _data.musicVolume; if (MusicVolume.Value > 1f) Debug.LogWarning("a number from 0 to 1 is recommended for musicVolume");
        EffectVolume.Value = _data.effectsVolume; if (EffectVolume.Value > 1f) Debug.LogWarning("a number from 0 to 1 is recommended for effectVolume");

        ApplySettings();
    }

    public void ApplySettings()
    {
        //GRAPHIC
        Application.targetFrameRate = TargetFrameRate_Menu.Value;
        Screen.fullScreenMode = (FullScreenMode)FullscreenMode.Value;
        QualitySettings.antiAliasing = AntiAliasing.Value;
        QualitySettings.vSyncCount = VSync.Value;

        UpdateData();
        SaveData();
    }



    void UpdateData()
    {
        //game
        _data.dontTranslateNameOfCard = DontTranslateNameOfCard.Value;

        //GRAPHIC
        _data.fullscreenMode = FullscreenMode.Value;
        //
        _data.antiAliasing = AntiAliasing.Value;
        _data.vSyncCount = VSync.Value;
        _data.targetFrameRate_BoardScene = TargetFrameRate_Board.Value;
        _data.targetFrameRate_MenuScene = TargetFrameRate_Menu.Value;

        //Audio
        _data.musicVolume = MusicVolume.Value;
        _data.effectsVolume = EffectVolume.Value;
    }    
    
    public void SaveData()
    {
        UpdateData();
        string jsonData = JsonUtility.ToJson(_data, true);
        File.WriteAllText(Application.persistentDataPath + "/gamesettings.json", jsonData);
    }
}

[Serializable]
public class GameSettingsData
{
    //game
    public bool dontTranslateNameOfCard;

    //GRAPHIC
    public int fullscreenMode,
        antiAliasing,
        vSyncCount;
    public int targetFrameRate_BoardScene, targetFrameRate_MenuScene;

    //Audio
    public float musicVolume, effectsVolume;
}