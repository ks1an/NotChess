using UnityEngine;

public sealed class MatchController : MonoBehaviour
{
    public static MatchController Instance { get; private set; }
    public PlayerSettings player;
    public Board board;
    public NetMatchSync netMatch;

    [HideInInspector] public MatchSettings settings;
    [HideInInspector] public MatchStates states;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            states = GetComponent<MatchStates>();
            netMatch.Preset();
            DontDestroyOnLoad(this);
        }
    }

    public void CreateGame(bool isNetMatch, int tileCountX, int tileCountY, float tileSize, int winSequence, bool firstMoveZero)
    {
        settings = new MatchSettings(tileCountX, tileCountY, tileSize, winSequence, firstMoveZero);
        states.CreateGame(isNetMatch);
    }
}
