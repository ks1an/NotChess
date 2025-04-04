using UnityEngine;

public sealed class PlayerSettings : MonoBehaviour
{
    public float upValueWhileSelectingPiece;
    public Material tileFirstMaterial, tileSecondMaterial;
    public Material crossMaterial, zeroMaterial;
    public GameObject crossPrefab, zeroPrefab;
    //material for available moves
    //material for hover tiles

    Team localPlayerTeam = Team.None;

    void Awake()
    {
        DontDestroyOnLoad(this); 
    }

    public void SetPlayerTeam(Team type) => localPlayerTeam = type;

    public Team GetLocalPlayerTeam()
    {
        return localPlayerTeam;
    }

}
