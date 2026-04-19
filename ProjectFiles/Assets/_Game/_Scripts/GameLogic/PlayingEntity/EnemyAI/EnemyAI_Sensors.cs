public class EnemyAI_Sensors
{
    public HTNWorldState worldState;
    public Team myTeam;
    PlayingEntity myEntity;

    public void Init(PlayingEntity entity)
    {
        worldState = new();
        myEntity = entity;
        worldState.SetValue(CurrentPlayingEntity_HTN_WorldKey.Key, myEntity);
        myEntity.OnTeamChanged += SetMyTeam;
        myEntity.OnCurrentManaChaged += SetCurrentMana;

        if (myEntity.GetType() == typeof(Player))
        {
            PlayerCardHand.Instance.OnCardAddedInHand += OnCardAdded;
            PlayerCardHand.Instance.OnAllCardsRemovedInHand += OnAllCardsRemoved;
            PlayerCardHand.Instance.OnCardRemovedInHand += OnCardRemoved;
        }
        else
        {
            EnemyDeck.Instance.hand.OnCardAddedInHand += OnCardAdded;
            EnemyDeck.Instance.hand.OnAllCardsRemovedInHand += OnAllCardsRemoved;
            EnemyDeck.Instance.hand.OnCardRemovedInHand += OnCardRemoved;
        }
    }

    public HTNWorldState GetWorldState() { return worldState; }
    public void SetNewWorldState(HTNWorldState newState) { worldState = newState; }

    void SetMyTeam(Team team) => myTeam = team;

    void SetCurrentMana(int currentMana) => worldState.SetValue(CurrentMana_HTN_WorldKey.Key, currentMana);

    //CARDS
    private void OnCardAdded(Card card)
    {
        if (card.GetType() == typeof(LightingBoltCard))
        {
            worldState.SetValue(HasLightingBoltCard_HTN_WorldKey.Key, (int)worldState.GetValue(HasLightingBoltCard_HTN_WorldKey.Key) + 1);
        }
        else if(card.GetType() == typeof(DistantRelativeCard))
        {
            worldState.SetValue(HasDistantRelativeCard_HTN_WorldKey.Key, (int)worldState.GetValue(HasDistantRelativeCard_HTN_WorldKey.Key) + 1);
        }
        else if (card.GetType() == typeof(MeteorRainCard))
        {
            worldState.SetValue(HasMeteorRainCard_HTN_WorldKey.Key, (int)worldState.GetValue(HasMeteorRainCard_HTN_WorldKey.Key) + 1);
        }
    }

    private void OnCardRemoved(Card card)
    {
        if (card.GetType() == typeof(LightingBoltCard))
        {
            worldState.SetValue(HasLightingBoltCard_HTN_WorldKey.Key, (int)worldState.GetValue(HasLightingBoltCard_HTN_WorldKey.Key) - 1);
        }
        else if (card.GetType() == typeof(DistantRelativeCard))
        {
            worldState.SetValue(HasDistantRelativeCard_HTN_WorldKey.Key, (int)worldState.GetValue(HasDistantRelativeCard_HTN_WorldKey.Key) - 1);
        }
        else if (card.GetType() == typeof(MeteorRainCard))
        {
            worldState.SetValue(HasMeteorRainCard_HTN_WorldKey.Key, (int)worldState.GetValue(HasMeteorRainCard_HTN_WorldKey.Key) - 1);
        }
    }

    private void OnAllCardsRemoved()
    {
        worldState.SetValue(HasLightingBoltCard_HTN_WorldKey.Key, 0);
        worldState.SetValue(HasDistantRelativeCard_HTN_WorldKey.Key, 0);
        worldState.SetValue(HasMeteorRainCard_HTN_WorldKey.Key, 0);
    }


    public void Destroy()
    {
        myEntity.OnTeamChanged -= SetMyTeam;
        myEntity.OnCurrentManaChaged -= SetCurrentMana;
        if (myEntity.GetType() == typeof(Player))
        {
            PlayerCardHand.Instance.OnCardAddedInHand -= OnCardAdded;
            PlayerCardHand.Instance.OnAllCardsRemovedInHand -= OnAllCardsRemoved;
            PlayerCardHand.Instance.OnCardRemovedInHand -= OnCardRemoved;
        }
        else
        {
            EnemyDeck.Instance.hand.OnCardAddedInHand -= OnCardAdded;
            EnemyDeck.Instance.hand.OnAllCardsRemovedInHand -= OnAllCardsRemoved;
            EnemyDeck.Instance.hand.OnCardRemovedInHand -= OnCardRemoved;
        }
    }
}
