using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GlobalVolumePostEffect))]
public sealed class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance;

    [SerializeField] List<FlickeringLight> boardFlickingLights;
    GlobalVolumePostEffect globalVolume;
    [Header("Light on turn")]
    [SerializeField] Light turnLight;
    [SerializeField, Min(0)] float enemyIntensityOnTurn, playerIntensityOnTurn;

    void Awake()
    {
        if (Instance == null)
        {
            globalVolume = GetComponent<GlobalVolumePostEffect>();
            Instance = this;
            if (turnLight != null)
            {
                GameController.Instance.states.OnTurnEnded += (x, y, team) =>
                { OnMovingTeam(GameController.Instance.player.IsMyTurnOrNot()); };
                GameController.Instance.states.OnGameStarted += () =>
                { OnMovingTeam(GameController.Instance.player.IsMyTurnOrNot()); };
            }
        }
        else
            Destroy(this);
    }

    //Light
    public void DoBoardFlickeringLight(int count)
    {
        if (boardFlickingLights.Count <= 0)
        {
            Debug.LogError("BoardFlickingLights not founded!");
            return;
        }
        for (int i = 0; i < boardFlickingLights.Count; i++)
            boardFlickingLights[i].StartFlickering(count);
    }
    public void OnMovingTeam(bool highlightPlayer)
    {
        if (turnLight == null) return;

        if (highlightPlayer && turnLight != null)
        {
            turnLight.intensity = playerIntensityOnTurn;
        }
        else
        {
            turnLight.intensity = enemyIntensityOnTurn;
        }
    }

    //PostEffects
    public void SetActiveVignetteFocus(bool activeFocus) => globalVolume.SetNeedVignetteFocus(activeFocus);
    public void SetActiveChromeAbb(bool active) => globalVolume.SetNeedChromeAbb(active);

    void OnDisable()
    {
            GameController.Instance.states.OnTurnEnded -= (x, y, team) =>
        { OnMovingTeam(GameController.Instance.player.IsMyTurnOrNot()); };
            GameController.Instance.states.OnGameStarted -= () =>
            { OnMovingTeam(GameController.Instance.player.IsMyTurnOrNot()); };
    }
}
