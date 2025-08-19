using DG.Tweening;
using UnityEngine;

public abstract class EffectorBehaviour
{
    protected int aplyTickRateTurns = 1;
    protected int durationTurn;
    protected int effectStacks;

    public ScriptableEffector Effect { get; }
    ParticleSystem vfxParticle;
    GameObject visualEffectObject;
    protected readonly GameObject targetObj;
    public bool isFinished;

    public EffectorBehaviour(ScriptableEffector buff, GameObject obj)
    {
        Effect = buff;
        this.targetObj = obj;
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
        if (vfxParticle == null && Effect.visualEffect != null && visualEffectObject == null)
        {
            posForVFX.y += Effect.hightFromWhichItInit;
            var obj = GameObject.Instantiate(Effect.visualEffect, posForVFX, Quaternion.identity);
            if (Effect.doRandomYForVfx)
                obj.transform.DORotate(obj.transform.rotation.eulerAngles + new Vector3(0, Random.Range(0, 360)), Effect.durationForSpawn);
            if (Effect.hightFromWhichItInit != 0)
                obj.transform.DOMoveY(posForVFX.y-Effect.hightFromWhichItInit, Effect.durationForSpawn);

            obj.TryGetComponent<ParticleSystem>(out vfxParticle);
            if (vfxParticle == null)
                visualEffectObject = obj;

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
        if (visualEffectObject != null)
            GameObject.Destroy(visualEffectObject);
    }
    /*  var posForSpawnPrefab = Board.Instance.tilesController.GetTileCenter(moves[0].x, moves[0].y);
        Instantiate(spawnPrefabAfterUsed, new Vector3(posForSpawnPrefab.x, posForSpawnPrefab.y - posForSpawnPrefab.y * 2, posForSpawnPrefab.z), 
            Quaternion.identity).transform.DOMoveY(posForSpawnPrefab.y, durationSpawn);*/

    protected abstract void DoOnStartEffect();
    protected abstract void DoOnTurnEnded();
    public virtual void EndEffect()
    {
        DestroyVFX();
    }
}