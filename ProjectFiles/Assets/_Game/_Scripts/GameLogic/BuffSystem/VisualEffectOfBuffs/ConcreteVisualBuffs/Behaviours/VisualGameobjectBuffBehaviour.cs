using DG.Tweening;
using UnityEngine;

public class VisualGameobjectBuffBehaviour : VisualBuffBehaviour
{
    readonly InstanceGameobject_SO_VB Effect;
    GameobjectVisualEffect obj;
    Vector3 posForVFX;

    public VisualGameobjectBuffBehaviour(IBuff buff,
        InstanceGameobject_SO_VB effect, Vector3 posForVFX)
        : base(buff)
    {
        Effect = effect;
        this.posForVFX = posForVFX;
    }

    protected override void DoOnAdded()
    {
        base.DoOnAdded();
        if (Effect.prefab == null) return;

        posForVFX.y += Effect.hightOnSpawn;
        obj = GameObject.Instantiate(Effect.prefab.gameObject,
            posForVFX, Quaternion.identity).GetComponent<GameobjectVisualEffect>();

        if (Effect.doRndYRotOnSpawn)
            obj.gameObject.transform.DORotate(obj.gameObject.transform.rotation.eulerAngles +
                new Vector3(0, Random.Range(0, 360)), Effect.spawnDuration);
        if (Effect.hightOnSpawn != 0)
            obj.gameObject.transform.DOMoveY(posForVFX.y - Effect.hightOnSpawn, Effect.spawnDuration);

        obj.DoOnSpawn();
        if (Effect.audioClipsOnAdded.Length > 0)
            GameSound.Instance.PlaySound(Effect.audioClipsOnAdded, Effect.volume,
                Effect.minPitch, Effect.maxPitch);
    }

    protected override void DoOnRemoved()
    {
        base.DoOnRemoved();

        if (Effect.audioClipsOnRemoved.Length > 0)
            GameSound.Instance.PlaySound(Effect.audioClipsOnAdded, Effect.volume,
                Effect.minPitch, Effect.maxPitch);
        obj.DoOnRemoved();
    }

    protected override void DoOnTurned()
    {
        base.DoOnTurned();

        if (Effect.audioClipsOnTurned.Length > 0)
            GameSound.Instance.PlaySound(Effect.audioClipsOnAdded, Effect.volume,
                Effect.minPitch, Effect.maxPitch);
        obj.DoOnTurned();
    }
}
