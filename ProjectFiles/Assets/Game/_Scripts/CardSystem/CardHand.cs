using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class CardHand : MonoBehaviour
{
    [SerializeField] SplineContainer splineContainer;
    [SerializeField] float cardSpacing = 0.1f;  
    [SerializeField] float cardSelectUp = 0.25f;

    public Card CurrentSelectCard { get; private set; }
    public List<Card> CardsInHand { get; private set; } = new();

    public void SetCurrentSelectCard(Card card)
    {
        if (card == null)
            ResetCurrentSelectCard(null);
        else if (CardsInHand.Contains(card))
        {
            CurrentSelectCard = card;
            card.transform.DOMoveY(cardSelectUp + card.transform.position.y, 0.1f);
        }
        else
            ResetCurrentSelectCard(card);
    }
    public void ResetCurrentSelectCard(Card card)
    {
        if (card != null)
            card.transform.DOMoveY(card.transform.position.y-cardSelectUp, 0.15f);
        CurrentSelectCard = null;
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
        float firtsCardPos = 0.5f - (CardsInHand.Count - 1) * cardSpacing / 2;
        Spline spline = splineContainer.Spline;

        for (int i=0; i < CardsInHand.Count; i++)
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
    }
}
