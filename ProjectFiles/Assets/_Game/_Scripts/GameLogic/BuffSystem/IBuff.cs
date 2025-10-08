using System;

public interface IBuff
{
    public event Action OnBuffAdded;
    public event Action OnBuffRemoved;
    public event Action OnBuffTicked;
    public int CountStacks { get; set; }
    public bool CanEffectStack { get; set; }
    public bool CanDurationStack { get; set; }

    IBuffableStats ApplyBuff(IBuffableStats baseStats, IBuffable owner);
    void DoOnAddBuff();
    void DoOnRemoveBuff();
    void DoOnTick();
    public bool TryStack(IBuff stackingBuff);
}
