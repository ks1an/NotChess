using UnityEngine;

[CreateAssetMenu(menuName = "BuffSystem/OnTile/VisualEffect/Gameobject_SO")]
public class InstanceGameobject_SO_VB : VisualBuff_SO
{
    [Header("Gameobject")]
    public GameobjectVisualEffect prefab;
    public bool doRndYRotOnSpawn;
    public float hightOnSpawn;
    [Min(0)] public float spawnDuration;
}
