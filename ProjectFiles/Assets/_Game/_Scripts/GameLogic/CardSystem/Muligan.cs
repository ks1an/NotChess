using UnityEngine;
using UnityEngine.Localization;

public sealed class Muligan
{
    LocalizedStringTable localTable;
    GameObject buttonMuliganObj;
    bool wasUsedCard;
    int startCardsCount, countOfUsed, maxMuligan;

    public void SetDefault(GameObject muliganBttnObj, LocalizedStringTable localTable)
    {
        buttonMuliganObj = muliganBttnObj;
        wasUsedCard = false;
        countOfUsed = 0;
        maxMuligan = GameController.Instance.matchSettings.startCards;
        startCardsCount = GameController.Instance.matchSettings.startCards;
        this.localTable = localTable;
    }

    public void TryDoMuligan() =>
        ModalViewWindowController.Instance.ShowHorizontalWithLocalize
            (
            localTable, "Muligan", false, false,
            () => { }, DoMuligan
            );

    public void HideMuliganButton() => buttonMuliganObj.SetActive(false);
    public void TryShowMuliganButton()
    {
        if(countOfUsed < maxMuligan && !wasUsedCard)
            buttonMuliganObj.SetActive(true);
    }
    public void OnUsedCard()
    {
        wasUsedCard = true;
        HideMuliganButton();
    }

    void DoMuligan()
    {
        countOfUsed++;
        int needToDraw = (countOfUsed <= (startCardsCount - 1)) ? (startCardsCount - countOfUsed) : 1;

        Card[] cards = new Card[PlayerCardHand.Instance.CardsInHand.Count-1];
        for(int i = 0; i < cards.Length; i++)
            cards[i] = GameController.Instance.globalCards.GlobalCardsDictionary[PlayerCardHand.Instance.CardsInHand[i].GetID()];

        PlayerDeck.Instance.DestroyAllCardsInHand(false);
        PlayerDeck.Instance.AddCardsToDeck(cards);
        PlayerDeck.Instance.AddToGraveyard(1);
        PlayerDeck.Instance.DrawHandRandomFromDeck(needToDraw, true);

        if (countOfUsed == maxMuligan)
            HideMuliganButton();
    }
}
