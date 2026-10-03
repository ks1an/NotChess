public class EnemyAI_Sensors
{
    public HTNWorldState worldState;
    public Team myTeam;
    public PlayingEntity myEntity;

    public void Init(PlayingEntity entity)
    {
        worldState = new();
        myEntity = entity;
        worldState.SetValue(CurrentPlayingEntity_HTN_WorldKey.Key, myEntity);
        myEntity.OnTeamChanged += SetMyTeam;
        myEntity.OnCurrentManaChanged += SetCurrentMana;
    }

    public HTNWorldState GetWorldState() { return worldState; }
    public void SetNewWorldState(HTNWorldState newState) { worldState = newState; }

    void SetMyTeam(Team team) => myTeam = team;

    void SetCurrentMana(int currentMana) => worldState.SetValue(CurrentMana_HTN_WorldKey.Key, currentMana);

    public void Destroy()
    {
        myEntity.OnTeamChanged -= SetMyTeam;
        myEntity.OnCurrentManaChanged -= SetCurrentMana;
    }
}
