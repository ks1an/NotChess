using System.IO;
using UnityEngine;

public class SettingsLoader : MonoBehaviour
{
    GameSettingsData data;

    void Start()
    {
        LoadSettings();
    }

    void LoadSettings()
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
                antiAliasing = 3 //0 - Disabled; 3 - MSAA 8x
            };
        }
        GameSettingsModel model = new(data);
        GameController.Instance.UpdateGameSettings(model);
    }
}
