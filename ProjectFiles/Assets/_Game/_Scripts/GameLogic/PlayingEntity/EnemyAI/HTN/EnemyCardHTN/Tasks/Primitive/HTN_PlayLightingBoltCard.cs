using System.Collections.Generic;
using UnityEngine;

public class HTN_PlayLightingBoltCard : HTN_PrimitiveTask
{
    LightingBoltCard cardPrefab;

    protected override Dictionary<string, object> PreConditions()
    {
        return new Dictionary<string, object>
        {
            {TargetIsEnemy_HTN_WorldKey.Key, true },
        };
    }

    public override TaskResult Execute(float delta, object actor, HTNWorldState worldState)
    {
        try
        {
            LightingBoltCard card;
            if ((PlayingEntity)worldState.GetValue(CurrentPlayingEntity_HTN_WorldKey.Key) == GameController.Instance.player)
            {
                card = (LightingBoltCard)PlayerCardHand.Instance.GetCardFromHand(cardPrefab);
            }
            else
            {
                card = (LightingBoltCard)EnemyDeck.Instance.hand.GetCardFromHand(cardPrefab);
            }
            if (card == null)
            {
                Debug.Log("LightingBoltIsDontFinded. IsPlayerBot?" + ((PlayingEntity)worldState.GetValue(CurrentPlayingEntity_HTN_WorldKey.Key) == GameController.Instance.player));
                return TaskResult.FAILURE;
            }

            Vector2Int target = (Vector2Int)worldState.GetValue(TargetTile_HTN_WorldKey.Key);
            card.Init(((PlayingEntity)worldState.GetValue(CurrentPlayingEntity_HTN_WorldKey.Key)).GetLocalPlayerTeam());
            card.UseCard(new List<Vector2Int> { target });

            return TaskResult.SUCCESS;
        }
        catch
        {
            return TaskResult.FAILURE;
        }
    }

    protected override Dictionary<string, object> Effects(HTNWorldState worldState)
    {
        ((PlayingEntity)worldState.GetValue(CurrentPlayingEntity_HTN_WorldKey.Key)).DeacreaseMana(cardPrefab.ManaCost);

        return new Dictionary<string, object>
        {

        };
    }

    public override bool IsAvailable(HTNWorldState worldState)
    {
        if ((int)worldState.GetValue(HasLightingBoltCard_HTN_WorldKey.Key) < 1)
            return false;

        cardPrefab = (LightingBoltCard)GameController.Instance.globalCards.GetCardPrefabByType(typeof(LightingBoltCard));
        if (cardPrefab == null || cardPrefab.ManaCost > (int)worldState.GetValue(CurrentMana_HTN_WorldKey.Key))
            return false;

        return base.IsAvailable(worldState);
    }
}
