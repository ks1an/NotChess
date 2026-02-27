using UnityEngine;

public class CastleWallsLandscape : LandscapeObject
{
    [field: SerializeField] InstanceGameobject_SO_VB visualEffectOnTile;
    [SerializeField] float spawnDurationMultiple;

    public override void Init(int tileX, int tileY)
    {
        base.Init(tileX, tileY);

        Tile tile = Board.Instance.tilesController.tiles[tileX, tileY];
        var buff = new BanAttack_TileBuff(false, true);
        new VisualGameobjectBuffBehaviour(buff, visualEffectOnTile, tile.tileCenter, true, spawnDurationMultiple);
        var tBuff = new TemporaryBuff(tile.Stats, buff, duration);
        tile.Stats.AddBuff(tBuff);
    }
}
