using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DeckService
{
    public const int DefaultMaxSize = 30;

    readonly GlobalCardCollection _global;
    readonly int _maxSize;
    readonly List<Card> _cards = new();

    public event Action Changed;

    public DeckService(GlobalCardCollection global, int maxSize = DefaultMaxSize)
    {
        _global = global != null ? global : throw new ArgumentNullException(nameof(global));
        if (maxSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxSize));
        _maxSize = maxSize;
    }

    // ===== Read =====

    public IReadOnlyList<Card> Cards => _cards;
    public int Count => _cards.Count;
    public int MaxSize => _maxSize;
    public bool IsFull => _cards.Count >= _maxSize;

    public SpriteRenderer CardBack { get; private set; }
    public string CollectionName { get; private set; } = "newCollection";

    public bool Contains(Card card) => card != null && IndexOf(card) >= 0;

    // ===== Mutate =====

    public bool Add(Card card)
    {
        if (card == null) return false;
        if (IsFull) return false;
        if (Contains(card)) return false;

        _cards.Add(card);
        Changed?.Invoke();
        return true;
    }

    public bool Remove(Card card)
    {
        if (card == null) return false;
        int idx = IndexOf(card);
        if (idx < 0) return false;

        _cards.RemoveAt(idx);
        Changed?.Invoke();
        return true;
    }

    public void Clear()
    {
        if (_cards.Count == 0) return;
        _cards.Clear();
        Changed?.Invoke();
    }

    public void SetCardBack(SpriteRenderer back)
    {
        if (CardBack == back) return;
        CardBack = back;
        Changed?.Invoke();
    }

    public void SetCollectionName(string name)
    {
        string normalized = string.IsNullOrEmpty(name) ? "newCollection" : name;
        if (CollectionName == normalized) return;
        CollectionName = normalized;
        Changed?.Invoke();
    }

    int IndexOf(Card card)
    {
        int id = card.ID;
        for (int i = 0; i < _cards.Count; i++)
            if (_cards[i] != null && _cards[i].ID == id) return i;
        return -1;
    }

    // ===== Stats =====

    public DeckStats GetStats() => DeckStats.Calculate(_cards);

    public int[] GetManaCurve()
    {
        var stats = GetStats();
        var curve = new int[stats.MaxMana + 1];
        foreach (var c in _cards)
        {
            if (c == null) continue;
            int m = c.ManaCost;
            if (m >= 0 && m < curve.Length) curve[m]++;
        }
        return curve;
    }

    public int[] GetBoneCurve()
    {
        var stats = GetStats();
        var curve = new int[stats.MaxBones + 1];
        foreach (var c in _cards)
        {
            if (c == null) continue;
            int b = c.GraveTokensCost;
            if (b >= 0 && b < curve.Length) curve[b]++;
        }
        return curve;
    }

    // ===== Serialization =====

    public DeckData ToData()
    {
        var data = new DeckData
        {
            collectionName = CollectionName,
            cardBackId = CardBack != null ? CardBack.gameObject.name : null,
        };
        foreach (var c in _cards)
            if (c != null) data.cardIds.Add(c.ID);
        return data;
    }

    /// <summary>
    /// Загружает данные в память. Неизвестные id и дубли пропускаются.
    /// Если после загрузки колода пуста и fallbackToStandardIfEmpty = true — ставится стандартная.
    /// </summary>
    public void LoadFromData(DeckData data, bool fallbackToStandardIfEmpty = true)
    {
        _cards.Clear();

        if (data != null)
        {
            var seen = new HashSet<int>();
            int skipped = 0;

            foreach (int id in data.cardIds)
            {
                if (!seen.Add(id)) continue;               // дубль в данных
                if (_cards.Count >= _maxSize) { skipped++; continue; }

                var card = _global.GetCardByStableId(id);
                if (card == null)
                {
                    skipped++;
                    Debug.LogWarning($"Deck: unknown card id {id} skipped");
                    continue;
                }

                _cards.Add(card);
            }

            if (skipped > 0)
                Debug.LogWarning($"Deck: {skipped} entries skipped on load");

            CardBack = _global.GetCardBackById(data.cardBackId);
            CollectionName = string.IsNullOrEmpty(data.collectionName) ? "newCollection" : data.collectionName;
        }

        if (_cards.Count == 0 && fallbackToStandardIfEmpty)
        {
            ResetToStandard();
            return;
        }

        Changed?.Invoke();
    }

    // ===== Persistence =====

    public bool Save(string fileName = DeckRepository.DefaultFileName)
        => DeckRepository.Save(ToData(), fileName);

    public bool Load(string fileName = DeckRepository.DefaultFileName)
    {
        var data = DeckRepository.Load(fileName);
        if (data == null)
        {
            Debug.LogWarning("Deck: save missing or unreadable — using standard deck");
            ResetToStandard();
            return false;
        }
        LoadFromData(data);
        return true;
    }

    // ===== Standard deck =====

    /// <summary>
    /// Стандартная колода: все уникальные карты из GlobalCardCollection, до MaxSize.
    /// Без случайных дублей и без повторов.
    /// </summary>
    public void ResetToStandard()
    {
        _cards.Clear();

        foreach (var pair in _global.GlobalCardsDictionary)
        {
            if (_cards.Count >= _maxSize) break;
            var card = pair.Value;
            if (card == null) continue;
            if (Contains(card)) continue;
            _cards.Add(card);
        }

        if (_global.GlobalCardBacks != null && _global.GlobalCardBacks.Count > 0)
        {
            var chosen = _global.GlobalCardBacks[UnityEngine.Random.Range(0, _global.GlobalCardBacks.Count)];
            CardBack = chosen != null ? chosen.GetComponent<SpriteRenderer>() : null;
        }
        else
        {
            CardBack = null;
        }

        CollectionName = "newCollection";
        Changed?.Invoke();
    }
}