using System;
using UnityEngine;

public class PlayingEntity : MonoBehaviour 
{
    public event Action <Team> OnTeamChanged;
    public event Action<int> OnCurrentManaChaged;

    [Header("Board")]
    public float upValueWhileSelectingPiece;
    public Material tileFirstMaterial, tileSecondMaterial;
    public Material crossMaterial, zeroMaterial;
    public GameObject crossPrefab, zeroPrefab;
    //material for available moves
    //material for hover tiles

    [Header("Stats")]
    protected Team localTeam = Team.None;
    protected int currentMana, maxMana;

    [HideInInspector] public Deck deck;
    [HideInInspector] public HandObject hand;

    #region Set
    public virtual void SetStartMana()
    {
        var settings = GameController.Instance.matchSettings;
        if (localTeam != Team.None)
        {
            maxMana = settings.maxMana;
            currentMana = settings.startMana;
            IncreaseMana(((settings.firtsMoveZero == (localTeam == Team.Cross))
                && localTeam != Team.None) ?
                    settings.startManaForEvenPlayer : 0);
        }
    }

    public virtual void SetTeam(Team team)
    {
        localTeam = team;
        OnTeamChanged?.Invoke(localTeam);
    }
    #endregion

    #region +- mana
    public virtual void IncreaseMana(int value)
    {
        if (value < 0)
        {
            Debug.LogError("Negative number received when increasing mana: " + value.ToString());
            return;
        }
        currentMana += value;
        if (currentMana > maxMana)
            currentMana = maxMana;
        OnCurrentManaChaged?.Invoke(currentMana);
    }

    public virtual void DeacreaseMana(int value)
    {
        if (value < 0)
        {
            Debug.LogError("Negative number received when deacreasing mana: " + value.ToString());
            return;
        }

        currentMana -= value;
        if (currentMana < 0)
            currentMana = 0;
        OnCurrentManaChaged?.Invoke(currentMana);
    }
    #endregion

    #region Get
    public Team GetLocalPlayerTeam() { return localTeam; }
    public bool IsMyTurnOrNot()
    {
        if ((GameController.Instance.states.isMoveOfZero && localTeam == Team.Zero)
            || (!GameController.Instance.states.isMoveOfZero && localTeam == Team.Cross))
            return true;
        else
            return false;
    }
    public int GetCurrentMana() { return currentMana; }
    #endregion
}
