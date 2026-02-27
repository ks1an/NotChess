using System;

public class DefferedActionBuff : IBuff
{
    //does not affect stats, unlike temporaryBuff
    #region IBuffVariable
    public int CountStacks { get { return 0; } set => CountStacks = 0; }
    public bool CanEffectStack { get { return false; } set => CanEffectStack = false; }
    public bool CanDurationStack { get { return false; } set => CanDurationStack = false; }

    public bool IsCriticalBuff { get { return isCriticalBuff; } set { IsCriticalBuff = isCriticalBuff; } }

    public event Action OnBuffAdded;
    public event Action OnBuffRemoved;
    public event Action OnBuffTicked;
    #endregion

    readonly Action<IBuffable> doOnTick;
    readonly Action<IBuffable> doOnEnd;
    IBuffable owner;
    readonly int waitTurns;
    readonly TurnTimer timer;
    TurnTimerSubscriber subscriberInTimer;
    readonly bool isCriticalBuff;

    public DefferedActionBuff(IBuffable owner, int waitTurns, Action<IBuffable> doOnEnd, Action<IBuffable> doOnTick = null, bool isCriticalBuff = false)
    {
        this.owner = owner;
        this.waitTurns = waitTurns;
        this.doOnTick = doOnTick;
        this.doOnEnd = doOnEnd;
        subscriberInTimer = null;
        timer = TurnTimer.GetInstance();
        this.isCriticalBuff = isCriticalBuff;
    }

    public IBuffableStats ApplyBuff(IBuffableStats baseStats, IBuffable owner)
    {
        this.owner = owner;
        if(waitTurns == 0)
        {
            OnLifeTurnsEnd();
            return baseStats;
        }

        if (subscriberInTimer == null)
            timer.StartTimer(waitTurns, OnLifeTurnsEnd, DoOnTick, out subscriberInTimer);
        return baseStats;
    }

    void OnLifeTurnsEnd()
    {
        doOnEnd?.Invoke(owner);
        owner.RemoveBuff(this);
    }

 
    public void DoOnAddBuff()
    {
        OnBuffAdded?.Invoke();
    }

    public void DoOnRemoveBuff()
    {
        OnBuffRemoved?.Invoke();
    }

    public void DoOnTick()
    {
        OnBuffTicked?.Invoke();
        doOnTick?.Invoke(owner);
    }

    public bool TryStack(IBuff stackingBuff)
    {
        return false;
    }
}
