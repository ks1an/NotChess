using System.Collections.Generic;

public abstract class HTNTask
{
    public virtual HTNTask GetClone()
    {
        HTNTask clone = (HTNTask)this.MemberwiseClone();
        return clone;
    }

    protected virtual Dictionary<string, object> PreConditions()
    {
        return new Dictionary<string, object>();
    }

    protected virtual Dictionary<string, object> Effects(HTNWorldState worldState)
    {
        return new Dictionary<string, object>();
    }

    public abstract TaskResult Execute(float delta, object actor, HTNWorldState worldState);
} 

public enum TaskResult
{
    SUCCESS,
    FAILURE,
    RUNNING
}
