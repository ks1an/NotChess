using System.Collections.Generic;
using UnityEngine;
using static EnemyAIPlanner;

public sealed class RockCard : Card, ICardAI
{
    [field: SerializeField] InstanceGameobject_SO_VB visualEffectOnTile;
    [SerializeField] int duration;

    public override void Init(Team teamWhoHave)
    {
        base.Init(teamWhoHave);
    }

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

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {
        if (!cardInited) Debug.LogError("Card NOT inited but used " + originalCardName);

        Tile tile = Board.Instance.tilesController.tiles[moves[0].x, moves[0].y];
        var buff = new BanPut_TileBuff(false, true);
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
        {
            KillCard();
        }
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
        if (Board.Instance.tilesController.tiles[targetX, targetY] != null && Board.Instance.piecesController.pieces[targetX, targetY] == null)
            return true;
        return false;
    }
    #endregion

    #region AI
    public List<List<Vector2Int>> GetTargets(FastBoardState state, CellOwner myTeam)
    {
        var result = new List<List<Vector2Int>>();
        if (state?.Board == null || state.Stats == null)
            return result;

        int w = state.Board.GetLength(0);
        int h = state.Board.GetLength(1);

        if (state.Stats.GetLength(0) != w || state.Stats.GetLength(1) != h)
            return result;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (state.Board[x, y] != CellOwner.None) continue;
                if (!state.Stats[x, y].CanPutOnTile) continue;

                result.Add(new List<Vector2Int> { new(x, y) });
            }
        }

        return result;
    }

    public FastBoardState ApplyToState(FastBoardState state, List<Vector2Int> targets, CellOwner myTeam)
    {
        if (targets == null || targets.Count == 0) return state;

        Vector2Int targetCell = targets[0];
        int w = state.Board.GetLength(0);
        int h = state.Board.GetLength(1);

        if (targetCell.x >= 0 && targetCell.x < w &&
            targetCell.y >= 0 && targetCell.y < h)
        {
            state.Stats[targetCell.x, targetCell.y].CanPutOnTile = false;
            state.Stats[targetCell.x, targetCell.y].BanPutDuration += duration;
        }
        state.Mana -= ManaCost;
        state.Bones -= GraveTokensCost;
        return state;
    }
    #endregion
}
