using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyAIPlanner : MonoBehaviour
{
    [Header("Base linebuilding")]
    [SerializeField, Range(0, 1000)] float attackBonus = 150f;
    [SerializeField, Range(0, 5000)] float friendUnderThreatBonus = 1200f;
    [SerializeField, Range(0, 1000)] float enemyUnderThreatBonus = 100f;
    [SerializeField, Range(0, 1000)] float lineBlockBonus = 250f;
    [SerializeField, Range(0, 10)] float centerBonusMultiple = 0.5f;
    [SerializeField, Range(0, 100)] float lineBuildingBonusMultiple = 10f;
    [SerializeField, Range(0, 100)] float potentialLineBonusMultiple = 20f;
    [SerializeField, Range(0, 100)] float enemyPotentionalMultiple = 2.2f;
    [Space(10)]
    [SerializeField] float nearWinAttackBonus = 3000f;
    [SerializeField] float attackSetupBonus = 500f;
    [SerializeField] float preventEnemyWinBonus = 2800f;
    [SerializeField] float blockEarlySetupBonus = 600f;

    [Header("Forks")]
    [SerializeField] float forkBonus = 3500f;
    [SerializeField] float openFourBonus = 6000f;
    [SerializeField] float hybridCounterAttackBonus = 3000f;

    [Header("Minmax")]
    [SerializeField] int searchDepth = 4;
    [SerializeField] int maxBranchingFactor = 14;
    [SerializeField] float myWeight = 1.5f;
    [SerializeField] float opponentWeight = 2.2f;
    [SerializeField] float repeatPreviousBoardStatePenalty = 1000f;
    [SerializeField] float feedingPenalty = 2000f;

    [Header("Human-like AI")]
    [SerializeField, Range(0f, 1f)] float blunderChance = 0.0f;
    [SerializeField] float blunderThreshold = 200f;

    [Header("Cards")]
    [SerializeField] float cardsPriorityMove = 8000f;
    [SerializeField] float destroyEnemyByCard = 3000f;
    [SerializeField] float manaCardSpendingMultiple = 2f;
    [SerializeField] float bonesCardSpendingMultiple = 2f;

    [Header("Landscape")]
    [SerializeField] float reductionFactorDurationFriendUnderWalls = 5;
    [SerializeField] float placePawnUnderWallsPonus = 1000f;

    private EnemyAI ai;
    private EnemyAI_Sensors sensor;

    public enum CellOwner { None, Zero, Cross }

    public void Init(EnemyAI ai, EnemyAI_Sensors sensor)
    {
        this.ai = ai;
        this.sensor = sensor;
    }

    public async Task<EnemyAIAction> GetBestMoveAsync(Team team, CancellationToken token)
    {
        FastBoardState initialState = CaptureFastState(team);
        try
        {
            EnemyAIAction bestAction = await Task.Run(() => ComputeBestMove(initialState, token), token);
            return bestAction;
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("[MinMax] ComputeBestMove was cancelled.");
            return default;
        }
    }

    public EnemyAIAction GetBestMove(Team team)
    {
        var task = GetBestMoveAsync(team, CancellationToken.None);
        task.Wait();
        return task.Result;
    }

    #region Async Thread MinMax Computation
    private EnemyAIAction ComputeBestMove(FastBoardState currentState, CancellationToken token)
    {
        CellOwner myTeam = currentState.MyTeam;

        var moves = GenerateSmartMovesForTeam(currentState, myTeam);

        // 2. ВАЖНО: Добавляем ходы картами СРАЗУ (включая LightningBolt)
        if (currentState.Hand != null && currentState.Hand.Count > 0)
        {
            foreach (var card in currentState.Hand)
            {
                // Безопасная проверка ссылки для фонового потока (без вызова Unity API)
                if (System.Object.ReferenceEquals(card, null) || card.ManaCost > currentState.Mana)
                    continue;

                if (card is ICardAI aiCard)
                {
                    var possibleTargetSets = aiCard.GetTargets(currentState, myTeam);
                    if (possibleTargetSets != null)
                    {
                        foreach (var targets in possibleTargetSets)
                        {
                            moves.Add(new EnemyAIAction
                            {
                                Type = ActionType.PlayCard,
                                CardToPlay = card,
                                CardTargets = targets
                            });
                        }
                    }
                }
            }
        }

        // 3. FALLBACK: Защита от "moves = 0", если все обычные клетки посчитались суицидальными
        if (moves.Count == 0)
        {
            for (int x = 0; x < currentState.Width; x++)
            {
                for (int y = 0; y < currentState.Height; y++)
                {
                    if (currentState.Board[x, y] == CellOwner.None && currentState.Stats[x, y].CanPutOnTile)
                    {
                        moves.Add(new EnemyAIAction
                        {
                            Type = ActionType.PlacePawn,
                            TargetCell = new Vector2Int(x, y)
                        });
                    }
                }
            }
        }

        // Если свободных клеток на доске нет физически
        if (moves.Count == 0)
        {
            Debug.LogWarning("[MinMax] Board is completely locked! No valid moves available.");
            return new EnemyAIAction { Type = ActionType.None};
        }

        // 4. Мгновенная проверка приоритетов (теперь видит и победу, и разрыв комбо врага картами)
        var priorityMove = GetInstantPriorityMove(currentState, moves, myTeam);
        if (priorityMove.HasValue)
        {
            return priorityMove.Value;
        }

        // 5. Ранжирование и отсечение ветвления (карты участвуют в ранжировании)
        moves = FilterAndOrderMoves(currentState, moves, myTeam, maxBranchingFactor);

        // 6. Оценка MinMax
        List<(EnemyAIAction move, float score)> scoredMoves = new();
        foreach (var move in moves)
        {
            token.ThrowIfCancellationRequested();

            var nextState = SimulateMove(currentState, move, myTeam);
            if (nextState == null) continue;

            if (CheckVictory(nextState, myTeam))
                return move;

            float score = MinMax(nextState, searchDepth - 1, float.MinValue, float.MaxValue, false, token);

            if (previousBoardHashes.Contains(GetBoardHash(nextState)))
                score -= repeatPreviousBoardStatePenalty;

            scoredMoves.Add((move, score));
        }

        if (scoredMoves.Count == 0) return moves[0];

        return SelectHumanLikeMove(scoredMoves);
    }

    private List<EnemyAIAction> FilterAndOrderMoves(FastBoardState s, List<EnemyAIAction> moves, CellOwner team, int maxCount)
    {
        CellOwner opp = GetOpponent(team);

        var rankedMoves = moves.Select(m =>
        {
            float priority = 0;

            if (m.Type == ActionType.PlayCard)
            {
                priority += cardsPriorityMove;

                if (m.CardTargets != null && m.CardTargets.Count > 0)
                {
                    float bestTargetScore = 0f;
                    bool hitsEnemy = false;

                    foreach (var t in m.CardTargets)
                    {
                        float v = FastEvaluateMovePotential(s, t.x, t.y, team);
                        if (v > bestTargetScore) bestTargetScore = v;

                        if (s.Board[t.x, t.y] == opp) hitsEnemy = true;
                    }

                    priority += bestTargetScore;
                    if (hitsEnemy) priority += destroyEnemyByCard;
                }
            }
            else if (m.Type == ActionType.AttackPawn)
            {
                priority += 10000;
            }
            else // PlacePawn
            {
                var sim = SimulateMove(s, m, team);
                if (sim != null && CheckVictory(sim, team))
                {
                    priority += 50000;
                }
                else
                {
                    float potentialScore = FastEvaluateMovePotential(s, m.TargetCell.x, m.TargetCell.y, team);
                    priority += potentialScore;

                    bool isCriticalDefense = potentialScore >= 1800;
                    if (!isCriticalDefense && IsCellAttackedBy(s, m.TargetCell.x, m.TargetCell.y, opp))
                    {
                        priority -= feedingPenalty;
                    }

                    float centerDist = Vector2Int.Distance(m.TargetCell, new Vector2Int(s.Width / 2, s.Height / 2));
                    priority += (10 - centerDist);

                    if (!s.Stats[m.TargetCell.x, m.TargetCell.y].CanAttackTile && potentialScore > 0)
                    {
                        int duration = s.Stats[m.TargetCell.x, m.TargetCell.y].BanAttackTileDuration;
                        float durationFactor = Mathf.Clamp(duration / reductionFactorDurationFriendUnderWalls, 0f, 1f);
                        priority += placePawnUnderWallsPonus * (potentialScore / blockEarlySetupBonus) * durationFactor;
                    }
                }
            }

            return (move: m, priority: priority);
        })
        .OrderByDescending(x => x.priority)
        .Select(x => x.move)
        .Take(maxCount)
        .ToList();

        return rankedMoves;
    }

    private float FastEvaluateMovePotential(FastBoardState s, int x, int y, CellOwner team)
    {
        float score = 0;
        CellOwner opp = GetOpponent(team);
        (int dx, int dy)[] dirs = { (1, 0), (0, 1), (1, 1), (1, -1) };

        foreach (var (dx, dy) in dirs)
        {
            int myCount = CountDir(s, x, y, dx, dy, team) + CountDir(s, x, y, -dx, -dy, team);
            int oppCount = CountDir(s, x, y, dx, dy, opp) + CountDir(s, x, y, -dx, -dy, opp);

            if (myCount >= 3) score += nearWinAttackBonus;
            else if (myCount >= 2) score += attackSetupBonus;

            if (oppCount >= 3) score += preventEnemyWinBonus;
            else if (oppCount >= 2) score += blockEarlySetupBonus;
        }

        return score;
    }

    private static readonly System.Random _rng = new();
    private EnemyAIAction SelectHumanLikeMove(List<(EnemyAIAction move, float score)> scoredMoves)
    {
        scoredMoves = scoredMoves.OrderByDescending(x => x.score).ToList();
        float bestScore = scoredMoves[0].score;

        var rand = _rng;
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

        // 1. Мгновенная собственная победа (пешкой или картой)
        foreach (var move in moves)
        {
            var next = SimulateMove(state, move, myTeam);
            if (next != null && CheckVictory(next, myTeam))
                return move;
        }

        // 2. Сканирование победных ходов противника (поиск критической угрозы)
        var oppMoves = GenerateSmartMovesForTeam(state, opp);

        foreach (var oppMove in oppMoves)
        {
            var oppNext = SimulateMove(state, oppMove, opp);
            if (oppNext != null && CheckVictory(oppNext, opp))
            {
                Vector2Int blockCell = oppMove.TargetCell;

                // Проверяем: находится ли блокирующая клетка под прямой/боковой атакой врага
                bool isBlockCellUnderAttack = IsCellAttackedBy(state, blockCell.x, blockCell.y, opp);

                // СЦЕНАРИЙ А: Клетка безопасна. Пытаемся заблокировать бесплатной пешкой, экономя ману.
                if (!isBlockCellUnderAttack)
                {
                    var blockingPawnMove = moves.FirstOrDefault(m =>
                        m.Type == ActionType.PlacePawn && m.TargetCell == blockCell);

                    if (!blockingPawnMove.Equals(default(EnemyAIAction)))
                    {
                        return blockingPawnMove; // Эффективный блок пешкой без потери ресурсов
                    }
                }

                // СЦЕНАРИЙ Б: Клетка под атакой (пешка отдаст Bones врагу) ИЛИ нет пешки для блока.
                // Ищем карту (например, LightningBolt), которая разрушит выигрышную цепочку врага!
                var cardMoves = moves.Where(m => m.Type == ActionType.PlayCard).ToList();

                // Сначала отдаем приоритет картам, бьющим по ПЕШКАМ противника, формирующим угрозу
                var targetedCardMove = cardMoves.FirstOrDefault(m =>
                    m.CardTargets != null && m.CardTargets.Any(t => state.Board[t.x, t.y] == opp));

                var preferredCardMoves = targetedCardMove.Equals(default(EnemyAIAction))
                    ? cardMoves
                    : cardMoves.OrderByDescending(m => m.Equals(targetedCardMove)).ToList();

                foreach (var cardMove in preferredCardMoves)
                {
                    var stateAfterCard = SimulateMove(state, cardMove, myTeam);
                    if (stateAfterCard == null) continue;

                    // Проверяем, может ли враг ВСЕ ЕЩЕ победить после применения нашей карты
                    var oppNextAfterCard = SimulateMove(stateAfterCard, oppMove, opp);
                    if (oppNextAfterCard == null || !CheckVictory(oppNextAfterCard, opp))
                    {
                        return cardMove; // Разрушили выигрышную комбинацию врага картой
                    }
                }

                // СЦЕНАРИЙ В: Карт/маны нет, а клетка под атакой.
                // Ставим пешку как крайнюю меру (отдаем Bones, но предотвращаем поражение в этот ход).
                if (isBlockCellUnderAttack)
                {
                    var desperationPawnBlock = moves.FirstOrDefault(m =>
                        m.Type == ActionType.PlacePawn && m.TargetCell == blockCell);

                    if (!desperationPawnBlock.Equals(default(EnemyAIAction)))
                    {
                        return desperationPawnBlock;
                    }
                }
            }
        }

        return null;
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

        moves = FilterAndOrderMoves(state, moves, currentTurnTeam, maxBranchingFactor);

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

    #region Balanced Evaluation
    private float EvaluateState(FastBoardState s, CellOwner myTeam)
    {
        CellOwner opp = GetOpponent(myTeam);
        float score = 0;

        score += EvaluateLines(s, myTeam) * myWeight - EvaluateLines(s, opp) * opponentWeight;
        score += EvaluateForksAndThreats(s, myTeam) * potentialLineBonusMultiple;
        score -= EvaluateForksAndThreats(s, opp) * enemyPotentionalMultiple * potentialLineBonusMultiple;

        score += CenterBonus(s, myTeam);
        score += LineBlockBonus(s, myTeam);
        score += ThreatBonus(s, myTeam);
        score -= VulnerabilityPenalty(s, myTeam);

        score += (s.MaxMana - s.Mana) * manaCardSpendingMultiple;
        score += (s.MaxBones - s.Bones) * bonesCardSpendingMultiple;

        return score;
    }

    private float EvaluateForksAndThreats(FastBoardState s, CellOwner team)
    {
        float score = 0;
        CellOwner opp = GetOpponent(team);
        int winLen = s.WinSequence;

        int[,] threatMap = new int[s.Width, s.Height];
        int[,] myBuildMap = new int[s.Width, s.Height];

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
                    int oppCount = 0;
                    int emptyCount = 0;
                    int lastEmptyX = -1, lastEmptyY = -1;

                    for (int i = 0; i < winLen; i++)
                    {
                        int cx = x + i * dx;
                        int cy = y + i * dy;

                        if (s.Board[cx, cy] == team) teamCount++;
                        else if (s.Board[cx, cy] == opp) oppCount++;
                        else
                        {
                            emptyCount++;
                            lastEmptyX = cx;
                            lastEmptyY = cy;
                        }
                    }

                    if (oppCount == 0)
                    {
                        if (teamCount == winLen - 1 && emptyCount == 1)
                        {
                            score += openFourBonus;
                            threatMap[lastEmptyX, lastEmptyY]++;
                            myBuildMap[lastEmptyX, lastEmptyY] += 3;
                        }
                        else if (teamCount == winLen - 2 && emptyCount == 2)
                        {
                            score += lineBuildingBonusMultiple * 12f;
                            if (lastEmptyX != -1) myBuildMap[lastEmptyX, lastEmptyY] += 1;
                        }
                    }
                }
            }
        }

        for (int x = 0; x < s.Width; x++)
        {
            for (int y = 0; y < s.Height; y++)
            {
                if (threatMap[x, y] >= 2) score += forkBonus;
                if (threatMap[x, y] >= 1 && myBuildMap[x, y] >= 1) score += hybridCounterAttackBonus;
            }
        }

        return score;
    }

    private float EvaluateLines(FastBoardState s, CellOwner team)
    {
        int longest = GetLongestLine(s, team);
        if (longest >= s.WinSequence) return 10000f;
        if (longest >= s.WinSequence - 1) return 5000f;
        if (longest >= s.WinSequence - 2) return 150f;
        if (longest >= 3) return 35f;
        if (longest >= 2) return 10f;
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
                        if (!s.Stats[nx, ny].CanAttackTile) continue;

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
                        if (!s.Stats[x, y].CanAttackTile)
                        {
                            int currentDuration = s.Stats[x, y].BanAttackTileDuration;
                            float durationFactor = Math.Clamp(currentDuration / reductionFactorDurationFriendUnderWalls, 0f, 1f);
                            float reduction = 1f + (0.1f - 1f) * durationFactor;

                            penalty += friendUnderThreatBonus * reduction;
                        }
                        else
                        {
                            penalty += friendUnderThreatBonus;
                        }
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
                    MarkWideActiveZone(activeZone, x, y, s.Width, s.Height);
                }
            }
        }

        for (int x = 0; x < s.Width; x++)
        {
            for (int y = 0; y < s.Height; y++)
            {
                // 1. ПОСТАНОВКА ПЕШКИ (PlacePawn)
                if (s.Board[x, y] == CellOwner.None && s.Stats[x, y].CanPutOnTile)
                {
                    if (isBoardEmpty || activeZone[x, y])
                    {
                        var placeAction = new EnemyAIAction { Type = ActionType.PlacePawn, TargetCell = new Vector2Int(x, y) };
                        var sim = SimulateMove(s, placeAction, team);

                        bool isWinningMove = (sim != null && CheckVictory(sim, team));

                        float potential = FastEvaluateMovePotential(s, x, y, team);
                        bool isProtectedByWalls = !s.Stats[x, y].CanAttackTile;
                        bool isCriticalMove = isWinningMove || potential >= 1800 || isProtectedByWalls;

                        bool isSuicide = !isCriticalMove && IsCellAttackedBy(s, x, y, opp);

                        if (!isSuicide)
                        {
                            moves.Add(placeAction);
                        }
                    }
                }

                // 2. АТАКА ПЕШКОЙ (AttackPawn)
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

        return moves;
    }

    private void MarkWideActiveZone(bool[,] zone, int cx, int cy, int w, int h)
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
    public FastBoardState CaptureFastState(Team myTeam)
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
            Mana = myTeam == GameController.Instance.player.GetLocalPlayerTeam() ?
            GameController.Instance.player.GetCurrentMana() : GameController.Instance.enemy.GetCurrentMana(),
            Bones = myTeam == GameController.Instance.player.GetLocalPlayerTeam() ?
            GameController.Instance.player.GetCurrentGraveTokens() : GameController.Instance.enemy.GetCurrentGraveTokens(),
            MaxMana = GameController.Instance.matchSettings.maxMana,
            MaxBones = GameController.Instance.matchSettings.maxGraveTokens,

            Board = new CellOwner[w, h],
            Stats = new FastTileStats[w, h],
            Hand = new List<Card>()
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
                    CanPutOnTile = st.canPutOnTile,
                    BanPutDuration = st.banPutDuration,
                    CanAttackTile = st.canAttackTile,
                    BanAttackTileDuration = st.banAttackDuration,
                    CanLeaveFromTile = st.canLeaveFromTile,
                    BanLeaveFromTileDuration = st.banLeaveDuration
                };
            }
        }

        List<Card> cardsInHand = new();
        if (myTeam != GameController.Instance.player.GetLocalPlayerTeam())
        {
            foreach (Card card in EnemyCardHand.Instance.cardsInHand.Values)
                cardsInHand.Add(card);
        }
        else
        {
            foreach (Card card in PlayerCardHand.Instance.CardsInHand)
                cardsInHand.Add(card);
        }

        state.Hand = cardsInHand;

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

            case ActionType.PlayCard:
                if (move.CardToPlay is ICardAI aiCard)
                {
                    newState = aiCard.ApplyToState(newState, move.CardTargets, actingTeam);
                    newState.Hand.Remove(move.CardToPlay);
                }
                break;

            default:
                return null;
        }
        DecrementTileBuffs(newState);
        return newState;
    }
    private void DecrementTileBuffs(FastBoardState state)
    {
        for (int x = 0; x < state.Width; x++)
        {
            for (int y = 0; y < state.Height; y++)
            {
                if (!state.Stats[x, y].CanAttackTile)
                {
                    state.Stats[x, y].BanAttackTileDuration--;

                    if (state.Stats[x, y].BanAttackTileDuration <= 0)
                    {
                        state.Stats[x, y].CanAttackTile = true;
                        state.Stats[x, y].BanAttackTileDuration = 0;
                    }
                }

                if (!state.Stats[x, y].CanPutOnTile)
                {
                    state.Stats[x, y].BanPutDuration--;

                    if (state.Stats[x, y].BanPutDuration <= 0)
                    {
                        state.Stats[x, y].CanPutOnTile = true;
                        state.Stats[x, y].BanPutDuration = 0;
                    }
                }

                if (!state.Stats[x, y].CanLeaveFromTile)
                {
                    state.Stats[x, y].BanLeaveFromTileDuration--;

                    if (state.Stats[x, y].BanLeaveFromTileDuration <= 0)
                    {
                        state.Stats[x, y].CanLeaveFromTile = true;
                        state.Stats[x, y].BanLeaveFromTileDuration = 0;
                    }
                }

            }
        }
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

    private readonly HashSet<int> previousBoardHashes = new();
    private readonly Queue<int> boardHashHistory = new();
    [SerializeField] int maxHistorySizeMoves = 8;

    public void RecordBoardState(FastBoardState state)
    {
        int hash = GetBoardHash(state);
        if (!previousBoardHashes.Add(hash)) return;

        boardHashHistory.Enqueue(hash);
        while (boardHashHistory.Count > maxHistorySizeMoves)
        {
            int old = boardHashHistory.Dequeue();
            previousBoardHashes.Remove(old);
        }
    }

    private int GetBoardHash(FastBoardState s)
    {
        unchecked
        {
            int hash = 17;
            int w = s.Board.GetLength(0), h = s.Board.GetLength(1);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    hash = hash * 31 + (int)s.Board[x, y];
                    if (s.Stats != null)
                    {
                        var st = s.Stats[x, y];
                        hash = hash * 31 + (st.CanAttackTile ? 1 : 0);
                        hash = hash * 31 + (st.CanPutOnTile ? 1 : 0);
                        hash = hash * 31 + (st.CanLeaveFromTile ? 1 : 0);
                    }
                }
            return hash;
        }
    }
}