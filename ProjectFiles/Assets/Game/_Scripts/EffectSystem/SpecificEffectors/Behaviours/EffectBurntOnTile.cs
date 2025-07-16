using UnityEngine;

public sealed class EffectBurntOnTile : EffectorBehaviour
{
    public EffectBurntOnTile(ScriptableEffector buff, GameObject obj, int x, int y) : base(buff, obj)
    {
        
    }

    protected override void DoOnStartEffect() { }
    protected override void DoOnTurnEnded() { }
    public override void EndEffect()
    {
        base.EndEffect();
    }
}
