using UnityEngine;

[CreateAssetMenu(menuName = "Effect/OnTile/BanUnitsMoving")]
public class TileBanUnitsMoving_ScriptableEffect : ScriptableEffector
{
    [Header("UniqueParams")]
    [field: SerializeField] bool banPutUnitsOnTile;
    [field: SerializeField] bool banAttackTileByUnits;
    [field: SerializeField] bool banLeaveTile;

    public override EffectorBehaviour InitializeEffect(GameObject obj, int x = -1, int y = -1)
    {
        return new EffectBanUnitMovingOnTile(this, obj, banPutUnitsOnTile, banAttackTileByUnits, banLeaveTile, x, y);
    }
}
