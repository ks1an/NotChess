using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardHand : HandObject
{
    public Card CurrentSelectCard { get; private set; }
    public List<Card> CardsInHand { get; private set; } = new();
    [SerializeField] protected float objSelectUpDistance = 0.25f;

    public void SetCurrentSelectCard(Card card)
    {
        if (card == null)
            ResetCurrentSelectCard(null);
        else if (CardsInHand.Contains(card))
        {
            CurrentSelectCard = card;
            CardUpDownMove(card, true, false);
        }
        else
            ResetCurrentSelectCard(card);
    }
    public void ResetCurrentSelectCard(Card card)
    {
        if (card != null)
            CardUpDownMove(card, false, false);
        CurrentSelectCard = null;
    }

    public void CardUpDownMove(Card card, bool toUp, bool liftSlightly)
    {
        if (isDealing)
            return;

        float multipleDirect = toUp ? 1 : -1;
        multipleDirect /= liftSlightly ? 2 : 1;

        card.transform.DOComplete();
        card.transform.DOMoveY(card.transform.position.y + objSelectUpDistance * multipleDirect, 0.1f);

        if (GameController.Instance.states.isNetMatch)
            GameController.Instance.netMatch.cardSync.Enemy_CardHandUpDownMoveRpc(CardsInHand.IndexOf(card),toUp,liftSlightly);
    }

    public void AddCard(Card card)
    {
        CardsInHand.Add(card);
        StartCoroutine(AddObj(card.gameObject));
    }

    public void RemoveCard(Card card)
    {
        if (CurrentSelectCard == card)
            ResetCurrentSelectCard(card);
        card.transform.DOKill();

        CardsInHand.Remove(card);
        StartCoroutine(RemoveObjectInHand(card.gameObject));
    }

    public void RemoveAllCards()
    {
        ResetCurrentSelectCard(null);
        for(int i = 0; i < CardsInHand.Count; i++)
            CardsInHand[i].transform.DOKill();

        CardsInHand.Clear();
        RemoveAllObjects();
    }
}
