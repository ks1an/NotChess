using System;
using System.Collections.Generic;

[Serializable]
public sealed class DeckData
{
    public int version = 1;
    public string id = Guid.NewGuid().ToString("N");
    public string collectionName = "newCollection";
    public string cardBackId;
    public bool isFavorite;
    public long lastUsedTicks;
    public List<int> cardIds = new();
}