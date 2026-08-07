using System;
using UnityEngine;


[Serializable]
public class LandscapeObject : MonoBehaviour
{
    public string landName;
    public int minAmount, maxAmount;
    public int duration;
    [Range(0, 100)] public int chance;

    void DestroyObject() => Destroy(gameObject);

    public virtual void Init(int tileX, int tileY)
    {
        
    }

    private void OnEnable()
    {
        GameController.Instance.states.OnGameStarted += DestroyObject;
    }

    private void OnDisable()
    {
        GameController.Instance.states.OnGameStarted -= DestroyObject;
    }
}
