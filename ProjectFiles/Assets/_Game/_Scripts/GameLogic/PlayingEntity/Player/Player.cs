public sealed class Player : PlayingEntity
{
    public CardCollection cardCollection;
    public StackView graveCoinStackView;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public override void SetStartManaAndGraveTokens()
    {
        base.SetStartManaAndGraveTokens();
        GameController.Instance.playerManaBottle.SetSettings(currentMana, maxMana);
        if (graveCoinStackView != null) graveCoinStackView.RemoveAll();
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

    public override void IncreaseGraveTokens(int value)
    {
        base.IncreaseGraveTokens(value);

        if (graveCoinStackView != null)
        {
            int amount = currentGraveTokens - graveCoinStackView.GetCountInStack();
            if (GameController.Instance.states.isNetMatch)
                GameController.Instance.netMatch.OnPlayerGraveCoinChangeRpc(amount, true);
            graveCoinStackView.Add(graveCoinPrefab, amount);
        }
    }

    public override void DeacreaseGraveTokens(int value)
    {
        base.DeacreaseGraveTokens(value);
        if (graveCoinStackView != null)
        {
            graveCoinStackView.Remove(value);
            if (GameController.Instance.states.isNetMatch)
                GameController.Instance.netMatch.OnPlayerGraveCoinChangeRpc(value, false);
        }
    }
}
