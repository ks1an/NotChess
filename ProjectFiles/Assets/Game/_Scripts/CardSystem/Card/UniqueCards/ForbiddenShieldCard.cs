using System.Collections.Generic;
using UnityEngine;

public sealed class ForbiddenShieldCard : Card
{
    [field: SerializeField] ScriptableEffector effectOfTile;

    public override void Init()
    {
        base.Init();
    }

    #region OnDrag
    void OnMouseDown()
    {
        if (MatchController.Instance.player.GetCurrentMana() >= ManaCost)
        {
            if (IsUseOnlyMyTurn)
            {
                if (MatchController.Instance.player.IsMyTurnOrNot())
                {
                    Deck.Instance.playerHand.SetCurrentSelectCard(this);
                    CardUI.SetBorderColor(BoarderColorOnDrag);
                }
            }
            else
            {
                Deck.Instance.playerHand.SetCurrentSelectCard(this);
                CardUI.SetBorderColor(BoarderColorOnDrag);
            }

        }
    }

    void OnMouseUp()
    {
        if (MatchController.Instance.player.GetCurrentMana() >= ManaCost && availableMoves.Count > 0)
        {
            MatchController.Instance.player.DeacreaseMana(ManaCost);
            UseCard(availableMoves);
            Deck.Instance.playerHand.ResetCurrentSelectCard(null);
        }
        else if (Deck.Instance.playerHand.CurrentSelectCard == this)
        {
            Deck.Instance.playerHand.ResetCurrentSelectCard(this);
        }

        CardUI.SetBorderColor(ColorBorder);
    }
    #endregion

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {
        Board.Instance.tilesController.tiles[moves[0].x, moves[0].y].
            AddEffect(effectOfTile.InitializeEffect(null, moves[0].x, moves[0].y));

        if (!isSynced)
        {
            MatchController.Instance.states.UseCard(ID, moves);
            Deck.Instance.DestroyCard(this);
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
        if (Board.Instance.tilesController.tiles[targetX, targetY] != null)
            return true;
        return false;
    }
    #endregion
}
