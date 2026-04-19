using DG.Tweening;
using UnityEngine;

public class VisualParticleBuffBehaviour : VisualBuffBehaviour
{
    readonly InstanceParticle_SO_VB Effect;
    Vector3 posForVFX;
    ParticleSystem vfxParticle;

    public VisualParticleBuffBehaviour(IBuff buff,
        InstanceParticle_SO_VB effect, Vector3 posForVFX)
        : base(buff)
    {
        Effect = effect;
        this.posForVFX = posForVFX;
    }

    protected override void DoOnAdded()
    {
        base.DoOnAdded();

        if (Effect.particleSystem != null)
        {
            posForVFX.y += Effect.hightOnSpawn;
            GameObject obj = GameObject.Instantiate(Effect.particleSystem.gameObject,
                posForVFX, Quaternion.identity);

            if (Effect.doRndYRotOnSpawn)
                obj.transform.DORotate(obj.transform.rotation.eulerAngles +
                    new Vector3(0, Random.Range(0, 360)), Effect.spawnDuration);
            if (Effect.hightOnSpawn != 0)
                obj.transform.DOMoveY(posForVFX.y - Effect.hightOnSpawn, Effect.spawnDuration);

            obj.TryGetComponent(out vfxParticle);
            if (vfxParticle == null)
            {
                Debug.LogError("VFX ParticleIsNull");
                vfxParticle = obj.AddComponent<ParticleSystem>();
            }

            if (Effect.pauseVfxAfterInstantiate)
            {
                vfxParticle.Simulate(0.1f, true, true);
                vfxParticle.Pause();
            }

            if (Effect.audioClipsOnAdded.Length > 0)
                GameSound.Instance.PlayRandomSound(Effect.audioClipsOnAdded, Effect.volume,
                    Effect.minPitch, Effect.maxPitch);
        }
    }

    protected override void DoOnRemoved()
    {
        base.DoOnRemoved();
        if (vfxParticle.isPaused)
            vfxParticle.Play();
        vfxParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (Effect.audioClipsOnRemoved.Length > 0)
            GameSound.Instance.PlayRandomSound(Effect.audioClipsOnAdded, Effect.volume,
                Effect.minPitch, Effect.maxPitch);
    }

    protected override void DoOnTurned()
    {
        base.DoOnTurned(); 

        if (Effect.audioClipsOnTurned.Length > 0)
            GameSound.Instance.PlayRandomSound(Effect.audioClipsOnAdded, Effect.volume,
                Effect.minPitch, Effect.maxPitch);
    }
}
