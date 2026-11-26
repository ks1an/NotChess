using System;
using System.IO;
using UnityEngine;

public static class DeckRepository
{
    public const string DefaultFileName = "MyDeck.json";

    public static string GetPath(string fileName = DefaultFileName)
        => Path.Combine(Application.persistentDataPath, fileName);

    public static bool Save(DeckData data, string fileName = DefaultFileName)
    {
        if (data == null) { Debug.LogError("DeckData is null"); return false; }
        try
        {
            string path = GetPath(fileName);
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Replace(tmp, path, null);
            else File.Move(tmp, path);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Deck save failed: {e}");
            return false;
        }
    }

    public static DeckData Load(string fileName = DefaultFileName)
    {
        try
        {
            string path = GetPath(fileName);
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<DeckData>(json);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Deck load failed: {e}");
            return null;
        }
    }

    public static bool Exists(string fileName = DefaultFileName)
        => File.Exists(GetPath(fileName));

    public static void Delete(string fileName = DefaultFileName)
    {
        try { File.Delete(GetPath(fileName)); }
        catch (Exception e) { Debug.LogError($"Deck delete failed: {e}"); }
    }
}