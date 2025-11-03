using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

//The dinosaurs won't like this. Oh, I mean the enemies.
public sealed class MeteorRainCard : Card
{
    [SerializeField] int countShells;
    [SerializeField] int radiousWidthRangeAttack;
    [SerializeField] int radiousHeightRangeAttack;

    [SerializeField] GameObject spawnOnUsed;
    [field: SerializeField] InstanceParticle_SO_VB buffOnTile;
    [SerializeField] int durationEffect;

    List<Vector2Int> alreadyAttacked;

    public override void Init()
    {
        base.Init();
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

    protected override void OnCursorUp()
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
            GameController.Instance.states.UseCard(ID, alreadyAttacked);
            Board.Instance.tilesController.RemoveHighlightTiles(moves);
            moves = alreadyAttacked;
        }

        for (int i = 0; i < moves.Count; i++)
        {
            Instantiate(spawnOnUsed, Board.Instance.tilesController.GetTileCenter(moves[i].x, moves[i].y), Quaternion.identity)
                .GetComponent<VisualEffect>();

            if (Board.Instance.tilesController.tiles[moves[i].x, moves[i].y].TryGetAroundDefend((int)AttackClass))
            {
                if (!isSynced)
                {
                    GameController.Instance.states.TryDestroyUnit(moves[i].x, moves[i].y,
                        false, GameController.Instance.player.GetLocalPlayerTeam());
                }
                Tile tile = Board.Instance.tilesController.tiles[moves[i].x, moves[i].y];
                //ScorchTemporaryBuff
                var logicEffectOnTile = new Scorch_TileBuff(false, false);
                new VisualParticleBuffBehaviour(logicEffectOnTile, buffOnTile, tile.tileCenter);
                tile.Stats.AddBuff(new TemporaryBuff(tile.Stats, logicEffectOnTile, durationEffect));
            }
        }

        if (!isSynced)
            PlayerDeck.Instance.DestroyCardInHand(this);
        else
            Destroy(gameObject);
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
}
