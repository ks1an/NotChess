using System;
public class TemporaryBuff : IBuff
{
    //Affects stats initially, unlike Deffered. Pay attention to coreBuff.
    public event Action OnBuffAdded;
    public event Action OnBuffRemoved;
    public event Action OnBuffTicked;
    public int CountStacks
    {
        get
        {
            return coreBuff.CountStacks;
        }
        set => CountStacks = coreBuff.CountStacks;
    }
    public bool CanEffectStack
    {
        get
        {
            return coreBuff.CanDurationStack;
        }
        set => CanEffectStack = coreBuff.CanEffectStack;
    }
    public bool CanDurationStack
    {
        get
        {
            return coreBuff.CanDurationStack;
        }
        set => CanDurationStack = coreBuff.CanDurationStack;
    }
    public bool IsCriticalBuff { get { return isCriticalBuff; } set { IsCriticalBuff = isCriticalBuff; } }

    public readonly IBuff coreBuff;
    public int lifeTurnsRemain;
    readonly int lifeTurns;
    readonly bool isCriticalBuff;
    readonly TurnTimer timer;
    IBuffable owner;
    TurnTimerSubscriber subscriberInTimer;

    public TemporaryBuff(IBuffable owner, IBuff coreBuff, int lifeTurns, bool isCriticalBuff = false)
    {
        subscriberInTimer = null;
        this.owner = owner;
        this.coreBuff = coreBuff;
        this.lifeTurns = lifeTurns;
        timer = TurnTimer.GetInstance();
        this.isCriticalBuff = isCriticalBuff;
    }

    public IBuffableStats ApplyBuff(IBuffableStats baseStats, IBuffable owner)
    {
        this.owner = owner;
        var newStats = coreBuff.ApplyBuff(baseStats, owner);
        if (subscriberInTimer == null)
        {
            lifeTurnsRemain = lifeTurns;
            timer.StartTimer(lifeTurns, OnLifeTurnsEnd, DoOnTick, out subscriberInTimer);
        }
        return newStats;
    }

    void OnLifeTurnsEnd()
    {
        owner.RemoveBuff(this);
    }


    public void DoOnAddBuff() { coreBuff.DoOnAddBuff(); OnBuffAdded?.Invoke(); }

    public void DoOnRemoveBuff() 
    {
        coreBuff.DoOnRemoveBuff();
        OnBuffRemoved?.Invoke(); 
    }
    public void DoOnTick()
    {
        lifeTurnsRemain--;
        coreBuff.DoOnTick();
        OnBuffTicked?.Invoke();
    }
    public bool TryStack(IBuff stackingBuff)
    {
        var t = (TemporaryBuff)stackingBuff;
        bool wasStacked = false;

        if(coreBuff.TryStack(t.coreBuff))
            wasStacked = true;
        if(stackingBuff.CanDurationStack && CanDurationStack)
        {
            lifeTurnsRemain += lifeTurns;
            subscriberInTimer?.AddTurnsLife(lifeTurns);
            wasStacked = true;
        }

        if (wasStacked)
            return true;
        else 
            return false;
    }
}
