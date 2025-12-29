using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PickpocketCard : Card
{
    bool enemyCardFromHandWasSelected;

    public override void Init()
    {
        base.Init();
        enemyCardFromHandWasSelected = false;
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
            enemyCardFromHandWasSelected = true;
            GameController.Instance.player.DeacreaseMana(ManaCost);
            UseCard(availableMoves);
        }
    }

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {
        if (!enemyCardFromHandWasSelected && !isSynced)
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

                GameController.Instance.states.move.UseCard(ID, targetCards);
                PlayerDeck.Instance.DestroyCardInHand(this);
            }
            else
            {
                GameController.Instance.netMatch.cardSync.Player_DrawCardInHandRpc(PlayerDeck.Instance.hand.CardsInHand[moves[0][0]].ID);
                PlayerDeck.Instance.DestroyCardInHand(PlayerDeck.Instance.hand.CardsInHand[moves[0][0]]);
                Destroy(gameObject);
            }
        else
        {
            List<Vector2Int> targetCards = new()
                    {
                        new Vector2Int(EnemyDeck.Instance.hand.CurrentSelectCardIndex, 0)
                    };
            PlayerDeck.Instance.DrawCardInHand(GameController.Instance.globalCards.
                GlobalCardsDictionary.ElementAt(Random.Range(0, GameController.Instance.globalCards.GlobalCardsDictionary.Count)).Value);
            EnemyDeck.Instance.DestroyCardInHand(EnemyDeck.Instance.hand.CardsInHand[targetCards[0][0]]);
            GameController.Instance.states.move.UseCard(ID, targetCards);
            PlayerDeck.Instance.DestroyCardInHand(this);
        }
    }
}
