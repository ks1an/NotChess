using System;
using UnityEngine;

public class DefendClass_TileBuff : IBuff
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


    public DefendClass defendClass;
    int countStacks;
    bool canEffectStack, canDurationStack, isCriticalBuff;

    public DefendClass_TileBuff(bool canEffectStack, bool canDurationStack, DefendClass defClass, bool isCriticalBuff = false)
    {
        countStacks = 0;
        this.canEffectStack = canEffectStack;
        this.canDurationStack = canDurationStack;
        defendClass = defClass;
        this.isCriticalBuff = isCriticalBuff;
    }

    public IBuffableStats ApplyBuff(IBuffableStats baseStats, IBuffable owner)
    {
        if (baseStats.GetType() == typeof(TileStats))
        {
            var newStats = (TileStats)baseStats;
            if ((int)defendClass >= (int)newStats.defendClass)
                newStats.defendClass = defendClass;
            return newStats;
        }
        else
        {
            Debug.LogError($"GetNotEqualTypeBuff. I want {typeof(TileStats)}, but I get {baseStats.GetType()}");
            owner.RemoveBuff(this);
            return baseStats;
        }
    }

    public void DoOnAddBuff() { OnBuffAdded?.Invoke(); }
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
        DefendClass_TileBuff stackBuff = (DefendClass_TileBuff)stackingBuff;
        if (stackBuff.defendClass == defendClass)
        {
            Debug.Log("StackedDefend");
            CountStacks++;
            return true;
        }
        return false;
    }
}
