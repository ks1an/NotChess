using System;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialActionType
{
    ShowMessage,    
    EnemyMove,
    EnemyPlayCard,
    PlayerPlayCard, 
    PlayerPlacePiece,
    PlayerSelectPiece,
    PlayerCapture, 
    Finish,
}

[Serializable]
public class TutorialStep
{
    public TutorialActionType type;

    [TextArea(2, 5)]
    public string message;

    public List<Vector2Int> targetTiles;
    public Card playCardType;
    public float delayBeforeWork;
}