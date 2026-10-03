using System;
using UnityEngine;

public class PlayingEntity : MonoBehaviour
{
    public event Action<Team> OnTeamChanged;
    public event Action<int> OnCurrentManaChanged;
    public event Action<int> OnCurrentGraveTokensChanged;

    [Header("Board")]
    public float upValueWhileSelectingPiece;
    public Material tileFirstMaterial, tileSecondMaterial;
    public Material crossMaterial, zeroMaterial;
    public PieceView crossPawnPrefab, zeroPawnPrefab;
    public GameObject graveCoinPrefab;
    //material for available moves
    //material for hover tiles

    [Header("Stats")]
    protected Team localTeam = Team.None;
    protected int currentMana, maxMana;
    protected int currentGraveTokens, maxGraveTokens;

    [HideInInspector] public Deck deck;
    [HideInInspector] public HandObject hand;

    #region Set
    public virtual void SetStartManaAndGraveTokens()
    {
        var settings = GameController.Instance.matchSettings;
        if (localTeam != Team.None)
        {
            maxMana = settings.maxMana;
            currentMana = settings.startMana;
            maxGraveTokens = settings.maxGraveTokens;
            currentGraveTokens = 0;

            if (IsSecondPlayer())
                currentMana += settings.startManaForEvenPlayer;
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
        OnCurrentManaChanged?.Invoke(currentMana);
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
        OnCurrentManaChanged?.Invoke(currentMana);
    }
    #endregion

    #region +- graveTokens
    public virtual void IncreaseGraveTokens(int value)
    {
        if (value < 0)
        {
            Debug.LogError("Negative number received when increasing grave tokens: " + value.ToString());
            return;
        }
        currentGraveTokens += value;
        if (currentGraveTokens > maxGraveTokens)
            currentGraveTokens = maxGraveTokens;
        OnCurrentGraveTokensChanged?.Invoke(currentGraveTokens);
    }

    public virtual void DeacreaseGraveTokens(int value)
    {
        if (value < 0)
        {
            Debug.LogError("Negative number received when deacreasing grave tokens: " + value.ToString());
            return;
        }

        currentGraveTokens -= value;
        if (currentGraveTokens < 0)
            currentGraveTokens = 0;
        OnCurrentGraveTokensChanged?.Invoke(currentGraveTokens);
    }
    #endregion

    #region Get
    public Team GetLocalPlayerTeam() { return localTeam; }
    public string GetStringPlayerTeam()
    {
        switch (localTeam)
        {
            case Team.Cross: return PieceData.CrossTeamName;
            case Team.Zero: return PieceData.ZeroTeamName;
            default: return PieceData.NoneTeamName;
        }
    }

    public bool IsMyTurnOrNot()
    {
        if ((GameController.Instance.states.isMoveOfZero && localTeam == Team.Zero)
            || (!GameController.Instance.states.isMoveOfZero && localTeam == Team.Cross))
            return true;
        else
            return false;
    }
    public int GetCurrentMana() { return currentMana; }

    public int GetCurrentGraveTokens() { return currentGraveTokens; }
    protected bool IsSecondPlayer()
    {
        bool zeroMovesFirst = GameController.Instance.matchSettings.firtsMoveZero;
        if (localTeam == Team.None) return false;
        return zeroMovesFirst
            ? localTeam == Team.Cross
            : localTeam == Team.Zero;
    }
    #endregion
}
