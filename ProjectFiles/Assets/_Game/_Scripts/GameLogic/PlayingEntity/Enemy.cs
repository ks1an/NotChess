public class Enemy : PlayingEntity
{
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public override void SetStartMana()
    {
        base.SetStartMana();
        GameController.Instance.enemyManaBottle.SetSettings(currentMana, maxMana);
    }

    public override void SetTeam(Team team)
    {
        base.SetTeam(team);
        deck = EnemyDeck.Instance;
    }


    public override void IncreaseMana(int value)
    {
        base.IncreaseMana(value);
        GameController.Instance.enemyManaBottle.IncreaseMana(value);
    }

    public override void DeacreaseMana(int value)
    {
        base.DeacreaseMana(value);
        GameController.Instance.enemyManaBottle.DeacreaseMana(value);
    }
}
