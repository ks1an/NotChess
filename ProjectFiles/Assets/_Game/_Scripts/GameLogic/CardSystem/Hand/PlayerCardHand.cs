using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardHand : HandObject
{
    public static PlayerCardHand Instance { get; private set; }
    public event Action<Card> OnCardAddedInHand;
    public event Action<Card> OnCardRemovedInHand;
    public event Action OnAllCardsRemovedInHand;

    public Card CurrentSelectCard { get; private set; }
    public Card CurrentHoverCard { get; private set; }
    public List<Card> CardsInHand { get; private set; } = new();
    [SerializeField] protected float objSelectUpDistance = 0.25f;

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

    #region Select/Hover Card
    public void TrySetCurrentSelectCard(Card card)
    {
        if (CardsInHand.Contains(card))
            CurrentSelectCard = card;
    }
    public void ResetCurrentSelectCard(Card card)
    {
        if(card == CurrentSelectCard)
            CurrentSelectCard = null;
    }

    public void CardUpDownMove(Card card, bool toUp, bool liftSlightly)
    {
        if (!GameController.Instance.states.isGameStarted || card == null)
            return;

        float multipleDirect = toUp ? 1 : -1;
        multipleDirect /= liftSlightly ? 2 : 1;

        card.transform.DOComplete();

        if (toUp)
            StartCoroutine(UpdateObjPos(objUpdatePosTime / 2, CardsInHand.IndexOf(card)));
        else
            StartCoroutine(UpdateObjPos(objUpdatePosTime / 4));

        if (toUp && liftSlightly)
            CurrentHoverCard = card;
        else
            CurrentHoverCard = null;

            card.transform.DOMoveY(card.transform.position.y + objSelectUpDistance * multipleDirect, 0.1f);

        if (GameController.Instance.states.isNetMatch)
            GameController.Instance.netMatch.cardSync.Enemy_CardHandUpDownMoveRpc(CardsInHand.IndexOf(card), toUp, liftSlightly);
    }
    #endregion

    public void AddCard(Card card)
    {
        CardsInHand.Add(card);

        OnCardAddedInHand?.Invoke(card);
        StartCoroutine(AddObj(card.gameObject));
    }

    public void RemoveCard(Card card)
    {
        if (CurrentSelectCard == card)
            CurrentSelectCard = null;
        if (CurrentSelectCard == card)
            CurrentSelectCard = null;

        CardsInHand.Remove(card);
        OnCardRemovedInHand?.Invoke(card);
        StartCoroutine(RemoveObjectInHand(card.gameObject));
    }

    public void RemoveAllCards()
    {
        for (int i = 0; i < CardsInHand.Count; i++)
            CardsInHand[i].transform.DOKill();

        CardsInHand.Clear();
        RemoveAllObjects();
        OnAllCardsRemovedInHand?.Invoke();
    }

    public Card GetCardFromHand(Card cardType)
    {
        foreach(Card card in CardsInHand)
        {
            if(cardType.GetType() == card.GetType()) return card;
        }
        return null;
    }
}
