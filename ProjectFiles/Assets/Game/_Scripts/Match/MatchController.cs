using Unity.Netcode;
using UnityEngine;

public sealed class MatchController : MonoBehaviour
{
    public static MatchController Instance { get; private set; }
    public PlayerSettings player;
    public Board board;

    public GameObject netSyncPrefab;
    [HideInInspector] public NetMatchSync netMatch;
    [HideInInspector] public MatchSettings settings;
    [HideInInspector] public MatchStates states;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            states = GetComponent<MatchStates>();
            DontDestroyOnLoad(this);
        }
    }

    public void CreateGame(bool isNetMatch, int tileCountX, int tileCountY, float tileSize, int winSequence, bool firstMoveZero)
    {
        settings = new MatchSettings(tileCountX, tileCountY, tileSize, winSequence, firstMoveZero);
        states.CreateGame(isNetMatch);
    }

    public void CreateNetSync()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        netMatch = Instantiate(netSyncPrefab).GetComponent<NetMatchSync>();
        netMatch.gameObject.GetComponent<NetworkObject>().Spawn();
    }
}
