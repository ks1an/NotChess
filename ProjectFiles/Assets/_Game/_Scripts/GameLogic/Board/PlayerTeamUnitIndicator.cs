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
        GameController.Instance.states.OnGameRestarted += DestroyUnitTeamIndicator;
    }
    private void OnDisable()
    {
        GameController.Instance.states.OnGameStarted -= SpawnUnitTeamIndicator;
        GameController.Instance.states.OnLeaveMatch -= DestroyUnitTeamIndicator;
        GameController.Instance.states.OnGameRestarted -= DestroyUnitTeamIndicator;
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
            prefab = isPlayerZone ? GameController.Instance.player.zeroPawnPrefab.gameObject : GameController.Instance.player.crossPawnPrefab.gameObject;
        }
        else
            prefab = isPlayerZone ? GameController.Instance.player.crossPawnPrefab.gameObject : GameController.Instance.player.zeroPawnPrefab.gameObject;


        unit = GameObject.Instantiate(prefab, spawnPoint);
    }
}
