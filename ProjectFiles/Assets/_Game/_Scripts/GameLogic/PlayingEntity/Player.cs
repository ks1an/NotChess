public sealed class Player : PlayingEntity
{
    public CardCollection cardCollection;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public override void SetStartMana()
    {
        base.SetStartMana();

        GameController.Instance.playerManaBottle.SetSettings(currentMana, maxMana);
    }

    public override void SetTeam(Team team)
    {
        base.SetTeam(team);
        deck = PlayerDeck.Instance;
        hand = PlayerCardHand.Instance;
    }

    public override void IncreaseMana(int value)
    {
        base.IncreaseMana(value);

        if (GameController.Instance.states.isNetMatch)
            GameController.Instance.netMatch.OnPlayerManaChangeRpc(value);

        GameController.Instance.playerManaBottle.IncreaseMana(value);
    }

    public override void DeacreaseMana(int value)
    {
        base.DeacreaseMana(value);

        if (GameController.Instance.states.isNetMatch)
            GameController.Instance.netMatch.OnPlayerManaChangeRpc(value * -1);

        GameController.Instance.playerManaBottle.DeacreaseMana(value);
    }
}
