using System.Collections.Generic;

public readonly struct DeckStats
{
    public readonly int Count;
    public readonly int TotalMana;
    public readonly int TotalBones;
    public readonly int ManaCards;
    public readonly int BoneCards;
    public readonly int BothCards;
    public readonly int FreeCards;
    public readonly int MaxMana;
    public readonly int MaxBones;

    public DeckStats(int count, int totalMana, int totalBones,
                     int manaCards, int boneCards, int bothCards, int freeCards,
                     int maxMana, int maxBones)
    {
        Count = count; TotalMana = totalMana; TotalBones = totalBones;
        ManaCards = manaCards; BoneCards = boneCards;
        BothCards = bothCards; FreeCards = freeCards;
        MaxMana = maxMana; MaxBones = maxBones;
    }

    public float AvgManaAll => Count > 0 ? (float)TotalMana / Count : 0f;
    public float AvgBonesAll => Count > 0 ? (float)TotalBones / Count : 0f;
    public float AvgManaOnManaCards => ManaCards > 0 ? (float)TotalMana / ManaCards : 0f;
    public float AvgBonesOnBoneCards => BoneCards > 0 ? (float)TotalBones / BoneCards : 0f;
    public float ManaShare => Count > 0 ? (float)ManaCards / Count : 0f;
    public float BoneShare => Count > 0 ? (float)BoneCards / Count : 0f;

    public static DeckStats Calculate(IReadOnlyList<Card> cards)
    {
        int totalMana = 0, totalBones = 0;
        int manaCards = 0, boneCards = 0, bothCards = 0, freeCards = 0;
        int maxMana = 0, maxBones = 0;
        int count = 0;

        if (cards != null)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                var c = cards[i];
                if (c == null) continue;

                count++;
                int m = c.ManaCost;
                int b = c.GraveTokensCost;

                totalMana += m;
                totalBones += b;

                if (m > 0) manaCards++;
                if (b > 0) boneCards++;
                if (m > 0 && b > 0) bothCards++;
                if (m == 0 && b == 0) freeCards++;
                if (m > maxMana) maxMana = m;
                if (b > maxBones) maxBones = b;
            }
        }

        return new DeckStats(count, totalMana, totalBones,
            manaCards, boneCards, bothCards, freeCards,
            maxMana, maxBones);
    }

    public static DeckStats CalculateFromIds(IEnumerable<int> cardIds, GlobalCardCollection global)
    {
        if (cardIds == null || global == null)
            return default;

        List<Card> resolved = new();
        foreach (int id in cardIds)
        {
            var card = global.GetCardByStableId(id);
            if (card != null)
                resolved.Add(card);
        }

        return Calculate(resolved);
    }

}