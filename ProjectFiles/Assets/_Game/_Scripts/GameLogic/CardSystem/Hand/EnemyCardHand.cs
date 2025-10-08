using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCardHand : HandObject
{
    public int CurrentSelectCardIndex { get; private set; }
    public List<GameObject> CardsInHand { get; private set; } = new();

    [SerializeField] float objSelectUpDistance = 0.25f;
    [SerializeField] bool needToHighlightOnHover;
    [SerializeField] Color highlightColor;

    void SetCurrentSelectCard(GameObject cardInHand) => CurrentSelectCardIndex = CardsInHand.IndexOf(cardInHand);

    void ResetCurrentSelectCard(GameObject cardInHand) => CurrentSelectCardIndex = -1;

    public void CardUpDownMove(int cardIndex, bool toUp, bool liftSlightly)
    {
        if (isDealing || cardIndex < 0)
            return;

        GameObject card = CardsInHand[cardIndex];
        float multipleDirect = toUp ? 1 : -1;
        multipleDirect /= liftSlightly ? 2 : 1;

        card.transform.DOComplete();
        card.transform.DOMoveY(card.transform.position.y + objSelectUpDistance * multipleDirect, 0.1f);
    }


    public void AddCard(GameObject obj)
    {
        CardsInHand.Add(obj);
        obj.AddComponent(typeof(BoxCollider2D));
        HoverObject cardInHand = obj.AddComponent<HoverObject>();
        cardInHand.SetSettigns(SetCurrentSelectCard, ResetCurrentSelectCard, needToHighlightOnHover, highlightColor);
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
