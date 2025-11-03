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
        CurrentStats = BaseStats;
        foreach (var buff in buffs)
        {
            TileStats newStats = (TileStats)buff.ApplyBuff(CurrentStats, this);
            if (newStats.DefendClass >= CurrentStats.DefendClass)
                buffWithMostDefend = buff;

            CurrentStats = newStats;
        }
        doOnUpdateStats?.Invoke(CurrentStats);
    }

    public void RemoveAllBuffs()
    {
        foreach (var buff in buffs.ToList())
        {
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
}
