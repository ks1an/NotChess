[System.Serializable]
public struct TileStats : IBuffableStats
{
    public bool canPutOnTile;
    public bool canAttackTile;
    public bool canLeaveFromTile;
    public DefendClass defendClass;

    public int banPutDuration,
        banAttackDuration,
        banLeaveDuration;

    public TileStats(bool canPutOnTile, bool canAttackTile, bool canLeaveFromTile, DefendClass defendClass, 
        int banPutOnTileDuration, int banAttackTileDur, int banLeaveFromTileDur)
    {
        this.canPutOnTile = canPutOnTile;
        this.canAttackTile = canAttackTile;
        this.canLeaveFromTile = canLeaveFromTile;
        this.defendClass = defendClass;

        this.banPutDuration = banPutOnTileDuration;
        this.banAttackDuration = banAttackTileDur;
        this.banLeaveDuration = banLeaveFromTileDur;
    }
}