using UnityEngine;

public sealed class EffectBanUnitMovingOnTile : EffectorBehaviour
{
    Tile effectableTile;
    bool isBanPutUnitsOnTile, isBanAttackTileByUnits, isBanLeaveTile;

    public EffectBanUnitMovingOnTile(ScriptableEffector buff, GameObject obj,
        bool banPutUnitsOnTile, bool banAttackTileByUnits, bool banLeaveTile, int x = -1, int y = -1) : base(buff, obj)
    {
        if (x > -1 && y > -1)
            effectableTile = Board.Instance.tilesController.tiles[x, y];
        else
            effectableTile = obj.GetComponent<Tile>();

        isBanPutUnitsOnTile = banPutUnitsOnTile;
        isBanAttackTileByUnits = banAttackTileByUnits;
        isBanLeaveTile = banLeaveTile;
    }

    protected override void DoOnStartEffect()
    {
        effectableTile.SetBans(isBanPutUnitsOnTile, isBanAttackTileByUnits, isBanLeaveTile);
    }

    protected override void DoOnTurnEnded() { }

    public override void EndEffect()
    {
        effectableTile.SetBans(false, false, false);
        base.EndEffect();
    }
}
