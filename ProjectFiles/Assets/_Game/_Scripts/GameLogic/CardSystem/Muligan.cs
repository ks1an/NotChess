using UnityEngine;

public sealed class Muligan
{
    GameObject buttonMuliganObj;
    int startCardsCount, countOfUsed, maxMuligan;

    public void SetDefault(GameObject muliganBttnObj)
    {
        buttonMuliganObj = muliganBttnObj;
        countOfUsed = 0;
        maxMuligan = GameController.Instance.settings.startCards;
        startCardsCount = GameController.Instance.settings.startCards;
    }

    public void TryDoMuligan()
    {
        ModalViewWindowController.Instance.ShowHorizontal(false, "Muligan?", "You discard <b>all</b> of your cards in your hand to the <b>deck</b> except for one. " +
            "It goes to the <b>graveyard</b>.\r\nIn exchange, you <b>draw one fewer</b> but greater than zero cards <b>into your hand</b>.",
    false, "Cancel", () => { }, "I'll be lucky", DoMuligan);
    }

    void DoMuligan()
    {
        if (countOfUsed >= maxMuligan)
        {
            buttonMuliganObj.SetActive(false);
            return;
        }

        countOfUsed++;
        int needToDraw = countOfUsed <= startCardsCount - 1 ? startCardsCount - countOfUsed : 1;
        PlayerDeck.Instance.DestroyAllCardsInHand(false);
        PlayerDeck.Instance.DrawHandRandomFromDeck(needToDraw, true);

        PlayerDeck.Instance.AddToGraveyard(1);
        PlayerDeck.Instance.AddToDeck(needToDraw - 1);

    }
}
