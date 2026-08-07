using DG.Tweening;
using UnityEngine;

public class PieceView : MonoBehaviour
{
    public PieceData dataTemp;

    [HideInInspector] public PieceData runtimeData;

    public void Init()
    {
        if (dataTemp != null)
        {
            runtimeData = Instantiate(dataTemp);
            runtimeData.view = this;
        }
        else
        {
            Debug.LogError($"There is no pieceData template set on unit {gameObject.name}!");
        }
    }

    public void SetPos(Vector3 targetPos, bool force = false)
    {
        transform.DOKill();
        if (force)
            transform.position = targetPos;
        else
            transform.DOMove(targetPos, runtimeData.durationSetPos).SetEase(Ease.InOutBack);
    }

    private void OnDestroy()
    {
        Destroy(runtimeData);
    }
}
