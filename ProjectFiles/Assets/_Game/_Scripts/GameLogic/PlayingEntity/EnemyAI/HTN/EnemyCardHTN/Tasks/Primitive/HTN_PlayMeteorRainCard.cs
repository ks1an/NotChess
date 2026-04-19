using System.Collections.Generic;
using UnityEngine;

public class HTN_PlayMeteorRainCard : HTN_PrimitiveTask
{
    MeteorRainCard cardPrefab;

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
            MeteorRainCard card;
            if ((PlayingEntity)worldState.GetValue(CurrentPlayingEntity_HTN_WorldKey.Key) == GameController.Instance.player)
            {
                card = (MeteorRainCard)PlayerCardHand.Instance.GetCardFromHand(cardPrefab);
            }
            else
            {
                card = (MeteorRainCard)EnemyDeck.Instance.hand.GetCardFromHand(cardPrefab);
            }
            if (card == null)
            {
                Debug.Log("MeteorRainCard Is Dont Finded. IsPlayerBot?" + ((PlayingEntity)worldState.GetValue(CurrentPlayingEntity_HTN_WorldKey.Key) == GameController.Instance.player));
                return TaskResult.FAILURE;
            }

            Vector2Int target = (Vector2Int)worldState.GetValue(TargetTile_HTN_WorldKey.Key);
            card.Init(((PlayingEntity)worldState.GetValue(CurrentPlayingEntity_HTN_WorldKey.Key)).GetLocalPlayerTeam());
            card.UseCard(card.GetAvailableMoves(GameController.Instance.matchSettings.tileCountX,
                GameController.Instance.matchSettings.tileCountY, target.x, target.y));

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
        if ((int)worldState.GetValue(HasMeteorRainCard_HTN_WorldKey.Key) < 1)
            return false;

        cardPrefab = (MeteorRainCard)GameController.Instance.globalCards.GetCardPrefabByType(typeof(MeteorRainCard));
        if (cardPrefab == null || cardPrefab.ManaCost > (int)worldState.GetValue(CurrentMana_HTN_WorldKey.Key))
            return false;

        return base.IsAvailable(worldState);
    }
}
