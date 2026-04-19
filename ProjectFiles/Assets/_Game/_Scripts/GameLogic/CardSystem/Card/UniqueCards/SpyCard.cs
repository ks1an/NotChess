using System.Collections.Generic;
using UnityEngine;

public class SpyCard : Card
{

    public override void Init(Team teamWhoHave)
    {
        base.Init(teamWhoHave);
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
        if (!cardInited) Debug.LogError("Card NOT inited but used " + originalCardName);

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

                GameController.Instance.states.move.UseCard(GetID(), targetCards, teamWhoHave);
                PlayerDeck.Instance.DestroyCardInHand(this);
            }
            else
            {
                GameController.Instance.netMatch.cardSync.ShowSpyInfoAboutCardRpc(
                    PlayerCardHand.Instance.CardsInHand[moves[0][0]].GetID());
                KillCard();
            }
        else
        {
            List<Vector2Int> targetCards = new()
                    {
                        new Vector2Int(EnemyDeck.Instance.hand.CurrentSelectCardIndex, 0)
                    };

            GameController.Instance.states.move.UseCard(GetID(), targetCards, teamWhoHave);

            if (teamWhoHave == GameController.Instance.player.GetLocalPlayerTeam())
            {
                GameObject cardInHandEnemy = EnemyDeck.Instance.hand.CardGameobjectsInHand[targetCards[0][0]];
                NotificationPanelConroller.Instance.ShowNotification
(GameController.Instance.globalCards.GetImortantWordsFromDescription(EnemyDeck.Instance.hand.cardsInHand[cardInHandEnemy].GetID()), () => { });
                PlayerDeck.Instance.DestroyCardInHand(this);
            }
            else
            {
                Card cardInHandPlayr = PlayerCardHand.Instance.CardsInHand[targetCards[0][0]];
                NotificationPanelConroller.Instance.ShowNotification
(GameController.Instance.globalCards.GetImortantWordsFromDescription(cardInHandPlayr.GetID()), () => { });

                //TODO: Effectively removes a random card of the type. Creates ambiguity for the player. Needs to be changed.
                EnemyCardHand hand = EnemyDeck.Instance.hand;
                EnemyDeck.Instance.DestroyCardInHand(hand.CardGameobjectsInHand[hand.GetIndexOfCardInHandByType(this)], true);
                KillCard();
            }
        }
    }
}
