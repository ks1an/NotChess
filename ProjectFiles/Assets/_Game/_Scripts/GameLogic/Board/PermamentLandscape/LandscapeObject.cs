using System;
using UnityEngine;


[Serializable]
public class LandscapeObject : MonoBehaviour
{
    public string landName;
    public int minAmount, maxAmount;
    public int duration;
    [Range(0, 100)] public int chance;

    protected Tile targetTile;
    protected IBuff appliedBuff;

    public virtual void Init(int tileX, int tileY)
    {
        targetTile = Board.Instance.tilesController.tiles[tileX, tileY];
    }

    public virtual void ClearLandscape()
    {
        if (targetTile != null && appliedBuff != null)
        {
            targetTile.Stats.RemoveBuff(appliedBuff);
            appliedBuff = null;
        }

        if (gameObject != null)
            Destroy(gameObject);
    }
}
