using System.Collections.Generic;
using UnityEngine;

//Zeus and Perun cast their hatred on the enemy
public sealed class LightingBoltCard : Card
{
    [Header("On used")]
    [SerializeField] GameObject OnUsedVFX;
    [field: SerializeField] InstanceParticle_SO_VB buffOnTile;
    [SerializeField] int durationEffect;
    [SerializeField] float chromDurationIfAttackSuccess, chromDurationIfAttackNOTSuccess;

    [Header("Audio")]
    [SerializeField] float volume = 1f;
    [SerializeField] float minPitch = 1f, maxPitch = 1f;
    [SerializeField] AudioClip[] audioClipsOnUsed;

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

        EnvironmentManager.Instance.DoBoardFlickeringLight(6);
        EnvironmentManager.Instance.SetActiveChromeAbb(true);

        Tile tile = Board.Instance.tilesController.tiles[moves[0].x, moves[0].y];
        Instantiate(OnUsedVFX, tile.tileCenter, Quaternion.identity);
        bool attackWasSuccessful = tile.TryGetAroundDefend((int)AttackClass);
        if (attackWasSuccessful)
        {
            GameController.Instance.secTimer.StartTimer(chromDurationIfAttackSuccess, out SecondTimerSubscriber sub,
    () => EnvironmentManager.Instance.SetActiveChromeAbb(false));
            //ScorchTemporaryBuff
            var logicEffectOnTile = new Scorch_TileBuff(false, false);
            new VisualParticleBuffBehaviour(logicEffectOnTile, buffOnTile, tile.tileCenter);
            tile.Stats.AddBuff(new TemporaryBuff(tile.Stats, logicEffectOnTile, durationEffect));
        }
        else
        {
            GameController.Instance.secTimer.StartTimer(chromDurationIfAttackNOTSuccess, out SecondTimerSubscriber sub,
() => EnvironmentManager.Instance.SetActiveChromeAbb(false));
        }

        if (audioClipsOnUsed.Length > 0)
            GameSound.Instance.PlayRandomSound(audioClipsOnUsed, volume, minPitch, maxPitch);


        if (!isSynced)
        {
            if (attackWasSuccessful)
            {
                GameController.Instance.states.move.TryDestroyUnit(moves[0].x, moves[0].y,
                    false, teamWhoHave);
            }
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
        if (Board.Instance.tilesController.tiles[targetX, targetY] != null)
            return true;
        return false;
    }
    #endregion
}
