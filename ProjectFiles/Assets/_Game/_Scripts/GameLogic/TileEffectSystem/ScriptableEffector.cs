using UnityEngine;

public abstract class ScriptableEffector : ScriptableObject
{
    [Header("General params")]
    public int durationTurns;    
    public bool isDurationStacked;
    public bool isEffectStacked;
    public DefendClass defendClass;

    [Header("VFX"), Tooltip("vfxPrefab = null if there is no vfx")]
    public GameObject visualEffect;
    public bool doRandomYForVfx;
    public float hightFromWhichItInit;
    public float durationForSpawn;
    public bool pauseVfxAfterInstantiate;

    [Header("Audio")]
    public float volume = 1f;
    public float minPitch = 1f, maxPitch = 1f;
    public AudioClip[] audioClipsOnUsed;

    public abstract EffectorBehaviour InitializeEffect(GameObject obj, int x = -1, int y = -1);
}
