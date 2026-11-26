using System.Collections.Generic;
using UnityEngine;

public sealed class ReturnCard : Card
{
    public override void Init(Team teamWhoHave)
    {
        base.Init(teamWhoHave);
    }

    public override bool CanBeSelected()
    {
        var player = GameController.Instance.player;

        if (player.GetCurrentMana() < ManaCost) return false;
        if (player.GetCurrentGraveTokens() < GraveTokensCost) return false;
        if (IsUseOnlyMyTurn && !player.IsMyTurnOrNot()) return false;
        if (PlayerDeck.Instance.GetGraveyardCardCount() <= 0) return false;

        return true;
    }

    public override bool CanBeUsed()
    {
        if (!CanBeSelected()) return false;
        if (availableMoves.Count < 1) return false;
        if (EnemyDeck.Instance == null) return false;

        return true;
    }

    protected override void OnCursorDown()
    {
        if (Hand.TrySetCurrentSelectCard(this))
            CardUI.MouseEnterFromCard(gameObject);
    }

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {
        if (!cardInited) Debug.LogError("Card NOT inited but used " + originalCardName);

        base.UseCard(moves, isSynced);
        if (!isSynced)
        {
            GameController.Instance.states.move.UseCard(ID, moves, teamWhoHave);

            if (teamWhoHave == GameController.Instance.player.GetTeam())
            {
                PlayerDeck.Instance.DrawLastFromGraveyard();
                PlayerDeck.Instance.DestroyCardInHand(this);
            }
            else
            {
                EnemyDeck.Instance.DrawLastFromGraveyard(); 
                //TODO: Effectively removes a random card of the type. Creates ambiguity for the player. Needs to be changed.
                EnemyCardHand hand = EnemyDeck.Instance.hand;
                EnemyDeck.Instance.DestroyCardInHand(hand.CardGameobjectsInHand[hand.GetIndexOfCardInHandByType(this)], true);
            }
        }
        else
            KillCard();
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
