using UnityEngine;

public sealed class Tile : MonoBehaviour
{
    public Vector2Int coord;
    public Vector3 tileCenter;

    public TileStatsComponent Stats;
    public TileStats StatsView;


    public void ResetStats(bool removeCriticalBuffs = false) => Stats?.RemoveAllBuffs(removeCriticalBuffs);


    public bool TryGetAroundDefend(int attackClass)
    {
        if (attackClass < 0)
        {
            Debug.LogError("Trying attack with negative attack class");
            return false;
        }
        if (Stats.CurrentStats.DefendClass == 0)
            return true;

        if (attackClass == (int)Stats.CurrentStats.DefendClass)
        {
            Stats.DestroyBuffWithMostDefendClass();
            return false;
        }
        if (attackClass > (int)Stats.CurrentStats.DefendClass)
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
            CanAttackTile = true,
            CanLeaveFromTile = true,
            CanPutOnTile = true,
            DefendClass = 0
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
