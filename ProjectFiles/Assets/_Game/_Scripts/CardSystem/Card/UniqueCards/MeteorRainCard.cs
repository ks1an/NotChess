using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public sealed class MeteorRainCard : Card
{
    //The dinosaurs won't like this. Oh, I mean the enemies.
    [SerializeField] int countShells;
    [SerializeField] int radiousWidthRangeAttack;
    [SerializeField] int radiousHeightRangeAttack;

    [SerializeField] GameObject myPrefabVFX;
    [field: SerializeField] ScriptableEffector effectOfTile;

    List<Vector2Int> alreadyAttacked;

    public override void Init()
    {
        base.Init();
    }

    #region OnDrag
    void OnMouseDown()
    {
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost)
        {
            if (IsUseOnlyMyTurn)
            {
                if (GameController.Instance.player.IsMyTurnOrNot())
                {
                    Deck.Instance.playerHand.SetCurrentSelectCard(this);
                    CardUI.SetBorderColor(BoarderColorOnDrag);
                }
            }
            else
            {
                Deck.Instance.playerHand.SetCurrentSelectCard(this);
                CardUI.SetBorderColor(BoarderColorOnDrag);
            }

        }

    }

    void OnMouseUp()
    {
        if (GameController.Instance.player.GetCurrentMana() >= ManaCost && availableMoves.Count > 0)
        {
            GameController.Instance.player.DeacreaseMana(ManaCost);
            UseCard(availableMoves);
            Deck.Instance.playerHand.ResetCurrentSelectCard(null);
        }
        else if (Deck.Instance.playerHand.CurrentSelectCard == this)
        {
            Deck.Instance.playerHand.ResetCurrentSelectCard(this);
        }

        CardUI.SetBorderColor(ColorBorder);
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
                        GameController.Instance.states.TryDestroyUnit(target.x, target.y, false);
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
            Instantiate(myPrefabVFX, Board.Instance.tilesController.GetTileCenter(moves[i].x, moves[i].y), Quaternion.identity)
                .GetComponent<VisualEffect>();

            Board.Instance.tilesController.tiles[moves[i].x, moves[i].y].
                AddEffect(effectOfTile.InitializeEffect(null, moves[i].x, moves[i].y));
        }

        if (isSynced)
            Destroy(gameObject);
        else
            Deck.Instance.DestroyCard(this);
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


        #region FindAvailabeRange

        #region GetFlawTiles
        int leftXFlaw = 0, rightXFlaw = 0;
        if (hoverX - radiousWidthRangeAttack < 0)
            leftXFlaw = (hoverX - radiousWidthRangeAttack) * -1;

        if (hoverX + radiousWidthRangeAttack > maxX)
            rightXFlaw = (maxX - hoverX + radiousWidthRangeAttack) * -1;

        int upYFlaw = 0, downYFlaw = 0;
        if (hoverY - radiousHeightRangeAttack < 0)
            downYFlaw = (hoverY - radiousHeightRangeAttack) * -1;

        if (hoverY + radiousHeightRangeAttack > maxY)
            upYFlaw = (maxY - hoverY + radiousHeightRangeAttack) * -1;
        #endregion

        if (hoverX - radiousWidthRangeAttack - rightXFlaw >= 0 && hoverX + radiousWidthRangeAttack + leftXFlaw <= maxX
            && hoverY - radiousHeightRangeAttack - upYFlaw >= 0 && hoverY + radiousHeightRangeAttack + downYFlaw <= maxY)
        {
            for (int x = hoverX - radiousWidthRangeAttack - rightXFlaw; x < hoverX + radiousWidthRangeAttack + leftXFlaw; x++)
            {
                for (int y = hoverY - radiousHeightRangeAttack - upYFlaw; y < hoverY + radiousHeightRangeAttack + downYFlaw; y++)
                {
                    if (y < 0)
                        continue;
                    if (y >= maxY) break;

                    if (IsAvailableMove(x, y))
                        availables.Add(new Vector2Int(x, y));
                }
            }
        }
        #endregion

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
