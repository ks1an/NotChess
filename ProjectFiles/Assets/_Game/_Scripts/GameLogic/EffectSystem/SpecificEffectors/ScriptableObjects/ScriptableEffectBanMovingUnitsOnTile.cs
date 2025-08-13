using UnityEngine;

[CreateAssetMenu(menuName = "Effect/OnTile/Burnt")]
public class BurntTile_ScriptableEffect : ScriptableEffector
{
    public override EffectorBehaviour InitializeEffect(GameObject obj, int x = -1, int y = -1)
    {
        return new EffectBurntOnTile(this, obj, x, y);
    }
}
