using System;
using System.Collections.Generic;

[Serializable]
public class DeckEntryData
{
    public string id = Guid.NewGuid().ToString("N");
    public bool isFavorite;
    public long lastUsedTicks; 
    public DeckData deckData = new();
}

[Serializable]
public class DeckLibraryData
{
    public string activeDeckId;
    public List<DeckEntryData> decks = new();
}