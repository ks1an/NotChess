using System.Collections.Generic;
using UnityEngine;

public sealed class ForbiddenShieldCard : Card
{
    [SerializeField] DefendClass defendClassForTile;
    [SerializeField] int durationEffectOfBanPut;
    [field: SerializeField] InstanceParticle_SO_VB visualEffectOnTile;


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

        List<IBuff> buffsOnTile = new()
        {
            new BanPut_TileBuff(false, true),
            new DefendClass_TileBuff(false, true, defendClassForTile)
        };
        var buffsOnTilePocket = new PocketBuff(false, true, buffsOnTile);
        var tileBuff = new TemporaryBuff(tile.Stats, buffsOnTilePocket, durationEffectOfBanPut);
        new VisualParticleBuffBehaviour(tileBuff, visualEffectOnTile, tile.tileCenter);
        tile.Stats.AddBuff(tileBuff);


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
}
