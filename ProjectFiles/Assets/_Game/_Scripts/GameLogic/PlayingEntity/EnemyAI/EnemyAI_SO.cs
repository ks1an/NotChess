using UnityEngine;

[CreateAssetMenu(menuName = "AI/EnemyAI_SO")]
public class EnemyAI_SO : ScriptableObject
{
    public CardCollection CardsCollection;

    [Header("General")]
    [Range(0.1f, 5)] public float minDelayBeforeDoMove;
    [Range(0, 100)] public int chanceOfSkipTheMostValuableMove;

    [Header("Value params")]
    [SerializeField] Complexity complexity;

    [Space(5)]
    [Range(-10, 10)]
    public int valueIf_Enemy_VertOrHoriz,
        valueIf_Friend_VertOrHoriz;

    [Space(5)]
    [Range(-10, 10)]
    public int valueIf_Enemy_Diagonal,
        valueIf_Friend_Diagonal,
        valueIfBoardBorder,
        valueIfTileByEnemy,
        valueIfTileByFriend;

    [Header("Enemy_LinesValue")]
    [Range(-10, 100)]
    public int valueForEach_Enemy_InTheLine;
    [Range(-10, 100)]
    public int valueIf_Enemy_lineCompleted_80_procent,
        valueIf_Enemy_lineCompleted_60_procent,
        valueIf_Enemy_lineCompleted_40_procent,
        valueIf_Enemy_lineCompleted_20_procent;
    [Min(0.1f)]
    public float ShiftCells_EnemyLine_ValueCoeffic;

    [Header("Friend_LinesValue")]
    [Range(-10, 100)]
    public int valueForEach_Friend_InTheLine;
    [Range(-10, 100)]
    public int valueIf_Friend_lineCompleted_80_procent,
    valueIf_Friend_lineCompleted_60_procent,
    valueIf_Friend_lineCompleted_40_procent,
    valueIf_Friend_lineCompleted_20_procent;
    [Min(0.1f)]
    public float ShiftCells_FriendLine_ValueCoeffic;
}
