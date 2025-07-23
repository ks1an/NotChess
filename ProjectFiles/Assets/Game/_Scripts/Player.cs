using UnityEngine;

public sealed class Player : MonoBehaviour
{
    [Header("Board")]
    public float upValueWhileSelectingPiece;
    public Material tileFirstMaterial, tileSecondMaterial;
    public Material crossMaterial, zeroMaterial;
    public GameObject crossPrefab, zeroPrefab;
    //material for available moves
    //material for hover tiles

    [Header("Stats")]
    Team localPlayerTeam = Team.None;
    int currentMana = 0;
    int maxMana;


    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void SetSettings(Team type, bool isStartGame = false)
    {
        if (isStartGame)
        {
            maxMana = MatchController.Instance.settings.maxMana;
            currentMana = MatchController.Instance.settings.startMana;
            BoardUI.Singleton.manaBar.IncreaseMana(currentMana);
        }

        localPlayerTeam = type;
    }

    #region +-Mana
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
        BoardUI.Singleton.manaBar.IncreaseMana(value);
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
        BoardUI.Singleton.manaBar.DeacreaseMana(value);
    }
    #endregion

    #region Get
    public Team GetLocalPlayerTeam() { return localPlayerTeam; }
    public bool IsMyTurnOrNot() 
    {
        if ((MatchController.Instance.states.isMoveOfZero && localPlayerTeam == Team.Zero)
            || (!MatchController.Instance.states.isMoveOfZero && localPlayerTeam == Team.Cross))
            return true;
        else
            return false;
    }
    public int GetCurrentMana() { return currentMana; }
    #endregion

}
