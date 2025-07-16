using Unity.Netcode;
using UnityEngine;

public sealed class MatchController : MonoBehaviour
{
    public static MatchController Instance { get; private set; }

    [Header("Net")]
    public GameObject netSyncPrefab;
    [HideInInspector] public NetMatchSync netMatch;

    [Space(10)]
    public Player player;
    public Board board;

    [HideInInspector] public MatchSettings settings;
    [HideInInspector] public MatchStates states;
    [HideInInspector] public GlobalCardCollection globalCardCollection;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            globalCardCollection = GetComponent<GlobalCardCollection>();
            globalCardCollection.CreateGlobalCards();
            states = GetComponent<MatchStates>();
            DontDestroyOnLoad(this);
        }
    }

    #region Create
    public void CreateGame(bool isNetMatch, int tileCountX = 8, int tileCountY = 8, float tileSize = 1.0f, int winSequence = 5, bool firstMoveZero = true,
        int startMana = 0, int maxMana = 10, int manaForDestoryEnemy = 1, int startCards = 5, int maxCardInHand = 5)
    {
        settings = new MatchSettings(tileCountX, tileCountY, tileSize, winSequence, firstMoveZero,startMana, maxMana,
            manaForDestoryEnemy,startCards, maxCardInHand);
        states.CreateGame(isNetMatch);
    }

    public void CreateNetSync()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        netMatch = Instantiate(netSyncPrefab).GetComponent<NetMatchSync>();
        netMatch.gameObject.GetComponent<NetworkObject>().Spawn();
    }
    #endregion
}
