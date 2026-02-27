using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class InteractLandscapeObject : MonoBehaviour
{
    public string landName;
    public int turnsWaiting, turnsLiving;
    public List<LandscapeVectorsPattern> patterns;

    public virtual void Init(int tileX, int tileY, List<Vector2Int> workzone, int pattern)
    {

    }

    public abstract bool CheckConditionOfExistence();

    public abstract List<Vector2Int> CheckConditionOfPlacing(int tileX, int tileY, List<Vector2Int> freeTiles);

    [Serializable]
    public class LandscapeVectorsPattern
    {
        public List<Vector2Int> positionsFromCenter;
    }
}