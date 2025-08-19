using UnityEngine;

public abstract class ScriptableEffector : ScriptableObject
{
    [Header("General params")]
    public int durationTurns;    
    public bool isDurationStacked;
    public bool isEffectStacked;

    [Header("VFX"), Tooltip("vfxPrefab = null if there is no vfx")]
    [field: SerializeField] public GameObject visualEffect;
    [field: SerializeField] public bool doRandomYForVfx;
    [field: SerializeField] public float hightFromWhichItInit;
    [field: SerializeField] public float durationForSpawn;
    [field: SerializeField] public bool pauseVfxAfterInstantiate;

    public abstract EffectorBehaviour InitializeEffect(GameObject obj, int x = -1, int y = -1);
}
