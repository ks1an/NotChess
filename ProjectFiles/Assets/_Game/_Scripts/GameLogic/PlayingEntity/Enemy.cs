public class Enemy : PlayingEntity
{
    public StackView graveCoinStackView;

    void Awake()
    {
        DontDestroyOnLoad(this);
        graveCoinStackView = GameController.Instance.enemyGraveCoinView;
    }

    public override void SetStartManaAndGraveTokens()
    {
        base.SetStartManaAndGraveTokens();
        GameController.Instance.enemyManaBottle.SetSettings(currentMana, maxMana);
        if (graveCoinStackView != null) graveCoinStackView.RemoveAll();
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

    public override void IncreaseGraveTokens(int value)
    {
        base.IncreaseGraveTokens(value);
        if (graveCoinStackView != null) graveCoinStackView.Add(graveCoinPrefab, currentGraveTokens - graveCoinStackView.GetCountInStack());
    }

    public override void DeacreaseGraveTokens(int value)
    {
        base.DeacreaseGraveTokens(value);
        if (graveCoinStackView != null) graveCoinStackView.Remove(value);
    }
}
