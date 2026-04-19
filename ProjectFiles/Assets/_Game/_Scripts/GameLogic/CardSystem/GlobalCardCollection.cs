using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public sealed class GlobalCardCollection : MonoBehaviour
{
    [field: SerializeField] public Dictionary<int, Card> GlobalCardsDictionary { get; private set; } = new();
    [field: SerializeField] public List<GameObject> GlobalCardBacks { get; private set; } = new();

    [field: SerializeField] List<Card> cards;

    public void CreateGlobalCards()
    {
        if (GlobalCardsDictionary.Count > 0)
            GlobalCardsDictionary.Clear();

        foreach (Card card in cards)
        {
            GlobalCardsDictionary.Add(card.GetID(), card);
        }
    }

    public string GetImortantWordsFromDescription(int cardID)
    {
        string descriptionWords = GetInfoAboutCard(cardID, false, false, true)["Describe"];
        string importantWords = "";

        string patternOfImportantWord = @"(?<=<b>).+?(?=</b>)";
        MatchCollection matches = Regex.Matches(descriptionWords, patternOfImportantWord);

        foreach (Match match in matches)
        {
            string cleanedWord = match.Value.Trim();
            if (!string.IsNullOrEmpty(cleanedWord))
            {
                importantWords += cleanedWord + " ";
            }
        }

        return importantWords;
    }

    public string GetImortantWordsFromDescription(string description)
    {

        StringBuilder importantWords = new StringBuilder();

        // Паттерн ищет: <b> в любом месте, затем всё до следующего </b>
        // включая возможные теги цвета внутри или снаружи
        string pattern = @"(<color[^>]*>)?<b>.*?</b>(</color>)?";
        MatchCollection matches = Regex.Matches(description, pattern, RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            string fragment = match.Value.Trim();
            if (!string.IsNullOrEmpty(fragment))
            {
                importantWords.Append(fragment + " ");
            }
        }

        return importantWords.ToString().Trim();
    }

    public Dictionary<string, string> GetInfoAboutCard(int cardID,
        bool needName, bool needCost = false, bool needDescribe = false)
    {
        Dictionary<string, string> infoAboutCard = new();
        Card card = GlobalCardsDictionary[cardID];

        if (needName)
            infoAboutCard.Add("Name", card.originalCardName);
        if (needCost)
            infoAboutCard.Add("ManaCost", card.ManaCost.ToString());
        if (needDescribe)
            infoAboutCard.Add("Describe", card.originalDescription);

        return infoAboutCard;
    }

    public Card GetCardPrefabByType(Card type)
    {
        foreach(Card card in GlobalCardsDictionary.Values)
        {
            if(type.GetType() == card.GetType())
                return card;
        }
        return null;
    }

    public Card GetCardPrefabByType(Type type)
    {
        foreach (Card card in GlobalCardsDictionary.Values)
        {
            if (type == card.GetType())
                return card;
        }
        return null;
    }
}
