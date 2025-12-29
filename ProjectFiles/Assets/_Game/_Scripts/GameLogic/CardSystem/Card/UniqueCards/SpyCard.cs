using System.Collections.Generic;
using UnityEngine;

public class SpyCard : Card
{

    public override void Init()
    {
        base.Init();
    }

    protected override void OnCursorDown()
    {
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost)
        {
            if (GameController.Instance.player.IsMyTurnOrNot())
            {
                Hand.TrySetCurrentSelectCard(this);
                CardUI.MouseEnterFromCard(gameObject);
            }
        }

    }

    protected override void DoActionOnMouseUp()
    {
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost &&
            EnemyDeck.Instance.hand.CurrentSelectCardIndex > -1)
        {
            GameController.Instance.player.DeacreaseMana(ManaCost);
            UseCard(availableMoves);
        }
    }

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {
        if ((EnemyDeck.Instance.hand.CurrentSelectCardIndex < 0 && !isSynced))
        {
            Hand.ResetCurrentSelectCard(this);
            return;
        }

        if (GameController.Instance.states.isNetMatch)
            if (!isSynced)
            {
                List<Vector2Int> targetCards = new()
                    {
                        new Vector2Int(EnemyDeck.Instance.hand.CurrentSelectCardIndex, 0)
                    };

                GameController.Instance.states.move.UseCard(GetID(), targetCards);
                PlayerDeck.Instance.DestroyCardInHand(this);
            }
            else
            {
                GameController.Instance.netMatch.cardSync.ShowSpyInfoAboutCardRpc(
                    PlayerDeck.Instance.hand.CardsInHand[moves[0][0]].GetID());
                Destroy(gameObject);
            }
        else
        {
            List<Vector2Int> targetCards = new()
                    {
                        new Vector2Int(EnemyDeck.Instance.hand.CurrentSelectCardIndex, 0)
                    };
            NotificationPanelConroller.Instance.ShowNotification
                (
                GameController.Instance.globalCards.GetImortantWordsFromDescriptionOfCard(
                    PlayerDeck.Instance.cardCollection.CardsInCollection[
                        Random.Range(0, PlayerDeck.Instance.cardCollection.CardsInCollection.Count)].GetID()), () => { }
                );

            GameController.Instance.states.move.UseCard(GetID(), targetCards);
            PlayerDeck.Instance.DestroyCardInHand(this);
        }
    }
}
