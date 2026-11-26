using System.Collections.Generic;
using UnityEngine;

public class RuneStoneLandscape : LandscapeObject
{
    [field: SerializeField] InstanceGameobject_SO_VB visualEffectOnTile;
    [SerializeField] float spawnDurationMultiple;
    VisualGameobjectBuffBehaviour spawnedVisualEffectObj;
    public override void Init(int tileX, int tileY)
    {
        base.Init(tileX, tileY);

        Tile tile = Board.Instance.tilesController.tiles[tileX, tileY];

        List<IBuff> buffsOnTile = new()
                {
                    new BanPut_TileBuff(false, false),
                    new BanAttack_TileBuff(false, false)
                };
        var buffsOnTilePocket = new PocketBuff(false, false, buffsOnTile);
        spawnedVisualEffectObj = new VisualGameobjectBuffBehaviour(buffsOnTilePocket, visualEffectOnTile, tile.tileCenter);

        var tileBuff = new TemporaryBuff(tile.Stats, buffsOnTilePocket, duration);
        appliedBuff = tileBuff;

        tile.Stats.AddBuff(tileBuff);
    }
}
