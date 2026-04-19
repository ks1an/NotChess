using System.Collections.Generic;
using UnityEngine;

public class HTN_PlayDistantRelativeCard : HTN_PrimitiveTask
{
    DistantRelativeCard cardPrefab;

    protected override Dictionary<string, object> PreConditions()
    {
        return new Dictionary<string, object>
        {

        };
    }

    public override TaskResult Execute(float delta, object actor, HTNWorldState worldState)
    {
        try
        {
            DistantRelativeCard card;
            if ((PlayingEntity)worldState.GetValue(CurrentPlayingEntity_HTN_WorldKey.Key) == GameController.Instance.player)
            {
                card = (DistantRelativeCard)PlayerCardHand.Instance.GetCardFromHand(cardPrefab);
            }
            else
            {
                card = (DistantRelativeCard)EnemyDeck.Instance.hand.GetCardFromHand(cardPrefab);
            }
            if (card == null)
            {
                Debug.Log("DistantRelativeIsDontFinded. IsPlayerBot?" + ((PlayingEntity)worldState.GetValue(CurrentPlayingEntity_HTN_WorldKey.Key) == GameController.Instance.player));
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
        if ((int)worldState.GetValue(HasDistantRelativeCard_HTN_WorldKey.Key) < 1)
            return false;

        cardPrefab = (DistantRelativeCard)GameController.Instance.globalCards.GetCardPrefabByType(typeof(DistantRelativeCard));
        if (cardPrefab == null || cardPrefab.ManaCost > (int)worldState.GetValue(CurrentMana_HTN_WorldKey.Key))
            return false;

        return base.IsAvailable(worldState);
    }
}
