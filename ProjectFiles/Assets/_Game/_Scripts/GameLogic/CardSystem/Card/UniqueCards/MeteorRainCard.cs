using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static EnemyAIPlanner;

//The dinosaurs won't like this. Oh, I mean the enemies.
public sealed class MeteorRainCard : Card, ICardAI
{
    [SerializeField] int countShells;
    [SerializeField] int radiousWidthRangeAttack;
    [SerializeField] int radiousHeightRangeAttack;

    [SerializeField] GameObject spawnOnUsed_Base, spawnOnUsed_Mobile;
    [field: SerializeField] InstanceParticle_SO_VB buffOnTile;
    [SerializeField] int durationEffect;

    List<Vector2Int> alreadyAttacked;

    public override void Init(Team teamWhoHave)
    {
        base.Init(teamWhoHave);

        if (Application.isMobilePlatform)
        {
            spawnOnUsed_Base = spawnOnUsed_Mobile;
        }
    }

    #region OnDrag
    protected override void OnCursorDown()
    {
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost)
        {
            if (GameController.Instance.player.IsMyTurnOrNot())
            {
                Hand.TrySetCurrentSelectCard(this);
                CardUI.MouseEnterFromCard(gameObject);
            }
        }

    }

    protected override void DoActionOnMouseUp()
    {
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost && availableMoves.Count > 0)
        {
            GameController.Instance.player.DeacreaseMana(ManaCost);
            UseCard(availableMoves);
        }
    }
    #endregion

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {
        if (!cardInited) Debug.LogError("Card NOT inited but used " + originalCardName);

        if (!isSynced)
        {
            alreadyAttacked = new();
            MathOperations.GetInstance().ShuffleList(moves);
            for (int i = 0; i < countShells; i++)
            {
                foreach (Vector2Int target in moves)
                    if (!alreadyAttacked.Contains(target))
                    {
                        alreadyAttacked.Add(target);
                        break;
                    }
            }
            GameController.Instance.states.move.UseCard(GetID(), alreadyAttacked, teamWhoHave);
            Board.Instance.tilesController.RemoveHighlightTiles(moves);
            moves = alreadyAttacked;
        }

        for (int i = 0; i < moves.Count; i++)
        {
            Instantiate(spawnOnUsed_Base, Board.Instance.tilesController.GetTileCenter(moves[i].x, moves[i].y), Quaternion.identity)
                .GetComponent<VisualEffect>();

            if (Board.Instance.tilesController.tiles[moves[i].x, moves[i].y].TryGetAroundDefend((int)AttackClass))
            {
                if (!isSynced)
                {
                    GameController.Instance.states.move.TryDestroyUnit(moves[i].x, moves[i].y,
                        false, teamWhoHave);
                }
                Tile tile = Board.Instance.tilesController.tiles[moves[i].x, moves[i].y];
                //ScorchTemporaryBuff
                var logicEffectOnTile = new Scorch_TileBuff(false, false);
                new VisualParticleBuffBehaviour(logicEffectOnTile, buffOnTile, tile.tileCenter);
                tile.Stats.AddBuff(new TemporaryBuff(tile.Stats, logicEffectOnTile, durationEffect));
            }
        }

        if (!isSynced)
        {
            if (teamWhoHave == GameController.Instance.player.GetLocalPlayerTeam())
                PlayerDeck.Instance.DestroyCardInHand(this);
            else
            {
                //TODO: Effectively removes a random card of the type. Creates ambiguity for the player. Needs to be changed.
                EnemyCardHand hand = EnemyDeck.Instance.hand;
                EnemyDeck.Instance.DestroyCardInHand(hand.CardGameobjectsInHand[hand.GetIndexOfCardInHandByType(this)], true);
            }
        }
        else
            KillCard();
    }

    #region AvailableMoves
    public override List<Vector2Int> GetAvailableMoves(int maxX, int maxY, int hoverX, int hoverY)
    {
        List<Vector2Int> availables = new();
        if (hoverX < 0 || hoverY < 0)
        {
            availableMoves = availables;
            return availables;
        }

        int leftXFlaw = Math.Max(0, radiousWidthRangeAttack - hoverX);
        int rightXFlaw = Math.Max(0, hoverX + radiousWidthRangeAttack - maxX);
        int downYFlaw = Math.Max(0, radiousHeightRangeAttack - hoverY);
        int upYFlaw = Math.Max(0, hoverY + radiousHeightRangeAttack - maxY);

        int adjustedX = hoverX + leftXFlaw - rightXFlaw;
        int adjustedY = hoverY + downYFlaw - upYFlaw;

        //Do we fit on the board?
        if (adjustedX - radiousWidthRangeAttack >= 0 &&
            adjustedX + radiousWidthRangeAttack <= maxX &&
            adjustedY - radiousHeightRangeAttack >= 0 &&
            adjustedY + radiousHeightRangeAttack <= maxY)
        {
            for (int x = adjustedX - radiousWidthRangeAttack; x < adjustedX + radiousWidthRangeAttack; x++)
                for (int y = adjustedY - radiousHeightRangeAttack; y < adjustedY + radiousHeightRangeAttack; y++)
                {
                    if (x >= 0 && x < maxX && y >= 0 && y < maxY)
                        if (IsAvailableMove(x, y))
                            availables.Add(new Vector2Int(x, y));
                }
        }

        availableMoves = availables;
        return availables;
    }

    public override bool IsAvailableMove(int targetX, int targetY)
    {
        if (Board.Instance.tilesController.tiles[targetX, targetY] != null)
            return true;
        return false;
    }
    #endregion

    #region ICardAI Implementation

    public List<List<Vector2Int>> GetTargets(FastBoardState state, CellOwner myTeam)
    {
        var result = new List<List<Vector2Int>>();
        CellOwner opp = myTeam == CellOwner.Zero ? CellOwner.Cross : CellOwner.Zero;

        for (int cx = 0; cx < state.Width; cx++)
        {
            for (int cy = 0; cy < state.Height; cy++)
            {
                var areaMoves = GetAvailableMovesFast(state.Width, state.Height, cx, cy);
                if (areaMoves == null || areaMoves.Count == 0) continue;

                int enemyCount = 0;
                int friendCount = 0;

                foreach (var cell in areaMoves)
                {
                    CellOwner owner = state.Board[cell.x, cell.y];
                    if (owner == opp) enemyCount++;
                    else if (owner == myTeam) friendCount++;
                }

                if (enemyCount >= 2 && friendCount < enemyCount)
                {
                    result.Add(areaMoves);
                }
            }
        }

        return result;
    }

    public FastBoardState ApplyToState(FastBoardState state, List<Vector2Int> targets, CellOwner myTeam)
    {
        if (targets == null || targets.Count == 0) return state;

        CellOwner opp = myTeam == CellOwner.Zero ? CellOwner.Cross : CellOwner.Zero;
        if (targets.Count == 0) return state;

        float hitProbability = 1.0f - Mathf.Pow((float)(targets.Count - 1) / targets.Count, countShells);

        var enemyCells = new List<Vector2Int>();
        foreach (var cell in targets)
        {
            if (state.Board[cell.x, cell.y] == opp)
            {
                enemyCells.Add(cell);
            }
        }

        int expectedKills = Mathf.RoundToInt(enemyCells.Count * hitProbability);
        for (int i = 0; i < expectedKills && i < enemyCells.Count; i++)
        {
            Vector2Int target = enemyCells[i];
            state.Board[target.x, target.y] = CellOwner.None;
        }

        state.Mana -= ManaCost;
        state.Bones -= GraveTokensCost;

        return state;
    }

    // Чистая генерация области для симулятора (без вызова Board.Instance)
    private List<Vector2Int> GetAvailableMovesFast(int maxX, int maxY, int hoverX, int hoverY)
    {
        List<Vector2Int> availables = new();
        if (hoverX < 0 || hoverY < 0) return availables;

        int leftXFlaw = Math.Max(0, radiousWidthRangeAttack - hoverX);
        int rightXFlaw = Math.Max(0, hoverX + radiousWidthRangeAttack - maxX);
        int downYFlaw = Math.Max(0, radiousHeightRangeAttack - hoverY);
        int upYFlaw = Math.Max(0, hoverY + radiousHeightRangeAttack - maxY);

        int adjustedX = hoverX + leftXFlaw - rightXFlaw;
        int adjustedY = hoverY + downYFlaw - upYFlaw;

        if (adjustedX - radiousWidthRangeAttack >= 0 &&
            adjustedX + radiousWidthRangeAttack <= maxX &&
            adjustedY - radiousHeightRangeAttack >= 0 &&
            adjustedY + radiousHeightRangeAttack <= maxY)
        {
            for (int x = adjustedX - radiousWidthRangeAttack; x < adjustedX + radiousWidthRangeAttack; x++)
            {
                for (int y = adjustedY - radiousHeightRangeAttack; y < adjustedY + radiousHeightRangeAttack; y++)
                {
                    if (x >= 0 && x < maxX && y >= 0 && y < maxY)
                        availables.Add(new Vector2Int(x, y));
                }
            }
        }

        return availables;
    }

    #endregion
}
