using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(GlobalCardCollection), typeof(MatchStates))]
public sealed class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Net")]
    public GameObject netSyncPrefab;
    [HideInInspector] public NetMatchSync netMatch;

    [Space(10)]
    public Player player;
    public Enemy enemy;
    public GameObject botPrefab;

    [Header("Board")]
    public Board board;
    public ManaBottle playerManaBottle, enemyManaBottle;
    public StackView enemyGraveCoinView;

    [HideInInspector] public GameSettingsModel gameSettings;
    [HideInInspector] public MatchSettings matchSettings;
    [HideInInspector] public MatchStates states;
    [HideInInspector] public GlobalCardCollection globalCards;
    [HideInInspector] public SecondTimer secTimer;
    [HideInInspector] bool isDemonstration;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            globalCards = GetComponent<GlobalCardCollection>();
            globalCards.CreateGlobalCards();
            states = GetComponent<MatchStates>();
            secTimer = gameObject.AddComponent<SecondTimer>();
            player = GameObject.Instantiate(player.gameObject).GetComponent<Player>();
            enemy = GameObject.Instantiate(enemy.gameObject).GetComponent<Enemy>();

            var loader = new SettingsLoader();
            loader.LoadSettings();

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
        int maxGraveTokens = 10, int graveTokensForKill = 1,
        int startCards = 6, int defaultCardsInHand = 5, int maxDeck = 26)
    {
        if (isDemonstration && !isMatchAiVsAi)
            states.EndDemonstrationGame();

        matchSettings = new MatchSettings(8, 8, 1.25f,
            winSequence, firstMoveZero,
            startMana, startManaForEvenPlayer, manaPerTurn, maxMana, manaForDestoryEnemy,
            maxGraveTokens, graveTokensForKill,
            startCards, defaultCardsInHand, maxDeck);

        states.CreateGame(isNetMatch, isMatchAiVsAi);
    }

    public void CreateNetSync()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        netMatch = Instantiate(netSyncPrefab).GetComponent<NetMatchSync>();
        netMatch.gameObject.GetComponent<NetworkObject>().Spawn();
    }
    
    public void UpdateGameSettings(GameSettingsModel newSettings) => gameSettings = newSettings;
}
