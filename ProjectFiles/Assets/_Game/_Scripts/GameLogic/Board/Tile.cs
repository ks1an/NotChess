using UnityEngine;

public sealed class Tile : MonoBehaviour
{
    public Vector2Int coord;
    public Vector3 tileCenter;

    public TileStatsAndBuffs tileBuffAndStatsComponent;
    public TileStats StatsView;

    void SetDef()
    {
        tileBuffAndStatsComponent?.RemoveAllBuffs();
        TileStats stats = new()
        {
            CanAttackTile = true,
            CanLeaveFromTile = true,
            CanPutOnTile = true,
            DefendClass = 0
        };
        tileBuffAndStatsComponent = new TileStatsAndBuffs(stats, UpdateStatsView);
    }

    void UpdateStatsView(TileStats newStats) => StatsView = newStats;

    public bool TryGetAroundDefend(int attackClass)
    {
        if (attackClass < 0)
        {
            Debug.LogError("Trying attack with negative attack class");
            return false;
        }
        if (tileBuffAndStatsComponent.CurrentStats.DefendClass == 0)
            return true;

        if (attackClass == (int)tileBuffAndStatsComponent.CurrentStats.DefendClass)
        {
            tileBuffAndStatsComponent.DestroyBuffWithMostDefendClass();
            return false;
        }
        if (attackClass > (int)tileBuffAndStatsComponent.CurrentStats.DefendClass)
        {
            tileBuffAndStatsComponent.DestroyBuffWithMostDefendClass();
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
