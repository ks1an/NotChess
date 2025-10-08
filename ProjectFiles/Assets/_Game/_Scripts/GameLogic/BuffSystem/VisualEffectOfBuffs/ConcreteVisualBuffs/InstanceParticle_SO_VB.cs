using UnityEngine;

[CreateAssetMenu(menuName = "BuffSystem/OnTile/VisualEffect/Particle_SO")]
public class InstanceParticle_SO_VB : VisualBuff_SO
{
    [Header("ParticleSystem")]
    public ParticleSystem particleSystem;
    public bool doRndYRotOnSpawn;
    public float hightOnSpawn;
    [Min(0)] public float spawnDuration;
    public bool pauseVfxAfterInstantiate;
}
