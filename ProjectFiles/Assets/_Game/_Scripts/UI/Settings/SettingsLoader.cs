using System.IO;
using UnityEngine;

public class SettingsLoader
{
    GameSettingsData data;

    public void LoadSettings()
    {
        try
        {
            data = JsonUtility.FromJson<GameSettingsData>(File.ReadAllText(Application.persistentDataPath + "/gamesettings.json"));
        }
        catch
        {
            data = new()
            {
                //SetDeffault;
                playerName = "Player45510",

                //GRAPHIC
                antiAliasing = 3, //0 - Disabled; 3 - MSAA 8x
                fullscreenMode = 1,  //Fuullscreen window

                //Audio
                musicVolume = 0.25f,
                effectsVolume = 0.50f
            };
        }
        GameSettingsModel model = new(data);
        GameController.Instance.UpdateGameSettings(model);
    }
}
