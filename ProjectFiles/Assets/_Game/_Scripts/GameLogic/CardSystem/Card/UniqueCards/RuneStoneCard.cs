using System.Collections.Generic;
using UnityEngine;

public sealed class RuneStoneCard : Card
{
    [Header("On used")]
    [field: SerializeField] InstanceGameobject_SO_VB visualEffectOnTile;
    [SerializeField] int waitTurns, runeDurationTurn;
    bool _isSynced;

    public override void Init()
    {
        base.Init();
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
        _isSynced = false;
        Tile tile = Board.Instance.tilesController.tiles[moves[0].x, moves[0].y];
        var tBuff = new DefferedActionBuff(tile.Stats,waitTurns, DoAfterWaitTurns);
        tile.Stats.AddBuff(tBuff);

        if (!isSynced)
        {
            GameController.Instance.states.move.UseCard(GetID(), moves);
            PlayerDeck.Instance.DestroyCardInHand(this);
        }
        else
        {
            _isSynced = true;
            Destroy(gameObject);
        }
    }

    void DoAfterWaitTurns(IBuffable target)
    {
        if (target is TileStatsComponent t)
        {
            bool attackWasSuccessful = t.Tile.TryGetAroundDefend((int)AttackClass);
            if (attackWasSuccessful)
            {
                if (!_isSynced)
                {
                    GameController.Instance.states.move.TryDestroyUnit(t.Tile.coord.x, t.Tile.coord.y,
                        false, GameController.Instance.player.GetLocalPlayerTeam());
                }

                List<IBuff> buffsOnTile = new()
                {
                    new BanPut_TileBuff(false, false),
                    new BanAttack_TileBuff(false, false)
                };
                var buffsOnTilePocket = new PocketBuff(false, false, buffsOnTile);
                new VisualGameobjectBuffBehaviour(buffsOnTilePocket, visualEffectOnTile, t.Tile.tileCenter);
                var tileBuff = new TemporaryBuff(target, buffsOnTilePocket, runeDurationTurn);
                target.AddBuff(tileBuff);
            }
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
