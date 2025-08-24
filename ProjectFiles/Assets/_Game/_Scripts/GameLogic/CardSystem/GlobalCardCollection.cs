using System.Collections.Generic;
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
            GlobalCardsDictionary.Add(card.SelfGetID(), card);
        }
    }
}
