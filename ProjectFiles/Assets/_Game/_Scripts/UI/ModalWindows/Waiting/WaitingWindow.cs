using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

public sealed class WaitingWindow : MonoBehaviour
{
    [SerializeField] LocalizedStringTable localStringLoadPhrases;
    [SerializeField] TextMeshProUGUI displayText, displayTitle;
    [SerializeField] Button exitBttn;

    string baseTitle;
    bool timerEnabled;
    float timerStartTime;

    public void SetEnableWaitingWindow(string title = null, string content = null, 
        Action onExitBttn = null, bool enableTimer = false)
    {
        if (title != null)
        {
            displayTitle.gameObject.SetActive(true);
            displayTitle.text = title;
            baseTitle = title;
        }
        else
        {
            displayTitle.gameObject.SetActive(false);
            baseTitle = null;
        }

        if (content != null)
            displayText.text = content;
        else
            SwitchRandomLoadTxt();

        if (onExitBttn != null)
        {
            exitBttn.gameObject.SetActive(true);
            exitBttn.onClick.RemoveAllListeners();
            exitBttn.onClick.AddListener(() => onExitBttn?.Invoke());
        }
        else
            exitBttn.gameObject.SetActive(false);

        timerEnabled = enableTimer;
        timerStartTime = Time.realtimeSinceStartup;

        if (!timerEnabled && baseTitle != null)
            displayTitle.text = baseTitle;
    }

    void Update()
    {
        if (!timerEnabled || baseTitle == null) return;

        int seconds = Mathf.CeilToInt(Time.realtimeSinceStartup - timerStartTime);
        displayTitle.text = $"{baseTitle} {seconds}";
    }


    public void SwitchRandomLoadTxt()
    {
        GetLocalizedTable(out StringTable table);
        displayText.text = GetLocalizedPhrase(table, UnityEngine.Random.Range(0, table.Count));
    }

    string GetLocalizedPhrase(StringTable table, int index)
    {
        return table.GetEntry(table.SharedData.Entries[index].Key).GetLocalizedString();
    }

    StringTable GetLocalizedTable(out StringTable table)
    {
        table = localStringLoadPhrases.GetTable();
        if (table == null)
            Debug.LogError("Not find table: " + table);
        return table;
    }
}
