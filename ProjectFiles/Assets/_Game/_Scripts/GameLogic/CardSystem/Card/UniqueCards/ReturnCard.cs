using System.Collections.Generic;
using UnityEngine;

public sealed class ReturnCard : Card
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
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost && availableMoves.Count > 0
            && PlayerDeck.Instance.GetGraveyardCardCount() > 0)
        {
            GameController.Instance.player.DeacreaseMana(ManaCost);
            UseCard(availableMoves);
        }
    }

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {
        if (!isSynced)
        {
            PlayerDeck.Instance.DrawLastFromGraveyard();
            GameController.Instance.states.move.UseCard(GetID(), moves);
            PlayerDeck.Instance.DestroyCardInHand(this);
        }
        else
            Destroy(gameObject);
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
