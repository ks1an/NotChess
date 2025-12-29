using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public sealed class GlobalCardCollection : MonoBehaviour
{
    [field: SerializeField] public Dictionary<int, Card> GlobalCardsDictionary { get; private set; } = new();

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

    public string GetImortantWordsFromDescriptionOfCard(int cardID)
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
}
