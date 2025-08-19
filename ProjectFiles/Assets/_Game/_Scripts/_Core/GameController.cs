using Unity.Netcode;
using UnityEngine;

public sealed class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Net")]
    public GameObject netSyncPrefab;
    [HideInInspector] public NetMatchSync netMatch;

    [Space(10)]
    public Player player;
    public GameObject botPrefab;
    public Board board;


    [HideInInspector] public MatchSettings settings;
    [HideInInspector] public MatchStates states;
    [HideInInspector] public GlobalCardCollection globalCardCollection;
    [HideInInspector] bool isDemonstration;


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


    public void CreateDemostrationGame() 
    {
        CreateGame(false, true);
        isDemonstration = true;
    }

    public void CreateGame(bool isNetMatch, bool isMatchAiVsAi = false,
        int winSequence = 5, bool firstMoveZero = true,
        int startMana = 0, int manaPerTurn = 1, int startManaForEvenPlayer = 1, int maxMana = 10, int manaForDestoryEnemy = 1, 
        int startCards = 5, int maxCardInHand = 5)
    {
        if (isDemonstration && !isMatchAiVsAi)
            states.EndDemonstrationGame();

        settings = new MatchSettings(8, 8, 1.25f, 
            winSequence, firstMoveZero, 
            startMana, startManaForEvenPlayer, manaPerTurn, maxMana, manaForDestoryEnemy, 
            startCards, maxCardInHand);

        states.CreateGame(isNetMatch, isMatchAiVsAi);
    }

    public void CreateNetSync()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        netMatch = Instantiate(netSyncPrefab).GetComponent<NetMatchSync>();
        netMatch.gameObject.GetComponent<NetworkObject>().Spawn();
    }
}
