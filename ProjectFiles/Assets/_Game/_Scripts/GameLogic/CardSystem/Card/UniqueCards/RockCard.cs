using System.Collections.Generic;
using UnityEngine;

public sealed class RockCard : Card
{
    [field: SerializeField] InstanceGameobject_SO_VB visualEffectOnTile;
    [SerializeField] int duration;

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
        Tile tile = Board.Instance.tilesController.tiles[moves[0].x, moves[0].y];
        var buff = new BanPut_TileBuff(false, true);
        new VisualGameobjectBuffBehaviour(buff, visualEffectOnTile, tile.tileCenter);
        var tBuff = new TemporaryBuff(tile.tileBuffAndStatsComponent, buff, duration);
        tile.tileBuffAndStatsComponent.AddBuff(tBuff);

        if (!isSynced)
        {
            GameController.Instance.states.UseCard(ID, moves);
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
        if (Board.Instance.tilesController.tiles[targetX, targetY] != null && Board.Instance.piecesController.pieces[targetX, targetY] == null)
            return true;
        return false;
    }
    #endregion
}
