using System.Collections.Generic;
using UnityEngine;
using static EnemyAIPlanner;

public sealed class CastleWallsCard : Card, ICardAI
{
    [field: SerializeField] InstanceGameobject_SO_VB visualEffectOnTile;
    [SerializeField] int duration;

    public override void Init(Team teamWhoHave)
    {
        base.Init(teamWhoHave);
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

        Tile tile = Board.Instance.tilesController.tiles[moves[0].x, moves[0].y];
        var buff = new BanAttack_TileBuff(false, true);
        new VisualGameobjectBuffBehaviour(buff, visualEffectOnTile, tile.tileCenter);
        var tBuff = new TemporaryBuff(tile.Stats, buff, duration);
        tile.Stats.AddBuff(tBuff);

        if (!isSynced)
        {
            GameController.Instance.states.move.UseCard(GetID(), moves, teamWhoHave);

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

        if (IsAvailableMove(hoverX, hoverY))
            availables.Add(new Vector2Int(hoverX, hoverY));

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
        int w = state.Board.GetLength(0);
        int h = state.Board.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                CellOwner owner = state.Board[x, y];

  
                if (owner == myTeam)
                {
                    if (IsCellAttackedByFast(state, x, y, opp))
                    {
                        result.Add(new List<Vector2Int> { new(x, y) });
                    }
                }
                else if (owner == CellOwner.None)
                {
                   if (!state.Stats[x, y].CanPutOnTile)
                        continue;

                    if (IsCellAttackedByFast(state, x, y, opp) && IsCellAttackedByFast(state, x, y, myTeam))
                    {
                        result.Add(new List<Vector2Int> { new(x, y) });
                    }
                }
            }
        }

        return result;
    }

    public FastBoardState ApplyToState(FastBoardState state, List<Vector2Int> targets, CellOwner myTeam)
    {
        if (targets == null || targets.Count == 0) return state;

        Vector2Int targetCell = targets[0];

        if (targetCell.x >= 0 && targetCell.x < state.Width && targetCell.y >= 0 && targetCell.y < state.Height)
        {
            state.Stats[targetCell.x, targetCell.y].CanAttackTile = false;
            state.Stats[targetCell.x, targetCell.y].BanAttackTileDuration += duration;
        }
        state.Mana -= ManaCost;
        state.Bones -= GraveTokensCost;
        return state;
    }

    private bool IsCellAttackedByFast(FastBoardState state, int x, int y, CellOwner attacker)
    {
        int[] dx = { -1, -1, 1, 1 };
        int[] dy = { -1, 1, -1, 1 };

        for (int i = 0; i < dx.Length; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx >= 0 && nx < state.Width && ny >= 0 && ny < state.Height)
            {
                if (state.Board[nx, ny] == attacker)
                    return true;
            }
        }

        return false;
    }

    #endregion
}
