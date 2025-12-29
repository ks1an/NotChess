using System.Collections.Generic;
using UnityEngine;

public sealed class SacrificeCard : Card
{
    [SerializeField] int manaToAdd;
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
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost && availableMoves.Count > 0)
        {
            GameController.Instance.player.DeacreaseMana(ManaCost);
            UseCard(availableMoves);
        }
    }

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {

        if (!isSynced)
        {
            GameController.Instance.states.move.TryDestroyUnit(moves[0].x, moves[0].y,
                false, GameController.Instance.player.GetLocalPlayerTeam());
            GameController.Instance.player.IncreaseMana(manaToAdd);

            GameController.Instance.states.move.UseCard(ID, moves);
            PlayerDeck.Instance.DestroyCardInHand(this);
        }
        else
        {
            Destroy(gameObject);
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
        if (Board.Instance.tilesController.tiles[targetX, targetY] != null && Board.Instance.piecesController.pieces[targetX, targetY] != null
            && Board.Instance.piecesController.pieces[targetX, targetY].team == GameController.Instance.player.GetLocalPlayerTeam())
            return true;
        return false;
    }
    #endregion
}
