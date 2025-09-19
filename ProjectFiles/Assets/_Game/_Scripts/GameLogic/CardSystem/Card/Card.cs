using System.Collections.Generic;
using UnityEngine;

public enum CardAttackStrong
{
    Zero,
    Light,
    Average,
    Advaced,
    Strong
}

[RequireComponent(typeof(CardUI))]
public class Card : MonoBehaviour
{
    [SerializeField] string originalCardName;
    [field: SerializeField, TextArea] string originalDescription;
    public int ID { get; private set; }

    public string DisplayName { get; private set; }
    public string DisplayDescription { get; private set; }
    [field: SerializeField] public int ManaCost { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public Color ColorBorder { get; private set; }

    [field: SerializeField] public bool IsUseOnlyMyTurn { get; private set; }
    [field: SerializeField] public CardOrientation Orientation { get; private set; }
    [field: SerializeField] public Category Category { get; private set; }
    [field: SerializeField] public CardRarity Rarity { get; private set; }
    [field: SerializeField] public CardAttackStrong AttackClass { get; private set; }  

    [field: SerializeField] public Color BoarderColorOnDrag { get; private set; }

    public CardUI CardUI { get; private set; }
    public PlayerCardHand Hand { get; private set; }

    protected List<Vector2Int> availableMoves = new();

    public virtual void OnValidate()
    {
        DisplayName = originalCardName;
        DisplayDescription = originalDescription;
        UpdateCardUI();
    }

    #region Sets
    public void SetDisplayNameOfCard(string value)
    {
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
        CardUI = GetComponent<CardUI>();
        UpdateCardUI();
        ID = SelfGetID();
        Hand = PlayerDeck.Instance.hand;
    }

    void UpdateCardUI()
    {
        if (CardUI != null)
            CardUI.SetCardUI();
        else
        {
            CardUI = GetComponent<CardUI>();
            CardUI.SetCardUI();
        }
    }

    public virtual void UseCard(List<Vector2Int> moves, bool isSynced) { }

    public virtual List<Vector2Int> GetAvailableMoves(int maxX, int maxY, int hoverX, int hoverY)
    {
        List<Vector2Int> available = new();
        return available;
    }

    public virtual bool IsAvailableMove(int targetX, int targetY) { return false; }

    public int SelfGetID()
    {
        if(ID != 0)
            return ID;

        string id = originalCardName.Length.ToString() + originalDescription.Length.ToString() + ((int)ColorBorder.b).ToString() +
            ((int)Orientation).ToString() + ((int)Category).ToString() + ((int)Rarity).ToString() + ((int)AttackClass).ToString();

        return int.Parse(id);
    }
}

public enum CardOrientation
{
    ATTACK,
    PROTECT,
    BUILD
}

public enum Category
{
    CELLS,
    PIECES,
    PIECES_AND_CELLS
}

public enum CardRarity
{
    BASIC,
    COMMON,
    RARE,
    EPIC,
    LEGENDARY
}