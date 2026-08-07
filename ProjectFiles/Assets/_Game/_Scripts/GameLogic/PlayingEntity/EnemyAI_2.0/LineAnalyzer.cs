using UnityEngine;

public class LineAnalyzer
{
    private PieceData[,] pieces;
    private int width, height, winLength;

    public LineAnalyzer(PieceData[,] pieces, Team team, int winLength)
    {
        this.pieces = pieces;
        this.width = pieces.GetLength(0);
        this.height = pieces.GetLength(1);
        this.winLength = winLength;
    }

    public bool HasCompleteLine(Team team)
    {
        return GetLongestLine(team) >= winLength;
    }

    public int GetLongestLine(Team team)
    {
        int max = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (pieces[x, y]?.team == team)
                {
                    max = Mathf.Max(max, CountLine(x, y, 1, 0, team));
                    max = Mathf.Max(max, CountLine(x, y, 0, 1, team));
                    max = Mathf.Max(max, CountLine(x, y, 1, 1, team));
                    max = Mathf.Max(max, CountLine(x, y, 1, -1, team));
                }
        return max;
    }

    private int CountLine(int sx, int sy, int dx, int dy, Team team)
    {
        int c = 0;
        int x = sx, y = sy;
        while (InBounds(x, y) && pieces[x, y]?.team == team)
        {
            c++;
            x += dx;
            y += dy;
        }
        return c;
    }

    private bool InBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    public float GetScore(Team myTeam, float oppWeight = 2.0f)
    {
        Team opp = myTeam == Team.Zero ? Team.Cross : Team.Zero;
        float score = EvaluateTeam(myTeam) * 1.5f - EvaluateTeam(opp) * oppWeight;
        return score;
    }

    private float EvaluateTeam(Team team)
    {
        int longest = GetLongestLine(team);
        if (longest >= winLength) return 10000f;
        if (longest >= winLength - 1) return 500f;
        if (longest >= winLength - 2) return 100f;
        if (longest >= 3) return 20f;
        if (longest >= 2) return 5f;
        if (longest >= 1) return 1f;
        return 0;
    }
}