using System.Collections.Generic;
using UnityEngine;

//Zeus and Perun cast their hatred on the enemy
public sealed class LightingBoltCard : Card
{
    #region Inspector Variable
    [Header("On used")]
    [SerializeField] GameObject OnUsedVFX;
    [field: SerializeField] InstanceParticle_SO_VB buffOnTile;
    [SerializeField] int durationEffect;

    [Header("Audio")]
    [SerializeField] float volume = 1f;
    [SerializeField] float minPitch = 1f, maxPitch = 1f;
    [SerializeField] AudioClip[] audioClipsOnUsed;
    #endregion

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
                    Hand.SetCurrentSelectCard(this);
                    CardUI.SetBorderColor(BoarderColorOnDrag);
                }
            }
            else
            {
                Hand.SetCurrentSelectCard(this);
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
            Hand.ResetCurrentSelectCard(null);
        }
        else if (PlayerDeck.Instance.hand.CurrentSelectCard == this)
        {
            Hand.ResetCurrentSelectCard(this);
        }

        CardUI.SetBorderColor(ColorBorder);
    }
    #endregion

    public override void UseCard(List<Vector2Int> moves, bool isSynced = false)
    {
        EnvironmentManager.Instance.DoMediumBoardFlickeringLight();
        Tile tile = Board.Instance.tilesController.tiles[moves[0].x, moves[0].y];
        Instantiate(OnUsedVFX, tile.tileCenter, Quaternion.identity);
        bool attackWasSuccessful = tile.TryGetAroundDefend((int)AttackClass);
        if (attackWasSuccessful)
        {
            //ScorchTemporaryBuff
            var logicEffectOnTile = new Scorch_TileBuff(false, false);
            new VisualParticleBuffBehaviour(logicEffectOnTile, buffOnTile, tile.tileCenter);
            tile.tileBuffAndStatsComponent.AddBuff(new TemporaryBuff(tile.tileBuffAndStatsComponent, logicEffectOnTile, durationEffect));
        }

        if (audioClipsOnUsed.Length > 0)
            GameSound.Instance.PlaySound(audioClipsOnUsed, volume, minPitch, maxPitch);


        if (!isSynced)
        {
            if (attackWasSuccessful)
            {
                GameController.Instance.states.TryDestroyUnit(moves[0].x, moves[0].y, 
                    false, GameController.Instance.player.GetLocalPlayerTeam());
            }
            GameController.Instance.states.UseCard(ID, moves);
            PlayerDeck.Instance.DestroyCardInHand(this);
        }
        else
        {
            Destroy(gameObject);
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
        if (Board.Instance.tilesController.tiles[targetX, targetY] != null)
            return true;
        return false;
    }
    #endregion
}
