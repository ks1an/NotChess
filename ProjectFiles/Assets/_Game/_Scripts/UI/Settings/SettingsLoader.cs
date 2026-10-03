using System.IO;
using UnityEngine;

public class SettingsLoader
{
    GameSettingsData gameData;
    PlayerData playerData;

    internal void LoadGameSettings()
    {
        bool isMobile = Application.isMobilePlatform;
        try
        {
            gameData = JsonUtility.FromJson<GameSettingsData>(File.ReadAllText(Application.persistentDataPath + "/gamesettings.json"));
        }
        catch
        {
            gameData = new()
            {
                //SetDeffault;
                //GRAPHIC
                antiAliasing = isMobile ? 0 : 3, //0 - Disabled; 3 - MSAA 8x
                //
                fullscreenMode = 1,  //Fuullscreen window
                //
                vSyncCount = isMobile ? 0 : 1,
                targetFrameRate_BoardScene = isMobile ? 60 : -1,
                targetFrameRate_MenuScene = isMobile ? 30 : -1,

                //Audio
                musicVolume = 0.25f,
                effectsVolume = 0.50f
            };
        }

        GameSettingsModel model = new(gameData);
        GameController.Instance.UpdateGameSettings(model);
    }

    internal void LoadPlayerData()
    {
        try
        {
            playerData = JsonUtility.FromJson<PlayerData>(File.ReadAllText(Application.persistentDataPath + "/playerData.json"));
        }
        catch
        {
            playerData = new()
            {
                //SetDeffault;
                playerName = "Player45510",
            };
        }
        PlayerDataModel model = new(playerData);
        GameController.Instance.UpdatePlayerData(model);
    }
}
