using System.Collections.Generic;
using UnityEngine;

public class DistantRelativeCard : Card
{
    [SerializeField, Min(0)] int amountGetMana, amountLostMana, afterTurnsRepayDebt;

    public override void Init()
    {
        base.Init();
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
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost && availableMoves.Count > 0)
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
        if (GameController.Instance.states.isNetMatch)
            if (!isSynced)
            {
                GameController.Instance.player.IncreaseMana(amountGetMana);
                TurnTimer.GetInstance().StartTimer(
                    afterTurnsRepayDebt,
                    DoAfterTurnsRepayDebt,
                    null,
                    out TurnTimerSubscriber sub
                    );
                GameController.Instance.states.UseCard(GetID(), moves);
                PlayerDeck.Instance.DestroyCardInHand(this);
            }
            else
            {
                Destroy(gameObject);
            }
        else
        {
            GameController.Instance.player.IncreaseMana(amountGetMana);
            TurnTimer.GetInstance().StartTimer(
                afterTurnsRepayDebt, 
                DoAfterTurnsRepayDebt,
                null,
                out TurnTimerSubscriber sub
                );
            GameController.Instance.states.UseCard(GetID(), moves);
            PlayerDeck.Instance.DestroyCardInHand(this);
        }
    }

    void DoAfterTurnsRepayDebt()
    {
        int playerMana = GameController.Instance.player.GetCurrentMana();
        if (playerMana >= amountGetMana)
        {
            GameController.Instance.player.DeacreaseMana(amountLostMana);
        }
        else
        {
            TurnTimer.GetInstance().StartTimer(
                afterTurnsRepayDebt,
                DoAfterTurnsRepayDebt,
                null,
                out TurnTimerSubscriber sub
                );
        }
    }

    #region AvailableMoves
    public override List<Vector2Int> GetAvailableMoves(int maxX, int maxY, int hoverX, int hoverY)
    {
        List<Vector2Int> availables = new();
        if (hoverX < 0 || hoverY < 0)
        {
            availableMoves = availables;
            return availables;
        }

        if (IsAvailableMove(hoverX, hoverY))
            availables.Add(new Vector2Int(hoverX, hoverY));

        availableMoves = availables;
        return availables;
    }

    public override bool IsAvailableMove(int targetX, int targetY)
    {
        if (Board.Instance.tilesController.tiles[targetX, targetY] != null)
            return true;
        return false;
    }
    #endregion
}
