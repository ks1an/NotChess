using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class HandObject : MonoBehaviour
{
    [SerializeField] protected SplineContainer splineContainer;
    [SerializeField] protected float objSpacing = 0.1f, sizeMultiple = 1f;
    [SerializeField] protected float objUpdatePosTime = 0.15f;
    protected bool isDealing;
    protected List<GameObject> handObjects = new();

    public SplineContainer GetSpline() { return splineContainer; }
    public IEnumerator SetSpline(SplineContainer splineContainer)
    {
        this.splineContainer = splineContainer;
        yield return StartCoroutine(UpdateObjPos(objUpdatePosTime));
    }

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
        if (amount >= handObjects.Count)
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
    protected IEnumerator UpdateObjPos(float duration, int indexSelectObj = -1)
    {
        if (handObjects.Count == 0) 
            yield break;
        isDealing = true;
        float firtsCardPos = 0.5f - (handObjects.Count - 1) * objSpacing/2;
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < handObjects.Count; i++)
            handObjects[i].transform.DOComplete();

        for (int i = 0; i < handObjects.Count; i++)
        {
            Transform objTransform = handObjects[i].transform;
            if (objTransform == null)
            {
                Debug.LogWarning("Not find transform in handObjects: " + handObjects[i].name);
                continue;
            }

            float additionalSpacing=0;
            if(indexSelectObj > -1)
            {
                if (i + 1 == indexSelectObj)
                    additionalSpacing = -0.005f;
                if (i - 1 == indexSelectObj)
                    additionalSpacing = 0.005f;
            }
            float pos = firtsCardPos + i * objSpacing + additionalSpacing;

            Vector3 splinePos = spline.EvaluatePosition(pos);
            Vector3 forward = spline.EvaluateTangent(pos);
            Vector3 up = spline.EvaluateUpVector(pos);
            Quaternion rot = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            if (i % 2 == 0)
                splinePos = new Vector3(splinePos.x, splinePos.y, splinePos.z + 0.05f);


            objTransform.DOMove(splinePos + transform.position, duration);
            objTransform.DOLocalRotate(rot.eulerAngles, duration);
        }

        yield return new WaitForSeconds(duration);
        isDealing = false;
        yield return null;
    }
}
