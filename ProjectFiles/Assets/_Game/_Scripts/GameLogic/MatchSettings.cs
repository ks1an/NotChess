
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
    public int maxMana;
    public int manaForDestroyEnemy;

    //Cards
    public int maxCardsInHand;
    public int startCards;

    public MatchSettings(int tileCountX, int tileCountY, float tileSize, 
        int piecesWinSequence, bool firtsMoveZero, 
        int startMana, int startManaForEvenPlayer, int maxMana, int manaForDestroyEnemy, 
        int startCards, int maxCardsInHand)
    {
        this.tileCountX = tileCountX;
        this.tileCountY = tileCountY;
        this.tileSize = tileSize;

        this.firtsMoveZero = firtsMoveZero;
        this.piecesWinSequence = piecesWinSequence;

        this.startMana = startMana;
        this.maxMana = maxMana;
        this.startManaForEvenPlayer = startManaForEvenPlayer;
        this.manaForDestroyEnemy = manaForDestroyEnemy;

        this.startCards = startCards;
        this.maxCardsInHand = maxCardsInHand;
    }
}
