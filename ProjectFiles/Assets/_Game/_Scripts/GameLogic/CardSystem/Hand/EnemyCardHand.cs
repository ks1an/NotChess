using System.Collections.Generic;
using UnityEngine;

public class EnemyCardHand : HandObject
{
    public List<GameObject> CardsInHand { get; private set; } = new();

    public void AddCard(GameObject obj)
    {
        CardsInHand.Add(obj);
        StartCoroutine(AddObj(obj));
    }

    public void RemoveCard(GameObject obj)
    {
        CardsInHand.Remove(obj);
        StartCoroutine(RemoveObjectInHand(obj));
    }

    public void RemoveAllCards()
    {
        CardsInHand.Clear();
        RemoveAllObjects();
    }
}
