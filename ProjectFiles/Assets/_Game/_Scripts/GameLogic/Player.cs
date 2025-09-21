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
    int currentMana;
    int maxMana;


    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void SetPlayerTeam(Team type) => localPlayerTeam = type;

    public void SetStartMana()
    {
        var settings = GameController.Instance.settings;
        if (localPlayerTeam != Team.None)
        {
            maxMana = settings.maxMana;
            currentMana = settings.startMana;
            currentMana += ((settings.firtsMoveZero == (localPlayerTeam == Team.Cross)) && localPlayerTeam != Team.None) ?
                settings.startManaForEvenPlayer : 0;

            BoardUI.Singleton.manaBar.SetSettings(currentMana, maxMana);
        }
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
        if ((GameController.Instance.states.isMoveOfZero && localPlayerTeam == Team.Zero)
            || (!GameController.Instance.states.isMoveOfZero && localPlayerTeam == Team.Cross))
            return true;
        else
            return false;
    }
    public int GetCurrentMana() { return currentMana; }
    #endregion
}
