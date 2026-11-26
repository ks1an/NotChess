using System;
using System.Collections.Generic;
using static EnemyAIPlanner;

public class FastBoardState
{
    public int Width;
    public int Height;
    public CellOwner MyTeam;
    public int WinSequence;

    public CellOwner[,] Board;
    public FastTileStats[,] Stats;

    public int Mana;
    public int Bones;
    public int MaxMana;
    public int MaxBones;

    public List<Card> Hand;

    public FastBoardState Clone()
    {
        var clone = new FastBoardState
        {
            Width = Width,
            Height = Height,
            MyTeam = MyTeam,
            WinSequence = WinSequence,
            Mana = Mana,
            Bones = Bones,
            MaxBones = MaxBones,
            MaxMana = MaxMana,

            Hand = this.Hand != null ? new List<Card>(this.Hand) : new List<Card>(),
            Board = (CellOwner[,])Board.Clone(),
            Stats = (FastTileStats[,])Stats.Clone()
        };
        return clone;
    }
}

public struct FastTileStats
{
    public bool CanPutOnTile;
    public int BanPutDuration;

    public bool CanAttackTile;
    public int BanAttackTileDuration;

    public bool CanLeaveFromTile;
    public int BanLeaveFromTileDuration;

    public int DefendClass;

    public FastTileStats Clone()
    {
        return new FastTileStats
        {
            CanPutOnTile = this.CanPutOnTile,
            BanPutDuration = this.BanPutDuration,

            CanAttackTile = this.CanAttackTile,
            BanAttackTileDuration = this.BanAttackTileDuration,

            CanLeaveFromTile = this.CanLeaveFromTile,
            BanLeaveFromTileDuration = this.BanLeaveFromTileDuration,

            DefendClass = this.DefendClass
        };
    }
}
