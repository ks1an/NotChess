using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardCollection")]
public class CardCollection : ScriptableObject
{
    [field: SerializeField] public List<Card> CardsInCollection { get; private set; }

    public void RemoveCardFromCollection(Card card)
    {
        if (CardsInCollection.Contains(card))
            CardsInCollection.Remove(card);
        else
            Debug.LogWarning("Card in not present in collection, but you try remove card");
    }

    public void AddCardToCollection(Card card)
    {
        CardsInCollection.Add(card);
    }
}
