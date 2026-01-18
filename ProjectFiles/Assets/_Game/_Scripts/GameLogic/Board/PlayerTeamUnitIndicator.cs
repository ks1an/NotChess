using UnityEngine;

public class PlayerTeamUnitIndicator : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] bool isPlayerZone;

    GameObject unit;

    private void OnEnable()
    {
        GameController.Instance.states.OnGameStarted += SpawnUnitTeamIndicator;
        GameController.Instance.states.OnLeaveMatch += DestroyUnitTeamIndicator;
    }
    private void OnDisable()
    {
        GameController.Instance.states.OnGameStarted -= SpawnUnitTeamIndicator;
        GameController.Instance.states.OnLeaveMatch -= DestroyUnitTeamIndicator;
    }

    private void DestroyUnitTeamIndicator()
    {
        if (unit != null)
            Destroy(unit);
    }

    private void SpawnUnitTeamIndicator()
    {
        GameObject prefab = null;
        if (GameController.Instance.player.GetLocalPlayerTeam() == Team.Zero)
        {
            prefab = isPlayerZone ? GameController.Instance.player.zeroPrefab : GameController.Instance.player.crossPrefab;
        }
        else
            prefab = isPlayerZone ? GameController.Instance.player.crossPrefab : GameController.Instance.player.zeroPrefab;


        unit = GameObject.Instantiate(prefab, spawnPoint);
    }
}
