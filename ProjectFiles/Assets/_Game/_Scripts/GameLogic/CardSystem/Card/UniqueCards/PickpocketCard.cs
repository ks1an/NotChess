using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PickpocketCard : Card
{
    bool enemyCardFromHandWasSelected;

    public override void Init(Team teamWhoHave)
    {
        base.Init(teamWhoHave);

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
        if (!cardInited) Debug.LogError("Card NOT inited but used " + originalCardName);

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

                GameController.Instance.states.move.UseCard(GetID(), targetCards, teamWhoHave);
                PlayerDeck.Instance.DestroyCardInHand(this);
            }
            else
            {
                GameController.Instance.netMatch.cardSync.Player_DrawCardInHandRpc(PlayerCardHand.Instance.CardsInHand[moves[0][0]].GetID());
                PlayerDeck.Instance.DestroyCardInHand(PlayerCardHand.Instance.CardsInHand[moves[0][0]], false);
                KillCard();
            }
        else
        {
            List<Vector2Int> targetCards = new()
                    {
                        new Vector2Int(EnemyDeck.Instance.hand.CurrentSelectCardIndex, 0)
                    };

            GameObject cardInHandEnemy = EnemyDeck.Instance.hand.CardGameobjectsInHand[targetCards[0][0]];
            PlayerDeck.Instance.DrawCardInHand(EnemyDeck.Instance.hand.cardsInHand[cardInHandEnemy]);
            EnemyDeck.Instance.DestroyCardInHand(cardInHandEnemy);
            GameController.Instance.states.move.UseCard(GetID(), targetCards, teamWhoHave);

            if (teamWhoHave == GameController.Instance.player.GetLocalPlayerTeam())
                PlayerDeck.Instance.DestroyCardInHand(this);
            else
            {
                //TODO: Effectively removes a random card of the type. Creates ambiguity for the player. Needs to be changed.
                EnemyCardHand hand = EnemyDeck.Instance.hand;
                EnemyDeck.Instance.DestroyCardInHand(hand.CardGameobjectsInHand[hand.GetIndexOfCardInHandByType(this)], true);
            }
        }
    }
}
