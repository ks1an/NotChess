using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCardHand : HandObject
{
    public static EnemyCardHand Instance { get; private set;  }
    public int CurrentSelectCardIndex { get; private set; }
    public List<GameObject> CardGameobjectsInHand { get; private set; } = new();
    public Dictionary<GameObject, Card> cardsInHand = new();


    [SerializeField] float objSelectUpDistance = 0.25f;
    [SerializeField] bool needToHighlightOnHover;
    [SerializeField] Color highlightColor;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogError("PlayerCardHand > 1 in scene");
            Destroy(this);
        }
    }

    void SetCurrentSelectCard(GameObject cardInHand) => CurrentSelectCardIndex = CardGameobjectsInHand.IndexOf(cardInHand);

    void ResetCurrentSelectCard(GameObject cardInHand) => CurrentSelectCardIndex = -1;

    public void CardUpDownMove(int cardIndex, bool toUp, bool liftSlightly)
    {
        if (isDealing || cardIndex < 0)
            return;

        GameObject card = CardGameobjectsInHand[cardIndex];
        float multipleDirect = toUp ? 1 : -1;
        multipleDirect /= liftSlightly ? 2 : 1;

        card.transform.DOComplete();
        card.transform.DOMoveY(card.transform.position.y + objSelectUpDistance * multipleDirect, 0.1f);

        if (!toUp)
            StartCoroutine(UpdateObjPos(0));
    }


    public void AddCard(GameObject obj, Card logic)
    {
        CardGameobjectsInHand.Add(obj);
        cardsInHand.Add(obj, logic);

        StartCoroutine(AddObj(obj));
        obj.AddComponent(typeof(BoxCollider));
        HoverObject cardInHand = obj.AddComponent<HoverObject>();
        cardInHand.SetSettigns(SetCurrentSelectCard, ResetCurrentSelectCard, needToHighlightOnHover, highlightColor);
        obj.layer = LayerMask.NameToLayer("EnemyCard");
    }

    public void RemoveCard(GameObject obj)
    {
        Card c = cardsInHand.GetValueOrDefault(obj);
        CardGameobjectsInHand.Remove(obj);
        cardsInHand.Remove(obj);

        StartCoroutine(RemoveObjectInHand(obj));
    }

    public void RemoveAllCards()
    {
        CurrentSelectCardIndex = -1;
        CardGameobjectsInHand.Clear();
        cardsInHand.Clear();
        RemoveAllObjects();
    }

    public Card GetCardFromHand(Card cardType)
    {
        foreach (var card in cardsInHand)
        {
            if (cardType.GetType() == card.Value.GetType()) return card.Value;
        }
        return null;
    }
    public int GetIndexOfCardInHandByType(Card cardType)
    {
        foreach (var card in cardsInHand)
        {
            if (cardType.GetType() == card.Value.GetType()) return CardGameobjectsInHand.IndexOf(card.Key);
        }
        return -1;
    }
}
