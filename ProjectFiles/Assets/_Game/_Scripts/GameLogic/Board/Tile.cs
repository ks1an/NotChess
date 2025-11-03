using UnityEngine;

public sealed class Tile : MonoBehaviour
{
    public Vector2Int coord;
    public Vector3 tileCenter;

    public TileStatsComponent Stats;
    public TileStats StatsView;

    void SetDef()
    {
        Stats?.RemoveAllBuffs();
        TileStats stats = new()
        {
            CanAttackTile = true,
            CanLeaveFromTile = true,
            CanPutOnTile = true,
            DefendClass = 0
        };
        Stats = new TileStatsComponent(stats, UpdateStatsView, this);
    }

    void UpdateStatsView(TileStats newStats) => StatsView = newStats;

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

    void OnEnable()
    {
        GameController.Instance.states.OnGameStarted += SetDef;
    }

    void OnDisable()
    {
        GameController.Instance.states.OnGameStarted -= SetDef;
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
