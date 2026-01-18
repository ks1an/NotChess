using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public enum CardAttackStrong
{
    Zero = 0,
    Light,
    Average,
    Advaced,
    Strong
}

[RequireComponent(typeof(CardVisual))]
public class Card : MonoBehaviour
{
    public string originalCardName;
    [field: TextArea] public string originalDescription;
    public int ID
    {
        get
        {
            return GetID();
        }
        set
        {
            ID = GetID();
        }
    }

    public string DisplayName { get; private set; }
    public string DisplayDescription { get; private set; }
    [field: SerializeField] public int ManaCost { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public Color ColorBorder { get; private set; }

    [field: SerializeField] public bool IsUseOnlyMyTurn { get; private set; } = true;
    [field: SerializeField] public CardOrientation Orientation { get; private set; }
    [field: SerializeField] public Category Category { get; private set; }
    [field: SerializeField] public CardRarity Rarity { get; private set; }
    [field: SerializeField] public CardAttackStrong AttackClass { get; private set; }

    [field: SerializeField] public Color BoarderColorOnDrag { get; private set; }

    public CardVisual CardUI { get; private set; }
    public PlayerCardHand Hand { get; private set; }

    protected List<Vector2Int> availableMoves = new();
    bool showOnlyOriginalName;

    public virtual void OnValidate()
    {
        DisplayName = originalCardName;
        DisplayDescription = originalDescription;
        UpdateCardUI();
    }

    #region Sets
    void TryBlockSetDisplayNameByLocals(bool b)
    {
        showOnlyOriginalName = b;
        if (b)
        {
            DisplayName = originalCardName;
            UpdateCardUI();
        }
    }

    public void SetDisplayNameOfCard(string value)
    {
        if(showOnlyOriginalName) return;
        DisplayName = value;
        UpdateCardUI();
    }

    public void SetDisplayDescriptionOfCard(string value)
    {
        DisplayDescription = value;
        UpdateCardUI();
    }
    #endregion

    public virtual void Init()
    {
        GetID();
        CardUI = GetComponent<CardVisual>();
        UpdateCardUI();
    
        Hand = PlayerDeck.Instance.hand;
        if (Hand.CardsInHand.Contains(this))
            CardUI.CreateCardInHandLogic();

        TryBlockSetDisplayNameByLocals(GameController.Instance.gameSettings.DontTranslateNameOfCard.Value);
        GameController.Instance.gameSettings.DontTranslateNameOfCard.OnChanged += TryBlockSetDisplayNameByLocals;
    }

    void UpdateCardUI()
    {
        if (CardUI != null)
            CardUI.SetCardUI();
        else
        {
            CardUI = GetComponent<CardVisual>();
            CardUI.SetCardUI();
        }
    }

    void OnMouseDown()
    {
        OnCursorDown();
    }
    protected virtual void OnCursorDown() { }


    public void DoOnMouseUp(bool withoutAction = false)
    {
        if (!withoutAction)
            DoActionOnMouseUp();

        if (PlayerDeck.Instance.hand.CurrentSelectCard == this)
        {
            Hand.ResetCurrentSelectCard(this);
        }

        CardUI.MouseExitFromCard(gameObject);
    }
    protected virtual void DoActionOnMouseUp() { }

    public virtual void UseCard(List<Vector2Int> moves, bool isSynced) { }

    public virtual List<Vector2Int> GetAvailableMoves(int maxX, int maxY, int hoverX, int hoverY)
    {
        List<Vector2Int> available = new();
        return available;
    }

    public virtual bool IsAvailableMove(int targetX, int targetY) { return false; }

    public int GetID()
    {
        if (ID != 0)
            return ID;

        string id = originalCardName.Length.ToString() + originalDescription.Length.ToString() + ((int)ColorBorder.b).ToString() +
            ((int)Orientation).ToString() + ((int)Category).ToString() + ((int)Rarity).ToString() + ((int)AttackClass).ToString();

        return int.Parse(id);
    }

    private void OnDestroy()
    {
        transform.DOKill();

        GameController.Instance.gameSettings.DontTranslateNameOfCard.OnChanged -= TryBlockSetDisplayNameByLocals;
    }
}

public enum CardOrientation
{
    ATTACK,
    PROTECT,
    BUILD,
    Social
}

public enum Category
{
    CELLS,
    PIECES,
    PIECES_AND_CELLS,
    HandOrDeck
}

public enum CardRarity
{
    BASIC,
    COMMON,
    RARE,
    EPIC,
    LEGENDARY
}