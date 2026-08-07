using System.Collections.Generic;
using UnityEngine;

public class KnightCard : Card
{
    [field: SerializeField] PieceView kingPrefab;
    public override void Init(Team teamWhoHave)
    {
        base.Init(teamWhoHave);
    }

    protected override void OnCursorDown()
    {
        if (GameController.Instance.player.GetCurrentGraveTokens() >= GraveTokensCost)
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
        if (GameController.Instance.player.GetCurrentGraveTokens() >= GraveTokensCost && availableMoves.Count > 0)
        {
            GameController.Instance.player.DeacreaseGraveTokens(GraveTokensCost);
            UseCard(availableMoves);
        }
    }

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {
        if (!cardInited) Debug.LogError("Card NOT inited but used " + originalCardName);

        if (!isSynced)
        {
            GameController.Instance.states.move.TryDestroyAndCreateUnit(moves[0].x, moves[0].y, false, teamWhoHave, kingPrefab, false);

            GameController.Instance.states.move.UseCard(GetID(), moves, teamWhoHave);
            if (teamWhoHave == GameController.Instance.player.GetLocalPlayerTeam())
                PlayerDeck.Instance.DestroyCardInHand(this);
            else
            {
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
        if (Board.Instance.tilesController.tiles[targetX, targetY] != null && Board.Instance.piecesController.pieces[targetX, targetY] != null
            && Board.Instance.piecesController.pieces[targetX, targetY].team == GameController.Instance.player.GetLocalPlayerTeam()
            && Board.Instance.piecesController.pieces[targetX, targetY].GetType() == typeof(PawnData))
            return true;
        return false;
    }
    #endregion
}
