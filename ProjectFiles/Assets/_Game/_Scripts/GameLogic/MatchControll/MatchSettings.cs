
public sealed class MatchSettings
{
    //Board
    public int tileCountX; 
    public int tileCountY;
    public float tileSize;

    //Turn and team
    public bool firtsMoveZero;
    public int piecesWinSequence;

    //Mana
    public int startMana;
    public int startManaForEvenPlayer;
    public int manaPerTurn;
    public int maxMana;
    public int manaForDestroyEnemy;
    public int graveTokensForDestroyEnemy;
    public int maxGraveTokens;

    //Cards
    public int defaultCardsInHand;
    public int startCards;
    public int maxDeck;

    public MatchSettings(int tileCountX, int tileCountY, float tileSize,
        int piecesWinSequence, bool firtsMoveZero,
        int startMana, int startManaForEvenPlayer, int manaPerTurn, int maxMana, int manaForDestroyEnemy, int maxGraveTokens, int graveTokensForDestroyEnemy,
        int startCards, int defaultCardsInHand, int maxDeck)
    {
        this.tileCountX = tileCountX;
        this.tileCountY = tileCountY;
        this.tileSize = tileSize;

        this.firtsMoveZero = firtsMoveZero;
        this.piecesWinSequence = piecesWinSequence;

        this.startMana = startMana;
        this.maxMana = maxMana;
        this.startManaForEvenPlayer = startManaForEvenPlayer;
        this.manaPerTurn = manaPerTurn;
        this.manaForDestroyEnemy = manaForDestroyEnemy;
        this.maxGraveTokens = maxGraveTokens;
        this.graveTokensForDestroyEnemy = graveTokensForDestroyEnemy;

        this.startCards = startCards;
        this.defaultCardsInHand = defaultCardsInHand;
        this.maxDeck = maxDeck;
    }
}
