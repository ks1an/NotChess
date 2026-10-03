using System;
using System.Collections.Generic;
using System.Linq;

public class TileStatsComponent : IBuffable
{
    public Tile Tile { get; }
    public TileStats BaseStats { get; }
    public TileStats CurrentStats { get; private set; }

    readonly List<IBuff> buffs = new();
    readonly Action<TileStats> doOnUpdateStats;
    IBuff buffWithMostDefend;

    public TileStatsComponent(TileStats baseStats, Action<TileStats> doOnUpdateStats, Tile tile)
    {
        this.Tile = tile;
        BaseStats = baseStats;
        CurrentStats = baseStats;
        this.doOnUpdateStats = doOnUpdateStats;
        doOnUpdateStats?.Invoke(CurrentStats);
    }

    public bool IsUnderBuffs()
    {
        if (buffs.Count > 0) return true;
        return false;
    }

    #region Buff
    public void AddBuff(IBuff buff)
    {
        bool wasSimBuff = false;
        if (buff.CanEffectStack || buff.CanDurationStack)
        {
            IBuff simBuff = TryGetSimillarBuffForStack(buff);
            if (simBuff != null)
            {
                if (simBuff.TryStack(buff))
                {
                    wasSimBuff = true;
                    ApplyBuffs();
                }
            }
        }

        if (wasSimBuff == false)
        {
            buffs.Add(buff);
            ApplyBuffs();
            buff.DoOnAddBuff();
        }
    }

    public void RemoveBuff(IBuff buff)
    {
        buffs.Remove(buff);
        ApplyBuffs();
        buff.DoOnRemoveBuff();
    }

    void ApplyBuffs()
    {
        buffWithMostDefend = null;
        int mostBanPutDur = 0, mostBanAttackDur = 0, MostBanLeaveDur = 0;
        CurrentStats = BaseStats;
        foreach (var buff in buffs)
        {
            TileStats newStats = (TileStats)buff.ApplyBuff(CurrentStats, this);
            if (newStats.defendClass >= CurrentStats.defendClass)
                buffWithMostDefend = buff;

            if (buff.GetType() == typeof(TemporaryBuff))
            {
                int turns = ((TemporaryBuff)buff).lifeTurnsRemain;
                mostBanPutDur = !newStats.canPutOnTile && turns > mostBanPutDur ? turns : mostBanPutDur;
                mostBanAttackDur = !newStats.canAttackTile && turns > mostBanAttackDur ? turns : mostBanAttackDur;
                MostBanLeaveDur = !newStats.canLeaveFromTile && turns > MostBanLeaveDur ? turns : MostBanLeaveDur;
            }

            CurrentStats = newStats;
        }
        TileStats resultStats = new(
            CurrentStats.canPutOnTile, CurrentStats.canAttackTile, CurrentStats.canLeaveFromTile, CurrentStats.defendClass,
            mostBanPutDur, mostBanAttackDur, MostBanLeaveDur);
        CurrentStats = resultStats;

        /*Debugger.Instance.DebugLog($"Tile {Tile.coord} has: " +
            $"canPutOnTile = {CurrentStats.canPutOnTile} with dur {CurrentStats.banPutDuration} " +
            $"canAttackTile = {CurrentStats.canAttackTile} with dur {CurrentStats.banAttackDuration} " +
            $"canLeaveFromTile = {CurrentStats.canLeaveFromTile} with dur {CurrentStats.banLeaveDuration} ");*/

        doOnUpdateStats?.Invoke(CurrentStats);
    }

    public void RemoveAllBuffs(bool resetCriticalBuffs)
    {
        foreach (var buff in buffs.ToList())
        {
            if (!resetCriticalBuffs && buff.IsCriticalBuff) continue;

            buffs.Remove(buff);
            buff.DoOnRemoveBuff();
        }

        ApplyBuffs();
    }
    #endregion

    public void DestroyBuffWithMostDefendClass()
    {
        if (buffWithMostDefend != null)
        {
            RemoveBuff(buffWithMostDefend);
        }
    }

    IBuff TryGetSimillarBuffForStack(IBuff buff)
    {
        IBuff simBuff = null;
        if (buff.GetType() == typeof(TemporaryBuff))
        {
            TemporaryBuff targetB = (TemporaryBuff)buff;
            foreach (IBuff b in buffs)
            {
                if (((b.CanDurationStack && buff.CanDurationStack) || (b.CanEffectStack && buff.CanEffectStack))
                    && b.GetType() == targetB.GetType())
                {
                    TemporaryBuff t = (TemporaryBuff)b;
                    if (t.coreBuff.GetType() == targetB.coreBuff.GetType())
                    {
                        simBuff = t;
                        break;
                    }
                }
            }
        }
        else
            simBuff = buffs.FirstOrDefault(b => b.GetType() == buff.GetType() &&
            ((b.CanDurationStack && buff.CanDurationStack) || (b.CanEffectStack && buff.CanEffectStack)));

        return simBuff;
    }

    public TileStatsComponent Clone()
    {
        return (TileStatsComponent)this.MemberwiseClone();
    }
}
