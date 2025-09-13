using System.Collections.Generic;
using UnityEngine;

public sealed class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance;

    [SerializeField] List<FlickeringLight> boardFlickingLights;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    //Light Over Board
    public void DoSmallBoardFlickeringLight()
    {
        for (int i = 0; i < boardFlickingLights.Count; i++)
            boardFlickingLights[i].StartFlickering(3);
    }
    public void DoMediumBoardFlickeringLight()
    {
        for (int i = 0; i < boardFlickingLights.Count; i++)
            boardFlickingLights[i].StartFlickering(6);
    }
}
