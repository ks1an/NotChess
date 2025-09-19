using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public sealed class WaitingWindow : MonoBehaviour
{
    [SerializeField] LocalizedStringTable localStringLoadPhrases;
    [SerializeField] TextMeshProUGUI displayText;

    public void SetRandomLoadText()
    {
        GetLocalizedTable(out StringTable table);
        displayText.text = GetLocalizedPhrase(table, Random.Range(0, table.Count));
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
