using UnityEngine;

[CreateAssetMenu(fileName = "AIProfile_New", menuName = "EnemyAI/AI Character Profile")]
public class AICharacterProfile : ScriptableObject
{
    [Header("Info")]
    public string profileName = "Name";

    [Header("Behavioral shift (%)")]
    public float attackBonus = 0f;
    public float lineBlockBonus = 0f;
    public float friendUnderThreatBonus = 0f;
    public float enemyUnderThreatBonus = 0f;

    [Header("Line Building & Landscape (%)")]
    public float lineBuildingBonus = 0f;
    public float placePawnUnderWallsBonus = 0f; 

    [Header("Cards (%)")]
    public float cardsPriority = 0f;
    public float destroyEnemyByCard = 0f;
    public float manaCardSpendingMultiple = 0f; 

    [Header("MinMax Settings")]
    public int searchDepthOffset = 0;
    public float blunderChanceDelta = 0f;
}