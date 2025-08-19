using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardUI))]
public class Card : MonoBehaviour
{
    public int ID { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [field: SerializeField] public int ManaCost { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public Color ColorBorder { get; private set; }

    [field: SerializeField] public bool IsUseOnlyMyTurn { get; private set; }
    [field: SerializeField] public CardOrientation Orientation { get; private set; }
    [field: SerializeField] public Category Category { get; private set; }
    [field: SerializeField] public CardRarity Rarity { get; private set; }

    [field: SerializeField] public Color BoarderColorOnDrag { get; private set; }

    public CardUI CardUI { get; private set; }
    public CardHand Hand { get; private set; }

    protected List<Vector2Int> availableMoves = new();

    public virtual void OnValidate()
    { 
        CardUI = GetComponent<CardUI>();
        CardUI.SetCardUI(); 
    }

    public virtual void Init()
    {
        CardUI = GetComponent<CardUI>();
        CardUI.SetCardUI();
        ID = CreateID();
        Hand = Deck.Instance.playerHand;
    }

    public virtual void UseCard(List<Vector2Int> moves, bool isSynced) { }

    public virtual List<Vector2Int> GetAvailableMoves(int maxX, int maxY, int hoverX, int hoverY)
    {
        List<Vector2Int> available = new();
        return available;
    }

    public virtual bool IsAvailableMove(int targetX, int targetY) { return false; }

    public int CreateID()
    {
        string id = Name.Length.ToString() + Description.Length.ToString() + ((int)ColorBorder.b).ToString() +
            ((int)Orientation).ToString() + ((int)Category).ToString() + ((int)Rarity).ToString();

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