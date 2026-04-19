using System.Collections.Generic;
using System.Diagnostics;

public abstract class HTN_PrimitiveTask : HTNTask
{
    protected abstract override Dictionary<string, object> Effects(HTNWorldState worldState);
    protected abstract override Dictionary<string, object> PreConditions();
    public abstract override TaskResult Execute(float delta, object actor, HTNWorldState worldState);

    public virtual void ApplyEffects(HTNWorldState worldState)
    {
        //UnityEngine.Debug.LogError("StartApplyEffect");
        var effects = Effects(worldState);
        foreach (var effect in effects)
        {
            //UnityEngine.Debug.Log("Effect: " + effect.Key + ". Value: " + effect.Value);
            worldState.SetValue(effect.Key, effect.Value);
        }
    }

    public virtual bool IsAvailable(HTNWorldState worldState)
    {
        var conditions = PreConditions();
        foreach (var kvp in conditions)
        {
            if (!Equals(worldState.GetValue(kvp.Key), kvp.Value))
            {
                return false;
            }
        }
        return true;
    }
}