using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class CardHand : MonoBehaviour
{
    public Card CurrentSelectCard { get; private set; }
    public List<Card> CardsInHand { get; private set; } = new();

    [SerializeField] SplineContainer splineContainer;
    [SerializeField] float cardSpacing = 0.1f;
    [SerializeField] float cardSelectUpDistance = 0.25f;

    bool isDealing;

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
        if(card != null)
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
        card.transform.DOMoveY(card.transform.position.y + cardSelectUpDistance*multipleDirect, 0.1f);
    }

    #region +-CardInHand
    public IEnumerator AddCard(Card card)
    {
        CardsInHand.Add(card);
        yield return UpdateCardPos(0.15f);
    }

    public IEnumerator RemoveCard(Card card)
    {
        CardsInHand.Remove(card);
        Destroy(card.gameObject);
        yield return UpdateCardPos(0.15f);
    }

    public void RemoveAllCards() => CardsInHand.Clear();
    #endregion

    IEnumerator UpdateCardPos(float duration)
    {
        if (CardsInHand.Count == 0) yield break;

        isDealing = true;
        float firtsCardPos = 0.5f - (CardsInHand.Count - 1) * cardSpacing / 2;
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < CardsInHand.Count; i++)
        {
            float pos = firtsCardPos + i * cardSpacing;
            Vector3 splinePos = spline.EvaluatePosition(pos);
            Vector3 forward = spline.EvaluateTangent(pos);
            Vector3 up = spline.EvaluateUpVector(pos);
            Quaternion rot = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            CardsInHand[i].transform.DOMove(splinePos + transform.position, duration);
            CardsInHand[i].transform.DOLocalRotate(rot.eulerAngles, duration);
        }

        yield return new WaitForSeconds(duration);
        isDealing = false;
    }
}
