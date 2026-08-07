using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    PlacePawn,
    AttackPawn,
    PlayCard
}

[System.Serializable]
public struct EnemyAIAction
{
    public ActionType Type;
    public Vector2Int TargetCell;
    public Vector2Int SourceCell;
    public Card CardToPlay;
    public List<Vector2Int> CardTargets;
}