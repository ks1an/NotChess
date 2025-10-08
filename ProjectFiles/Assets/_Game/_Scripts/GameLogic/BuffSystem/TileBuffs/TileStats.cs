[System.Serializable]
public struct TileStats : IBuffableStats
{
    public bool CanPutOnTile;
    public bool CanAttackTile;
    public bool CanLeaveFromTile;
    public DefendClass DefendClass;

    public TileStats(bool canPutOnTile, bool canAttackTile, bool canLeaveFromTile, DefendClass defendClass)
    {
        CanPutOnTile = canPutOnTile;
        CanAttackTile = canAttackTile;
        CanLeaveFromTile = canLeaveFromTile;
        DefendClass = defendClass;
    }
}