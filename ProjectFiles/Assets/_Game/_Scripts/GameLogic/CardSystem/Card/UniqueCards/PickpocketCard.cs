using System.Collections.Generic;
using UnityEngine;

public class PickpocketCard : Card
{
    public override void Init()
    {
        base.Init();
        PlayerDeck.Instance.DestroyCardInHand(this);
    }

    #region OnDrag
    void OnMouseDown()
    {
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost)
        {
            if (IsUseOnlyMyTurn)
            {
                if (GameController.Instance.player.IsMyTurnOrNot())
                {
                    Hand.SetCurrentSelectCard(this);
                    CardUI.SetBorderColor(BoarderColorOnDrag);
                }
            }
            else
            {
                Hand.SetCurrentSelectCard(this);
                CardUI.SetBorderColor(BoarderColorOnDrag);
            }

        }
    }

    void OnMouseUp()
    {
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost &&
            EnemyDeck.Instance.hand.CurrentSelectCardIndex > -1)
        {
            GameController.Instance.player.DeacreaseMana(ManaCost);
            UseCard(availableMoves);
            Hand.ResetCurrentSelectCard(null);
        }
        else if (PlayerDeck.Instance.hand.CurrentSelectCard == this)
        {
            Hand.ResetCurrentSelectCard(this);
        }

        CardUI.SetBorderColor(ColorBorder);
    }
    #endregion

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {
       /* if (!isSynced)
        {
            List<Vector2Int> targetCards = new()
            {
                new Vector2Int(EnemyDeck.Instance.hand.CurrentSelectCardIndex, 0)
            };

            GameController.Instance.states.UseCard(ID, targetCards);
            PlayerDeck.Instance.DestroyCardInHand(this);
        }
        else
        {
            int cardID = PlayerDeck.Instance.hand.CardsInHand[moves[0][0]].SelfGetID();
            EnemyDeck.Instance.DrawCardInHand(GameController.Instance.globalCardCollection.GlobalCardsDictionary[cardID]);
            PlayerDeck.Instance.DestroyCardInHand(PlayerDeck.Instance.hand.CardsInHand[moves[0][0]]);
            Destroy(gameObject);
        }*/
    }
}
