using UnityEngine;

public class RockLandscape : LandscapeObject
{
    [field: SerializeField] InstanceGameobject_SO_VB visualEffectOnTile;
    [SerializeField] float spawnDurationMultiple;

    VisualGameobjectBuffBehaviour spawnedVisualEffectObj;

    public override void Init(int tileX, int tileY)
    {
        base.Init(tileX, tileY);

        Tile tile = Board.Instance.tilesController.tiles[tileX, tileY];
        var buff = new BanPut_TileBuff(false, true);

        spawnedVisualEffectObj =  new VisualGameobjectBuffBehaviour(buff, visualEffectOnTile, tile.tileCenter, true, spawnDurationMultiple);

        var tBuff = new TemporaryBuff(tile.Stats, buff, duration);
        appliedBuff = tBuff;

        tile.Stats.AddBuff(tBuff);
    }
}
