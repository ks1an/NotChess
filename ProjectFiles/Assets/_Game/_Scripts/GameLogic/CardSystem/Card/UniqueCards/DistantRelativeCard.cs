using System.Collections.Generic;
using UnityEngine;

public class DistantRelativeCard : Card
{
    [SerializeField, Min(0)] int amountGetMana, amountLostMana, afterTurnsRepayDebt;

    public override void Init(Team teamWhoHave)
    {
        base.Init(teamWhoHave);
    }

    #region OnDrag
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
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost && availableMoves.Count > 0)
        {
            GameController.Instance.player.DeacreaseMana(ManaCost);
            UseCard(availableMoves);
        }
    }
    #endregion

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {
        if (!cardInited) Debug.LogError("Card NOT inited but used " + originalCardName);

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
                GameController.Instance.states.move.UseCard(GetID(), moves, teamWhoHave);
                PlayerDeck.Instance.DestroyCardInHand(this);
            }
            else
            {
                KillCard();
            }
        else
        {
            TurnTimer.GetInstance().StartTimer(
                afterTurnsRepayDebt,
                DoAfterTurnsRepayDebt,
                null,
                out TurnTimerSubscriber sub
                );
            GameController.Instance.states.move.UseCard(GetID(), moves, teamWhoHave);

            if (teamWhoHave == GameController.Instance.player.GetLocalPlayerTeam())
            {
                GameController.Instance.player.IncreaseMana(amountGetMana);
                PlayerDeck.Instance.DestroyCardInHand(this);
            }
            else
            {
                GameController.Instance.enemy.IncreaseMana(amountGetMana);
                //TODO: Effectively removes a random card of the type. Creates ambiguity for the player. Needs to be changed.
                EnemyCardHand hand = EnemyDeck.Instance.hand;
                EnemyDeck.Instance.DestroyCardInHand(hand.CardGameobjectsInHand[hand.GetIndexOfCardInHandByType(this)], true);
            }
        }
    }

    void DoAfterTurnsRepayDebt()
    {
        if (teamWhoHave == GameController.Instance.player.GetLocalPlayerTeam())
        {
            if (GameController.Instance.player.GetCurrentMana() >= amountGetMana)
            {
                GameController.Instance.player.DeacreaseMana(amountLostMana);
                return;
            }
        }
        else
        {
            if (GameController.Instance.enemy.GetCurrentMana() >= amountGetMana)
            {
                GameController.Instance.enemy.DeacreaseMana(amountLostMana);
                return;
            }
        }

        TurnTimer.GetInstance().StartTimer(
        afterTurnsRepayDebt,
        DoAfterTurnsRepayDebt,
        null,
        out TurnTimerSubscriber sub
        );
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
