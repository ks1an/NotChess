using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CardAttackStrong
{
    Zero = 0,
    Light,
    Average,
    Advaced,
    Strong
}

[RequireComponent(typeof(CardVisual))]
public class Card : MonoBehaviour, IPointerDownHandler
{
    public string originalCardName;
    [field: TextArea] public string originalDescription;

    public string DisplayName { get; private set; }
    public string DisplayDescription { get; private set; }
    [field: SerializeField] public int ManaCost { get; private set; }
    [field: SerializeField] public int GraveTokensCost { get; private set;  }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public Color ColorBorder { get; private set; }

    [field: SerializeField] public bool IsUseOnlyMyTurn { get; private set; } = true;
    [field: SerializeField] public CardOrientation Orientation { get; private set; }
    [field: SerializeField] public TargetCategory TargetCategory { get; private set; }
    [field: SerializeField] public CardRarity Rarity { get; private set; }
    [field: SerializeField] public CardAttackStrong AttackClass { get; private set; }

    [field: SerializeField] public Color BoarderColorOnDrag { get; private set; }

    public CardVisual CardUI { get; private set; }
    public PlayerCardHand Hand { get; private set; }

    protected List<Vector2Int> availableMoves = new();
    protected bool cardUsedForLandscape;
    protected bool cardInited;
    protected Team teamWhoHave;
    bool showOnlyOriginalName;

    [SerializeField, HideInInspector] private string stableGuid;
    [SerializeField, HideInInspector] private int stableId;
    public int ID
    {
        get
        {
#if UNITY_EDITOR
            EnsureStableId();
#endif
            return stableId;
        }
    }
#if UNITY_EDITOR
    void EnsureStableId()
    {
        if (string.IsNullOrEmpty(stableGuid))
        {
            stableGuid = System.Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }
        int h = stableGuid.GetHashCode();
        stableId = StableHash(stableGuid);
    }

    static int StableHash(string s)
    {
        unchecked
        {
            int h = 5381;
            foreach (char c in s) h = ((h << 5) + h) ^ c; 
            return h == 0 ? 1 : h;
        }
    }

    public virtual void OnValidate()
    {
        EnsureStableId();
        DisplayName = originalCardName;
        DisplayDescription = originalDescription;
        UpdateCardUI();
    }
#endif

    #region Sets
    void TryBlockSetDisplayNameByLocals(bool b)
    {
        if (this == null || !gameObject) return;
        showOnlyOriginalName = b;
        if (b)
        {
            DisplayName = originalCardName;
            UpdateCardUI();
        }
    }

    public void SetDisplayNameOfCard(string value)
    {
        if (showOnlyOriginalName) return;
        DisplayName = value;
        UpdateCardUI();
    }

    public void SetDisplayDescriptionOfCard(string value)
    {
        //DisplayDescription = GameController.Instance.globalCards.GetImortantWordsFromDescription(value);
        DisplayDescription = value;
        UpdateCardUI();
    }

    public void SetCardUsingForLandscape(bool b) => cardUsedForLandscape = b;
    #endregion

    public virtual void Init(Team teamWhoHave)
    {
        this.teamWhoHave = teamWhoHave;
        CardUI = GetComponent<CardVisual>();
        UpdateCardUI();

        Hand = PlayerCardHand.Instance;
        if (Hand.CardsInHand.Contains(this))
            CardUI.CreateCardInHandLogic();

        TryBlockSetDisplayNameByLocals(GameController.Instance.gameSettings.DontTranslateNameOfCard.Value);
        cardInited = true;
    }

    void UpdateCardUI()
    {
        if (this == null || !gameObject) return;
        if (CardUI != null)
            CardUI.SetCardUI();
        else
        {
            TryGetComponent<CardVisual>(out CardVisual CardUI);
            this.CardUI = CardUI;
            if (CardUI != null)
                CardUI.SetCardUI();
            else
                Debug.Log("CardVisual is null: " + originalCardName);
        }
    }

    public virtual bool CanBeSelected()
    {
        var player = GameController.Instance.player;

        if (player.GetCurrentMana() < ManaCost) return false;
        if (player.GetCurrentGraveTokens() < GraveTokensCost) return false; 
        if (IsUseOnlyMyTurn && !player.IsMyTurnOrNot()) return false;

        return true;
    }

    public virtual bool CanBeUsed()
    {
        return CanBeSelected();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnCursorDown();
    }

    protected virtual void OnCursorDown() { }


    public void DoOnMouseUp(bool withoutAction = false)
    {
        if (!withoutAction)
            DoActionOnMouseUp();

        if (PlayerCardHand.Instance.CurrentSelectCard == this)
        {
            Hand.ResetCurrentSelectCard(this);
        }

        CardUI.MouseExitFromCard(gameObject);
    }
    protected virtual void DoActionOnMouseUp()
    {
        if (!CanBeUsed() || availableMoves.Count < 1) return;

        GameController.Instance.player.DecreaseMana(ManaCost);
        GameController.Instance.player.DecreaseGraveTokens(GraveTokensCost);

        GameController.Instance.states.stats.countOfPlayedCards++;
        GameController.Instance.states.stats.manaSpent += ManaCost;
        GameController.Instance.states.stats.graveTokensSpent += GraveTokensCost;

        AnalyticService.Instance.LogCardPlayed(originalCardName, GameController.Instance.states.turnCount);

        UseCard(availableMoves, false);
    } 

    public virtual void UseCard(List<Vector2Int> moves, bool isSynced)
    {

    }

    public virtual List<Vector2Int> GetAvailableMoves(int maxX, int maxY, int hoverX, int hoverY)
    {
        List<Vector2Int> available = new();
        return available;
    }

    public virtual bool IsAvailableMove(int targetX, int targetY) { return false; }

    private void OnEnable()
    {
        GameController.Instance.gameSettings.DontTranslateNameOfCard.OnChanged += TryBlockSetDisplayNameByLocals;
    }

    private void OnDisable()
    {
        GameController.Instance.gameSettings.DontTranslateNameOfCard.OnChanged -= TryBlockSetDisplayNameByLocals;
    }

    public void KillCard()
    {
        GameController.Instance.gameSettings.DontTranslateNameOfCard.OnChanged -= TryBlockSetDisplayNameByLocals;
        transform.DOKill();
        Destroy(gameObject);
    }
}

public enum CardOrientation
{
    ATTACK,
    PROTECT,
    BUILD,
    Social
}

public enum TargetCategory
{
    CELLS,
    PIECES,
    EnemyHand
}

public enum CardRarity
{
    BASIC,
    COMMON,
    RARE,
    EPIC,
    LEGENDARY
}