using UnityEngine;
using UnityEngine.Localization;

public sealed class Muligan
{
    LocalizedStringTable localTable;
    GameObject buttonMuliganObj;
    int startCardsCount, countOfUsed, maxMuligan;

    public void SetDefault(GameObject muliganBttnObj, LocalizedStringTable localTable)
    {
        buttonMuliganObj = muliganBttnObj;
        countOfUsed = 0;
        maxMuligan = GameController.Instance.settings.startCards;
        startCardsCount = GameController.Instance.settings.startCards;
        this.localTable = localTable;
    }

    public void TryDoMuligan() =>
        ModalViewWindowController.Instance.ShowHorizontalWithLocalize
            (
            localTable, "Muligan", false, false,
            () => { }, DoMuligan
            );

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
