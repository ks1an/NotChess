using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class HandObject : MonoBehaviour
{
    [SerializeField] protected SplineContainer splineContainer;
    [SerializeField] protected float objSpacing = 0.1f;
    [SerializeField] protected float objUpdatePosTime = 0.15f;
    protected bool isDealing;
    protected List<GameObject> handObjects = new();

    //ADD
    protected IEnumerator AddObj(GameObject obj)
    {
        handObjects.Add(obj);
        yield return StartCoroutine(UpdateObjPos(objUpdatePosTime));
    }

    //REMOVE
    protected IEnumerator RemoveObjectInHand(GameObject obj)
    {
        handObjects.Remove(obj);
        Destroy(obj);
        yield return StartCoroutine(UpdateObjPos(objUpdatePosTime));
    }
    protected IEnumerator RemoveObjects(int amount)
    {
        if(amount >= handObjects.Count)
        {
            RemoveAllObjects();
            yield break;
        }

        for (int i = 0; i < amount && i < handObjects.Count; i++)
        {
            handObjects.Remove(handObjects[i]);
            Destroy(handObjects[i]);
            yield return StartCoroutine(UpdateObjPos(objUpdatePosTime));
        }
    }
    protected void RemoveAllObjects()
    {
        for (int i = 0; i < handObjects.Count && i < handObjects.Count; i++)
            Destroy(handObjects[i]);
        handObjects.Clear();
    }

    //DoSomeWithAllList
    IEnumerator UpdateObjPos(float duration)
    {
        if (handObjects.Count == 0) 
            yield break;
        isDealing = true;
        float firtsCardPos = 0.5f - (handObjects.Count - 1) * objSpacing / 2;
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < handObjects.Count; i++)
        {
            Transform objTransform = handObjects[i].transform;
            if (objTransform == null)
                continue;

            float pos = firtsCardPos + i * objSpacing;
            Vector3 splinePos = spline.EvaluatePosition(pos);
            Vector3 forward = spline.EvaluateTangent(pos);
            Vector3 up = spline.EvaluateUpVector(pos);
            Quaternion rot = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            objTransform.DOMove(splinePos + transform.position, duration);
            objTransform.DOLocalRotate(rot.eulerAngles, duration);
        }

        yield return new WaitForSeconds(duration);
        isDealing = false;
        yield return null;
    }
}
