using UnityEngine;

public sealed class Tile : MonoBehaviour
{
    public Vector2Int coord;
    public Vector3 tileCenter;

    public TileStatsComponent Stats;
    public TileStats StatsView;

    public MeshRenderer render;

    public void ResetStats(bool removeCriticalBuffs = false) => Stats?.RemoveAllBuffs(removeCriticalBuffs);

    public bool TryGetAroundDefend(int attackClass)
    {
        if (attackClass < 0)
        {
            Debug.LogError("Trying attack with negative attack class");
            return false;
        }
        if (Stats.CurrentStats.defendClass == 0)
            return true;

        if (attackClass == (int)Stats.CurrentStats.defendClass)
        {
            Stats.DestroyBuffWithMostDefendClass();
            return false;
        }
        if (attackClass > (int)Stats.CurrentStats.defendClass)
        {
            Stats.DestroyBuffWithMostDefendClass();
            return true;
        }
        return false;
    }

    void UpdateStatsView(TileStats newStats) => StatsView = newStats;

    void OnGameStarted() => ResetStats(true);

    void OnEnable()
    {
        TileStats stats = new()
        {
            canAttackTile = true,
            canLeaveFromTile = true,
            canPutOnTile = true,
            defendClass = 0
        };
        Stats = new TileStatsComponent(stats, UpdateStatsView, this);

        GameController.Instance.states.OnGameStarted += OnGameStarted;
    }

    void OnDisable()
    {
        GameController.Instance.states.OnGameStarted -= OnGameStarted;
    }
}

public enum DefendClass
{
    None,
    Light,
    Average,
    Advanced,
    Strong
}
