using System.Collections.Generic;
using UnityEngine;

public class PickpocketCard : Card
{
    bool enemyCardFromHandWasSelected;

    public override void Init(Team teamWhoHave)
    {
        base.Init(teamWhoHave);

        enemyCardFromHandWasSelected = false;
    }

    public override bool CanBeUsed()
    {
        if (!CanBeSelected()) return false;
        if (EnemyDeck.Instance == null) return false;
        if (EnemyDeck.Instance.hand.CurrentSelectCardIndex <= -1) return false;
        return true;
    }

    protected override void OnCursorDown()
    {
        if (Hand.TrySetCurrentSelectCard(this))
            CardUI.MouseEnterFromCard(gameObject);
    }

    protected override void DoActionOnMouseUp()
    {
        if (!CanBeUsed()) return;
        availableMoves.Add(new Vector2Int(EnemyDeck.Instance.hand.CurrentSelectCardIndex, 0));
        enemyCardFromHandWasSelected = true;
        base.DoActionOnMouseUp();
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

                GameController.Instance.states.move.UseCard(ID, targetCards, teamWhoHave);
                PlayerDeck.Instance.DestroyCardInHand(this);
            }
            else
            {
                GameController.Instance.netMatch.cardSync.Player_DrawCardInHandRpc(PlayerCardHand.Instance.CardsInHand[moves[0][0]].ID);
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
            GameController.Instance.states.move.UseCard(ID, targetCards, teamWhoHave);

            if (teamWhoHave == GameController.Instance.player.GetTeam())
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
