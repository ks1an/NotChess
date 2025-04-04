public sealed class MatchSettings
{
    public int tileCountX;
    public int tileCountY;
    public float tileSize;
    public int piecesWinSequence = 5;
    public bool firtsMoveZero;

    public MatchSettings(int tileCountX, int tileCountY, float tileSize, int piecesWinSequence, bool firtsMoveZero)
    {
        this.tileCountX = tileCountX;
        this.tileCountY = tileCountY;
        this.tileSize = tileSize;
        this.piecesWinSequence = piecesWinSequence;
        this.firtsMoveZero = firtsMoveZero;
    }
}
