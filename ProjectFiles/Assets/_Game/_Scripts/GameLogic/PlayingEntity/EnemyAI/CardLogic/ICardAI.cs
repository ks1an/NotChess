using System.Collections.Generic;
using UnityEngine;
using static EnemyAIPlanner;

public interface ICardAI
{
    int ManaCost { get; }
    int GraveTokensCost { get; }

    List<List<Vector2Int>> GetTargets(FastBoardState state, CellOwner myTeam);
    FastBoardState ApplyToState(FastBoardState state, List<Vector2Int> targets, CellOwner myTeam);
}
