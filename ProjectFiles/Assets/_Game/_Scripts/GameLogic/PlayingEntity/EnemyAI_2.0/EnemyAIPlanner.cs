using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyAIPlanner : MonoBehaviour
{
    [Header("Базовые Веса и Бонусы")]
    [SerializeField, Range(0, 1000)] private float attackBonus = 150f;
    [SerializeField, Range(0, 5000)] private float friendUnderThreatBonus = 1500f; // Абсолютный табу-штраф на отдачу
    [SerializeField, Range(0, 1000)] private float enemyUnderThreatBonus = 100f;
    [SerializeField, Range(0, 1000)] private float lineBlockBonus = 200f;
    [SerializeField, Range(0, 10)] private float centerBonusMultiple = 0.5f;
    [SerializeField, Range(0, 100)] private float lineBuildingBonusMultiple = 3f;
    [SerializeField, Range(0, 100)] private float potentialLineBonusMultiple = 15f;
    [SerializeField, Range(0, 100)] private float enemyPotentionalMultiple = 4.0f;

    [Header("Детекция Вилок и Двойных Угроз")]
    [SerializeField] private float forkBonus = 3000f;
    [SerializeField] private float openFourBonus = 6000f; // Критическая угроза 4-в-ряд (включая с дырками)

    [Header("Настройки Поиска MinMax")]
    [SerializeField] private int searchDepth = 4;
    [SerializeField] private float opponentWeight = 3.0f;

    [Header("Человекоподобность (Human-like AI)")]
    [SerializeField, Range(0f, 1f)] private float blunderChance = 0.0f;
    [SerializeField] private float blunderThreshold = 200f;

    private EnemyAI ai;
    private EnemyAI_Sensors sensor;
    private CancellationTokenSource currentCts;

    public enum CellOwner { None, Zero, Cross }

    public struct FastTileStats
    {
        public bool CanPutOnTile;
        public bool CanAttackTile;
        public bool CanLeaveFromTile;
    }

    public class FastBoardState
    {
        public int Width;
        public int Height;
        public CellOwner[,] Board;
        public FastTileStats[,] Stats;
        public CellOwner MyTeam;
        public int WinSequence;

        public FastBoardState Clone()
        {
            var clone = new FastBoardState
            {
                Width = Width,
                Height = Height,
                MyTeam = MyTeam,
                WinSequence = WinSequence,
                Board = new CellOwner[Width, Height],
                Stats = new FastTileStats[Width, Height]
            };
            Array.Copy(Board, clone.Board, Board.Length);
            Array.Copy(Stats, clone.Stats, Stats.Length);
            return clone;
        }
    }

    public void Init(EnemyAI ai, EnemyAI_Sensors sensor)
    {
        this.ai = ai;
        this.sensor = sensor;
    }

    /// <summary>
    /// Отменяет текущий расчет бота (вызывать при рестарте игры)
    /// </summary>
    public void CancelPendingSearch()
    {
        if (currentCts != null)
        {
            currentCts.Cancel();
            currentCts.Dispose();
            currentCts = null;
        }
    }

    public async Task<EnemyAIAction> GetBestMoveAsync()
    {
        CancelPendingSearch();

        currentCts = new CancellationTokenSource();
        var token = currentCts.Token;

        var myTeam = sensor.myTeam;
        FastBoardState initialState = CaptureFastState(myTeam);

        try
        {
            EnemyAIAction bestAction = await Task.Run(() => ComputeBestMove(initialState, token), token);
            return bestAction;
        }
        catch (OperationCanceledException)
        {
            return default;
        }
    }

    public EnemyAIAction GetBestMove()
    {
        var task = GetBestMoveAsync();
        task.Wait();
        return task.Result;
    }

    #region Async Thread MinMax Computation
    private EnemyAIAction ComputeBestMove(FastBoardState currentState, CancellationToken token)
    {
        CellOwner myTeam = currentState.MyTeam;
        var moves = GenerateSmartMovesForTeam(currentState, myTeam);

        if (moves.Count == 0)
        {
            return new EnemyAIAction { Type = ActionType.PlacePawn, TargetCell = Vector2Int.zero };
        }

        // 1. Мгновенные обязательные ходы
        var priorityMove = GetInstantPriorityMove(currentState, moves, myTeam);
        if (priorityMove.HasValue)
            return priorityMove.Value;

        // 2. Сортировка ходов
        moves = OrderMovesByHeuristic(currentState, moves, myTeam);

        List<(EnemyAIAction move, float score)> scoredMoves = new List<(EnemyAIAction, float)>();

        foreach (var move in moves)
        {
            token.ThrowIfCancellationRequested();

            var nextState = SimulateMove(currentState, move, myTeam);
            if (nextState == null) continue;

            if (CheckVictory(nextState, myTeam))
                return move;

            float score = MinMax(nextState, searchDepth - 1, float.MinValue, float.MaxValue, false, token);
            scoredMoves.Add((move, score));
        }

        if (scoredMoves.Count == 0) return moves[0];

        return SelectHumanLikeMove(scoredMoves);
    }

    private List<EnemyAIAction> OrderMovesByHeuristic(FastBoardState s, List<EnemyAIAction> moves, CellOwner team)
    {
        CellOwner opp = GetOpponent(team);

        return moves.OrderByDescending(m =>
        {
            if (m.Type == ActionType.AttackPawn) return 10000;
            if (IsCellAttackedBy(s, m.TargetCell.x, m.TargetCell.y, opp)) return -5000;

            float centerDist = Vector2Int.Distance(m.TargetCell, new Vector2Int(s.Width / 2, s.Height / 2));
            return 100 - centerDist;
        }).ToList();
    }

    private EnemyAIAction SelectHumanLikeMove(List<(EnemyAIAction move, float score)> scoredMoves)
    {
        scoredMoves = scoredMoves.OrderByDescending(x => x.score).ToList();
        float bestScore = scoredMoves[0].score;

        var rand = new System.Random();
        if (rand.NextDouble() < blunderChance && scoredMoves.Count > 1)
        {
            var viableMoves = scoredMoves.Where(x => bestScore - x.score <= blunderThreshold).ToList();
            if (viableMoves.Count > 1)
            {
                int randomIndex = rand.Next(0, Math.Min(viableMoves.Count, 3));
                return viableMoves[randomIndex].move;
            }
        }

        return scoredMoves[0].move;
    }

    private EnemyAIAction? GetInstantPriorityMove(FastBoardState state, List<EnemyAIAction> moves, CellOwner myTeam)
    {
        CellOwner opp = GetOpponent(myTeam);

        foreach (var move in moves)
        {
            var next = SimulateMove(state, move, myTeam);
            if (next != null && CheckVictory(next, myTeam))
                return move;
        }

        foreach (var move in moves)
        {
            var next = SimulateMove(state, move, myTeam);
            if (next == null) continue;

            int oppWinMovesBefore = CountWinningMoves(state, opp);
            int oppWinMovesAfter = CountWinningMoves(next, opp);

            if (oppWinMovesBefore > 0 && oppWinMovesAfter < oppWinMovesBefore)
                return move;
        }

        return null;
    }

    private int CountWinningMoves(FastBoardState s, CellOwner team)
    {
        int count = 0;
        var moves = GenerateSmartMovesForTeam(s, team);
        foreach (var m in moves)
        {
            var next = SimulateMove(s, m, team);
            if (next != null && CheckVictory(next, team)) count++;
        }
        return count;
    }

    private float MinMax(FastBoardState state, int depth, float alpha, float beta, bool isMaximizing, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        CellOwner opp = GetOpponent(state.MyTeam);

        if (CheckVictory(state, state.MyTeam)) return 100000f + depth;
        if (CheckVictory(state, opp)) return -100000f - depth;
        if (depth == 0 || IsDraw(state)) return EvaluateState(state, state.MyTeam);

        CellOwner currentTurnTeam = isMaximizing ? state.MyTeam : opp;
        var moves = GenerateSmartMovesForTeam(state, currentTurnTeam);

        if (moves.Count == 0) return isMaximizing ? -100000f : 100000f;

        if (isMaximizing)
        {
            float maxEval = float.MinValue;
            foreach (var move in moves)
            {
                var next = SimulateMove(state, move, state.MyTeam);
                if (next == null) continue;
                float eval = MinMax(next, depth - 1, alpha, beta, false, token);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) break;
            }
            return maxEval;
        }
        else
        {
            float minEval = float.MaxValue;
            foreach (var move in moves)
            {
                var next = SimulateMove(state, move, opp);
                if (next == null) continue;
                float eval = MinMax(next, depth - 1, alpha, beta, true, token);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break;
            }
            return minEval;
        }
    }
    #endregion

    #region Advanced Evaluation & Gap Detection
    private float EvaluateState(FastBoardState s, CellOwner myTeam)
    {
        CellOwner opp = GetOpponent(myTeam);
        float score = 0;

        score += EvaluateLines(s, myTeam) * 1.5f - EvaluateLines(s, opp) * opponentWeight;
        score += EvaluateForksAndThreats(s, myTeam) * potentialLineBonusMultiple;
        score -= EvaluateForksAndThreats(s, opp) * enemyPotentionalMultiple * potentialLineBonusMultiple;

        score += CenterBonus(s, myTeam);
        score += LineBlockBonus(s, myTeam);
        score += ThreatBonus(s, myTeam);
        score -= VulnerabilityPenalty(s, myTeam);

        return score;
    }

    private float EvaluateForksAndThreats(FastBoardState s, CellOwner team)
    {
        float score = 0;
        int winLen = s.WinSequence;
        int[,] threatMap = new int[s.Width, s.Height];

        (int dx, int dy)[] dirs = { (1, 0), (0, 1), (1, 1), (1, -1) };

        for (int x = 0; x < s.Width; x++)
        {
            for (int y = 0; y < s.Height; y++)
            {
                foreach (var (dx, dy) in dirs)
                {
                    int endX = x + (winLen - 1) * dx;
                    int endY = y + (winLen - 1) * dy;

                    if (!InBounds(endX, endY, s.Width, s.Height)) continue;

                    int teamCount = 0;
                    int emptyCount = 0;
                    int lastEmptyX = -1, lastEmptyY = -1;
                    bool blocked = false;

                    for (int i = 0; i < winLen; i++)
                    {
                        int cx = x + i * dx;
                        int cy = y + i * dy;

                        if (s.Board[cx, cy] == team)
                        {
                            teamCount++;
                        }
                        else if (s.Board[cx, cy] == CellOwner.None)
                        {
                            emptyCount++;
                            lastEmptyX = cx;
                            lastEmptyY = cy;
                        }
                        else
                        {
                            blocked = true;
                            break;
                        }
                    }

                    if (!blocked)
                    {
                        if (teamCount == winLen - 1 && emptyCount == 1)
                        {
                            score += openFourBonus;
                            threatMap[lastEmptyX, lastEmptyY]++;
                        }
                        else if (teamCount == winLen - 2 && emptyCount == 2)
                        {
                            score += lineBuildingBonusMultiple * 10f;
                        }
                    }
                }
            }
        }

        for (int x = 0; x < s.Width; x++)
        {
            for (int y = 0; y < s.Height; y++)
            {
                if (threatMap[x, y] >= 2)
                {
                    score += forkBonus;
                }
            }
        }

        return score;
    }

    private float EvaluateLines(FastBoardState s, CellOwner team)
    {
        int longest = GetLongestLine(s, team);
        if (longest >= s.WinSequence) return 10000f;
        if (longest >= s.WinSequence - 1) return 500f;
        if (longest >= s.WinSequence - 2) return 100f;
        if (longest >= 3) return 20f;
        if (longest >= 2) return 5f;
        return 0;
    }

    private int GetLongestLine(FastBoardState s, CellOwner team)
    {
        int max = 0;
        for (int x = 0; x < s.Width; x++)
            for (int y = 0; y < s.Height; y++)
                if (s.Board[x, y] == team)
                {
                    max = Math.Max(max, CountDir(s, x, y, 1, 0, team) + 1);
                    max = Math.Max(max, CountDir(s, x, y, 0, 1, team) + 1);
                    max = Math.Max(max, CountDir(s, x, y, 1, 1, team) + 1);
                    max = Math.Max(max, CountDir(s, x, y, 1, -1, team) + 1);
                }
        return max;
    }

    private float CenterBonus(FastBoardState s, CellOwner myTeam)
    {
        if (centerBonusMultiple <= 0) return 0;
        float bonus = 0;
        float centerX = (s.Width - 1) / 2f, centerY = (s.Height - 1) / 2f;
        float maxDist = (float)Math.Sqrt(centerX * centerX + centerY * centerY);

        for (int x = 0; x < s.Width; x++)
            for (int y = 0; y < s.Height; y++)
                if (s.Board[x, y] == myTeam)
                {
                    float dist = (float)Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    bonus += (maxDist - dist) * centerBonusMultiple;
                }
        return bonus;
    }

    private float LineBlockBonus(FastBoardState s, CellOwner myTeam)
    {
        if (lineBlockBonus <= 0) return 0;
        float bonus = 0;
        CellOwner opp = GetOpponent(myTeam);
        int[] dx = { -1, -1, 1, 1 }, dy = { -1, 1, -1, 1 };

        for (int x = 0; x < s.Width; x++)
            for (int y = 0; y < s.Height; y++)
            {
                if (s.Board[x, y] != myTeam || !s.Stats[x, y].CanLeaveFromTile) continue;

                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d], ny = y + dy[d];
                    if (InBounds(nx, ny, s.Width, s.Height) && s.Board[nx, ny] == opp)
                    {
                        int lineLen = GetMaxLineLength(s, nx, ny, opp);
                        if (lineLen >= 2) bonus += lineBlockBonus * (lineLen - 1);
                    }
                }
            }
        return bonus;
    }

    private float ThreatBonus(FastBoardState s, CellOwner myTeam)
    {
        if (enemyUnderThreatBonus <= 0 && attackBonus <= 0) return 0;
        float bonus = 0;
        CellOwner opp = GetOpponent(myTeam);
        int[] dx = { -1, -1, 1, 1 }, dy = { -1, 1, -1, 1 };

        for (int x = 0; x < s.Width; x++)
            for (int y = 0; y < s.Height; y++)
            {
                if (s.Board[x, y] != myTeam || !s.Stats[x, y].CanLeaveFromTile) continue;

                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d], ny = y + dy[d];
                    if (InBounds(nx, ny, s.Width, s.Height) && s.Board[nx, ny] == opp && s.Stats[nx, ny].CanAttackTile)
                    {
                        bonus += enemyUnderThreatBonus + attackBonus;
                    }
                }
            }
        return bonus;
    }

    private float VulnerabilityPenalty(FastBoardState s, CellOwner myTeam)
    {
        float penalty = 0;
        CellOwner opp = GetOpponent(myTeam);

        for (int x = 0; x < s.Width; x++)
        {
            for (int y = 0; y < s.Height; y++)
            {
                if (s.Board[x, y] == myTeam)
                {
                    if (IsCellAttackedBy(s, x, y, opp))
                    {
                        penalty += friendUnderThreatBonus;
                    }
                }
            }
        }
        return penalty;
    }

    private bool IsCellAttackedBy(FastBoardState s, int targetX, int targetY, CellOwner attackerTeam)
    {
        int[] dx = { -1, -1, 1, 1 };
        int[] dy = { -1, 1, -1, 1 };

        for (int d = 0; d < 4; d++)
        {
            int ax = targetX + dx[d];
            int ay = targetY + dy[d];

            if (InBounds(ax, ay, s.Width, s.Height))
            {
                if (s.Board[ax, ay] == attackerTeam)
                {
                    if (s.Stats[ax, ay].CanLeaveFromTile && s.Stats[targetX, targetY].CanAttackTile)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private int GetMaxLineLength(FastBoardState s, int x, int y, CellOwner team)
    {
        int maxLen = 0;
        foreach (var (dx, dy) in new[] { (1, 0), (0, 1), (1, 1), (1, -1) })
        {
            int len = 1 + CountDir(s, x, y, dx, dy, team) + CountDir(s, x, y, -dx, -dy, team);
            if (len > maxLen) maxLen = len;
        }
        return maxLen;
    }

    private int CountDir(FastBoardState s, int sx, int sy, int dx, int dy, CellOwner team)
    {
        int c = 0;
        int x = sx + dx, y = sy + dy;
        while (InBounds(x, y, s.Width, s.Height) && s.Board[x, y] == team)
        {
            c++;
            x += dx;
            y += dy;
        }
        return c;
    }
    #endregion

    #region Smart Moves Generation
    private List<EnemyAIAction> GenerateSmartMovesForTeam(FastBoardState s, CellOwner team)
    {
        var moves = new List<EnemyAIAction>();
        CellOwner opp = GetOpponent(team);
        int[] dx = { -1, -1, 1, 1 };
        int[] dy = { -1, 1, -1, 1 };

        bool isBoardEmpty = true;
        bool[,] activeZone = new bool[s.Width, s.Height];

        for (int x = 0; x < s.Width; x++)
        {
            for (int y = 0; y < s.Height; y++)
            {
                if (s.Board[x, y] != CellOwner.None)
                {
                    isBoardEmpty = false;
                    MarkActiveZone(activeZone, x, y, s.Width, s.Height);
                }
            }
        }

        for (int x = 0; x < s.Width; x++)
        {
            for (int y = 0; y < s.Height; y++)
            {
                if (s.Board[x, y] == CellOwner.None && s.Stats[x, y].CanPutOnTile)
                {
                    if (isBoardEmpty || activeZone[x, y])
                    {
                        bool isSuicide = IsCellAttackedBy(s, x, y, opp);
                        if (!isSuicide)
                        {
                            moves.Add(new EnemyAIAction { Type = ActionType.PlacePawn, TargetCell = new Vector2Int(x, y) });
                        }
                    }
                }

                if (s.Board[x, y] == team && s.Stats[x, y].CanLeaveFromTile)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d], ny = y + dy[d];
                        if (InBounds(nx, ny, s.Width, s.Height) && s.Board[nx, ny] == opp)
                        {
                            if (s.Stats[nx, ny].CanAttackTile)
                            {
                                moves.Add(new EnemyAIAction
                                {
                                    Type = ActionType.AttackPawn,
                                    SourceCell = new Vector2Int(x, y),
                                    TargetCell = new Vector2Int(nx, ny)
                                });
                            }
                        }
                    }
                }
            }
        }

        if (moves.Count == 0 && !isBoardEmpty)
        {
            for (int x = 0; x < s.Width; x++)
                for (int y = 0; y < s.Height; y++)
                    if (s.Board[x, y] == CellOwner.None && s.Stats[x, y].CanPutOnTile && activeZone[x, y])
                        moves.Add(new EnemyAIAction { Type = ActionType.PlacePawn, TargetCell = new Vector2Int(x, y) });
        }

        return moves;
    }

    private void MarkActiveZone(bool[,] zone, int cx, int cy, int w, int h)
    {
        for (int rx = -2; rx <= 2; rx++)
        {
            for (int ry = -2; ry <= 2; ry++)
            {
                int nx = cx + rx;
                int ny = cy + ry;
                if (InBounds(nx, ny, w, h))
                {
                    zone[nx, ny] = true;
                }
            }
        }
    }
    #endregion

    #region State Capture & Helpers
    private FastBoardState CaptureFastState(Team myTeam)
    {
        var piecesC = Board.Instance.piecesController;
        var tilesC = Board.Instance.tilesController;
        int w = GameController.Instance.matchSettings.tileCountX;
        int h = GameController.Instance.matchSettings.tileCountY;

        var state = new FastBoardState
        {
            Width = w,
            Height = h,
            MyTeam = ConvertTeam(myTeam),
            WinSequence = GameController.Instance.matchSettings.piecesWinSequence,
            Board = new CellOwner[w, h],
            Stats = new FastTileStats[w, h]
        };

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var p = piecesC.pieces[x, y];
                state.Board[x, y] = p == null ? CellOwner.None : ConvertTeam(p.team);

                var st = tilesC.tiles[x, y].Stats.CurrentStats;
                state.Stats[x, y] = new FastTileStats
                {
                    CanPutOnTile = st.CanPutOnTile,
                    CanAttackTile = st.CanAttackTile,
                    CanLeaveFromTile = st.CanLeaveFromTile
                };
            }
        }
        return state;
    }

    private FastBoardState SimulateMove(FastBoardState state, EnemyAIAction move, CellOwner actingTeam)
    {
        var newState = state.Clone();
        switch (move.Type)
        {
            case ActionType.PlacePawn:
                newState.Board[move.TargetCell.x, move.TargetCell.y] = actingTeam;
                break;
            case ActionType.AttackPawn:
                newState.Board[move.TargetCell.x, move.TargetCell.y] = newState.Board[move.SourceCell.x, move.SourceCell.y];
                newState.Board[move.SourceCell.x, move.SourceCell.y] = CellOwner.None;
                break;
            default:
                return null;
        }
        return newState;
    }

    private bool CheckVictory(FastBoardState s, CellOwner t) => GetLongestLine(s, t) >= s.WinSequence;

    private CellOwner ConvertTeam(Team t) => t == Team.Zero ? CellOwner.Zero : CellOwner.Cross;

    private CellOwner GetOpponent(CellOwner t) => t == CellOwner.Zero ? CellOwner.Cross : CellOwner.Zero;

    private bool InBounds(int x, int y, int w, int h) => x >= 0 && x < w && y >= 0 && y < h;

    private bool IsDraw(FastBoardState state)
    {
        for (int x = 0; x < state.Width; x++)
            for (int y = 0; y < state.Height; y++)
                if (state.Board[x, y] == CellOwner.None && state.Stats[x, y].CanPutOnTile)
                    return false;
        return true;
    }
    #endregion
}