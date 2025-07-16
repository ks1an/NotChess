using System.Collections.Generic;

public interface IEffectable
{
    public Dictionary<ScriptableEffector, EffectorBehaviour> Effects { get; set; }

    public abstract void AddEffect(EffectorBehaviour buff);

    public abstract void OnTurnEnded(int x, int y, Team teamTurned);

    public abstract void EndAllEffects();
}
