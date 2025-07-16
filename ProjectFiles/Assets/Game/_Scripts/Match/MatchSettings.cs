
public sealed class MatchSettings
{
    //Board
    public int tileCountX;
    public int tileCountY;
    public float tileSize;

    //Turn and team
    public bool firtsMoveZero;
    public int piecesWinSequence;

    //Cards
    public int maxCardsInHand;
    public int startCards;
    public int startMana;
    public int maxMana;
    public int manaForDestroyEnemy;

    public MatchSettings(int tileCountX, int tileCountY, float tileSize, int piecesWinSequence, bool firtsMoveZero, int startMana, int maxMana, 
        int manaForDestroyEnemy, int startCards, int maxCardsInHand)
    {
        this.tileCountX = tileCountX;
        this.tileCountY = tileCountY;
        this.tileSize = tileSize;
        this.firtsMoveZero = firtsMoveZero;
        this.piecesWinSequence = piecesWinSequence;
        this.startCards = startCards;
        this.startMana = startMana;
        this.maxMana = maxMana;
        this.manaForDestroyEnemy = manaForDestroyEnemy;
        this.maxCardsInHand = maxCardsInHand;
    }
}
