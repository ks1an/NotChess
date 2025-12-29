using UnityEngine;

public class Enemy
{
    //Stats
    Team localTeam = Team.None;
    int currentMana;
    int maxMana;

    public void SetTeam(Team type) => localTeam = type;

    public void SetStartMana()
    {
        var settings = GameController.Instance.matchSettings;
        maxMana = settings.maxMana;
        currentMana = settings.startMana;
        GameController.Instance.enemyManaBottle.SetSettings(currentMana, maxMana);
    }

    public void IncreaseMana(int value)
    {
        if (value < 0)
        {
            Debug.LogError("Negative number received when increasing mana: " + value.ToString());
            return;
        }
        currentMana += value;
        if (currentMana > maxMana)
            currentMana = maxMana;
        GameController.Instance.enemyManaBottle.IncreaseMana(value);
    }

    public void DeacreaseMana(int value)
    {
        if (value < 0)
        {
            Debug.LogError("Negative number received when deacreasing mana: " + value.ToString());
            return;
        }

        currentMana -= value;
        if (currentMana < 0)
            currentMana = 0;
        GameController.Instance.enemyManaBottle.DeacreaseMana(value);
    }
}
