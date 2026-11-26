using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Deck Preset", fileName = "DeckPreset_")]
public sealed class CardCollection : ScriptableObject
{
    [SerializeField] string displayName = "newPreset";
    [SerializeField] List<Card> cards = new();
    [SerializeField] SpriteRenderer cardBack;

    public string DisplayName => displayName;
    public IReadOnlyList<Card> Cards => cards;
    public SpriteRenderer CardBack => cardBack;

    public DeckService BuildService(GlobalCardCollection global, int maxSize)
    {
        if (global == null) return null;

        var ds = new DeckService(global, maxSize);

        foreach (var card in cards)
            if (card != null) ds.Add(card);

        var validBack = GetValidCardBack();
        if (validBack == null && global.GlobalCardBacks != null && global.GlobalCardBacks.Count > 0)
        {
            var obj = global.GlobalCardBacks[Random.Range(0, global.GlobalCardBacks.Count)];
            if (obj != null) validBack = obj.GetComponent<SpriteRenderer>();
        }

        if (validBack != null) ds.SetCardBack(validBack);
        return ds;
    }

    SpriteRenderer GetValidCardBack()
    {
        if (cardBack == null) return null;
        try
        {
            return cardBack.gameObject != null ? cardBack : null;
        }
        catch (MissingReferenceException)
        {
            return null;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (cards == null) return;
        cards.RemoveAll(c => c == null);

        int n = cards.Count;
        if (n == 0)
            Debug.LogWarning($"[{name}] Deck preset is empty — add cards in the inspector", this);
        else if (n > 60)
            Debug.LogWarning($"[{name}] Deck preset has {n} cards — likely duplicated entries", this);
    }
#endif
}