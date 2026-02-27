using System;
using System.Collections.Generic;

public class PocketBuff : IBuff
{
    public event Action OnBuffAdded;
    public event Action OnBuffRemoved;
    public event Action OnBuffTicked;
    public int CountStacks
    {
        get
        {
            return countStacks;
        }
        set => countStacks = value;
    }
    public bool CanEffectStack
    {
        get
        {
            return canEffectStack;
        }
        set => canEffectStack = value;
    }
    public bool CanDurationStack
    {
        get
        {
            return canDurationStack;
        }
        set => canDurationStack = value;
    }
    public bool IsCriticalBuff { get { return isCriticalBuff; } set { IsCriticalBuff = isCriticalBuff; } }

    public List<IBuff> buffs;
    int countStacks;
    bool canEffectStack, canDurationStack, isCriticalBuff;

    public PocketBuff(bool canEffectStack, bool canDurationStack, List<IBuff> buffs, bool isCriticalBuff = false)
    {
        countStacks = 0;
        this.canEffectStack = canEffectStack;
        this.canDurationStack = canDurationStack;
        this.buffs = buffs;
        this.isCriticalBuff = isCriticalBuff;
    }

    public IBuffableStats ApplyBuff(IBuffableStats baseStats, IBuffable owner)
    {
        var newStats = baseStats;
        foreach (var buff in buffs)
            newStats = buff.ApplyBuff(newStats, owner);
        return newStats;
    }

    public void DoOnAddBuff()
    {
        foreach (var buff in buffs)
            buff.DoOnAddBuff();
        OnBuffAdded?.Invoke();
    }

    public void DoOnRemoveBuff()
    {
        foreach (var buff in buffs)
            buff.DoOnRemoveBuff();
        OnBuffRemoved?.Invoke();
    }

    public void DoOnTick()
    {
        foreach (var buff in buffs)
            buff.DoOnTick();
        OnBuffTicked?.Invoke();
    }

    public bool TryStack(IBuff stackingBuff)
    {
        if (!CheckCompatibility((PocketBuff)stackingBuff))
            return false;

        bool wasStacked = false;

        if ((stackingBuff.CanDurationStack && canDurationStack)
            || (stackingBuff.CanEffectStack && canEffectStack))
            foreach (var buff in buffs)
            {
                bool b = buff.TryStack(buff);
                if (b)
                    wasStacked = true;
            }

        if (wasStacked)
            return true;
        else
            return false;
    }

    bool CheckCompatibility(PocketBuff pocket)
    {
        if (pocket.buffs.Count != buffs.Count) return false;
        bool b = true;
        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i] != pocket.buffs[i]) b = false;
        }
        return b;
    }
}
