public abstract class VisualBuffBehaviour
{
    protected readonly IBuff buff;

    protected VisualBuffBehaviour(IBuff buff)
    {
        this.buff = buff;
        buff.OnBuffAdded += DoOnAdded;
        buff.OnBuffRemoved += DoOnRemoved;
        buff.OnBuffTicked += DoOnTurned;
    }

    protected virtual void DoOnAdded() { }

    protected virtual void DoOnTurned() { }

    protected virtual void DoOnRemoved()
    {
        buff.OnBuffAdded -= DoOnAdded;
        buff.OnBuffAdded -= DoOnRemoved;
        buff.OnBuffTicked -= DoOnTurned;
    }
}
