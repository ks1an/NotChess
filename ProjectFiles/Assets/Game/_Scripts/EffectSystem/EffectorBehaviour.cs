using UnityEngine;

public abstract class EffectorBehaviour
{
    protected int aplyTickRateTurns = 1;
    protected int durationTurn;
    protected int effectStacks;

    public ScriptableEffector Effect { get; }
    ParticleSystem vfxParticle;
    protected readonly GameObject obj;
    public bool isFinished;

    public EffectorBehaviour(ScriptableEffector buff, GameObject obj)
    {
        Effect = buff;
        this.obj = obj;
    }

    public void OnTurnEnded()
    {
        durationTurn -= 1;
        DoOnTurnEnded();

        if (durationTurn <= 0)
        {
            EndEffect();
            isFinished = true;
            DestroyVFX();
        }
    }

    public void Activate(Vector3 posForVFX)
    {
        //VFX only 1
        if (vfxParticle == null && Effect.vfxPrefab != null)
        {
            vfxParticle = GameObject.Instantiate(Effect.vfxPrefab, posForVFX, Quaternion.identity).GetComponent<ParticleSystem>();
            if (Effect.pauseVfxAfterInstantiate)
            {
                vfxParticle.Simulate(0.1f, true, true);
                vfxParticle.Pause();
            }
        }

        //Stacks
        if (Effect.isEffectStacked || durationTurn <= 0)
        {
            DoOnStartEffect();
            effectStacks++;
        }

        if (Effect.isDurationStacked || durationTurn <= 0)
        {
            durationTurn += Effect.durationTurns;
        }
    }

    protected void DestroyVFX()
    {
        if (vfxParticle != null)
        {
            if (vfxParticle.isPaused)
                vfxParticle.Play();
            vfxParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    protected abstract void DoOnStartEffect();
    protected abstract void DoOnTurnEnded();
    public virtual void EndEffect()
    {
        DestroyVFX();
    }
}