using System;

public class DefferedActionBuff : IBuff
{
    #region IBuffVariable
    public int CountStacks { get { return 0; } set => CountStacks = 0; }
    public bool CanEffectStack { get { return false; } set => CanEffectStack = false; }
    public bool CanDurationStack { get { return false; } set => CanDurationStack = false; }

    public event Action OnBuffAdded;
    public event Action OnBuffRemoved;
    public event Action OnBuffTicked;
    #endregion

    Action<IBuffable> doOnEnd;
    IBuffable owner;
    int waitTurns;
    readonly TurnTimer timer;
    TurnTimerSubscriber subscriberInTimer;

    public DefferedActionBuff(IBuffable owner, int waitTurns, Action<IBuffable> doOnEnd)
    {
        this.owner = owner;
        this.waitTurns = waitTurns;
        this.doOnEnd = doOnEnd;
        subscriberInTimer = null;
        timer = TurnTimer.GetInstance();
    }

    public IBuffableStats ApplyBuff(IBuffableStats baseStats, IBuffable owner)
    {
        this.owner = owner;
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
    }

    public bool TryStack(IBuff stackingBuff)
    {
        return false;
    }
}
