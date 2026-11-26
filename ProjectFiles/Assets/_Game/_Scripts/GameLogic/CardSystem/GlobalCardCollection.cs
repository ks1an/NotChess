using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public sealed class GlobalCardCollection : MonoBehaviour
{
    [field: SerializeField] public Dictionary<int, Card> GlobalCardsDictionary { get; private set; } = new();
    [field: SerializeField] public List<GameObject> GlobalCardBacks { get; private set; } = new();

    [SerializeField] private List<Card> cards = new();

    private static readonly Regex ImportantWordsPattern = new(
        @"(<color[^>]*>)?<b>([^<]+)</b>(</color>)?",
        RegexOptions.Compiled | RegexOptions.Singleline);

    // ===== Init =====

    public void CreateGlobalCards()
    {
        GlobalCardsDictionary.Clear();

        if (cards == null || cards.Count == 0)
        {
            Debug.LogWarning($"{name}: cards list is empty", this);
            return;
        }

        foreach (Card card in cards)
        {
            if (card == null)
            {
                Debug.LogWarning($"{name}: null entry in cards list, skipped", this);
                continue;
            }

            int id = card.ID;
            if (GlobalCardsDictionary.ContainsKey(id))
            {
                Debug.LogError(
                    $"{name}: StableId collision {id} between " +
                    $"'{GlobalCardsDictionary[id].originalCardName}' and '{card.originalCardName}'",
                    card);
                continue;
            }

            GlobalCardsDictionary.Add(id, card);
        }
    }

    // ===== Lookups =====

    public Card GetCardByStableId(int id)
        => GlobalCardsDictionary.TryGetValue(id, out var card) ? card : null;

    public Card GetCardTemplateOfType(Type type)
    {
        if (type == null) return null;
        foreach (Card card in GlobalCardsDictionary.Values)
            if (card != null && type == card.GetType())
                return card;
        return null;
    }

    public Card GetCardTemplateOfType(Card type)
        => type != null ? GetCardTemplateOfType(type.GetType()) : null;

    public SpriteRenderer GetCardBackById(string id)
    {
        if (string.IsNullOrEmpty(id) || GlobalCardBacks == null) return null;
        foreach (var obj in GlobalCardBacks)
            if (obj != null && obj.name == id) return obj.GetComponent<SpriteRenderer>();
        return null;
    }

    // ===== Info =====

    public Dictionary<string, string> GetInfoAboutCard(
        int cardID, bool needName, bool needCost = false, bool needDescribe = false)
    {
        var info = new Dictionary<string, string>(3);

        if (!GlobalCardsDictionary.TryGetValue(cardID, out var card) || card == null)
        {
            Debug.LogWarning($"{name}: card with StableId {cardID} not found", this);
            return info;
        }

        if (needName) info["Name"] = card.originalCardName;
        if (needCost) info["ManaCost"] = card.ManaCost.ToString();
        if (needDescribe) info["Describe"] = card.originalDescription;

        return info;
    }

    // ===== Important words =====

    public string GetImportantWordsFromDescription(int cardID)
    {
        if (!GlobalCardsDictionary.TryGetValue(cardID, out var card) || card == null)
        {
            Debug.LogWarning($"{name}: card with StableId {cardID} not found", this);
            return string.Empty;
        }
        return GetImportantWordsFromDescription(card.originalDescription);
    }

    public string GetImportantWordsFromDescription(string description)
    {
        if (string.IsNullOrEmpty(description)) return string.Empty;

        var sb = new StringBuilder();
        foreach (Match match in ImportantWordsPattern.Matches(description))
        {
            string word = match.Groups[2].Value.Trim();
            if (word.Length == 0) continue;
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(word);
        }
        return sb.ToString();
    }
}