using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardHand : HandObject
{
    public Card CurrentSelectCard { get; private set; }
    public List<Card> CardsInHand { get; private set; } = new();
    [SerializeField] protected float objSelectUpDistance = 0.25f;

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

        card.transform.DOMoveY(card.transform.position.y + objSelectUpDistance * multipleDirect, 0.1f);

        if (GameController.Instance.states.isNetMatch)
            GameController.Instance.netMatch.cardSync.Enemy_CardHandUpDownMoveRpc(CardsInHand.IndexOf(card), toUp, liftSlightly);
    }
    #endregion

    public void AddCard(Card card)
    {
        CardsInHand.Add(card);
        StartCoroutine(AddObj(card.gameObject));
    }

    public void RemoveCard(Card card)
    {
        if (CurrentSelectCard == card)
            CurrentSelectCard = null;
        if (CurrentSelectCard == card)
            CurrentSelectCard = null;

        CardsInHand.Remove(card);
        StartCoroutine(RemoveObjectInHand(card.gameObject));
    }

    public void RemoveAllCards()
    {
        for (int i = 0; i < CardsInHand.Count; i++)
            CardsInHand[i].transform.DOKill();

        CurrentSelectCard = null;
        CardsInHand.Clear();
        RemoveAllObjects();
    }
}
