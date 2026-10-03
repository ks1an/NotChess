using System;
using UnityEngine;

public class BanPut_TileBuff : IBuff
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


    int countStacks;
    bool canEffectStack, canDurationStack, isCriticalBuff;

    public BanPut_TileBuff(bool canEffectStack, bool canDurationStack, bool isCriticalBuff = false)
    {
        countStacks = 0;
        this.canEffectStack = canEffectStack;
        this.canDurationStack = canDurationStack;
        this.isCriticalBuff = isCriticalBuff;
    }

    public IBuffableStats ApplyBuff(IBuffableStats baseStats, IBuffable owner)
    {
        if (baseStats.GetType() == typeof(TileStats))
        {
            var newStats = (TileStats)baseStats;
            newStats.canPutOnTile = false;
            return newStats;
        }
        else
        {
            Debug.LogError($"GetNotEqualTypeBuff. My want {typeof(TileStats)}, but I get {baseStats.GetType()}");
            owner.RemoveBuff(this);
            return baseStats;
        }
    }

    public void DoOnAddBuff() {OnBuffAdded?.Invoke(); }
    public void DoOnRemoveBuff() { OnBuffRemoved?.Invoke(); }
    public void DoOnTick() { OnBuffTicked?.Invoke(); }
    public bool TryStack(IBuff stackingBuff) 
    {
        if (!stackingBuff.CanEffectStack || !CanEffectStack) return false;
        if (stackingBuff.GetType() != GetType())
        {
            Debug.LogError($"GetNotEqualTypeBuff on stacking. " +
                $"I want {GetType()}, but I get {stackingBuff.GetType()}");
            return false;
        }
        CountStacks++;
        return true;
    }
}
