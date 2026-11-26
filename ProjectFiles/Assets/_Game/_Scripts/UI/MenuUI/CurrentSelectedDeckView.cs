using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CurrentSelectedDeckView : MonoBehaviour
{
    [SerializeField] Image deck;
    [SerializeField] TextMeshProUGUI deckName, deckInfo;

    DeckService _subscribedTo;

    DeckService Deck
    {
        get
        {

            var player = GameController.Instance.player;
            return player != null ? player.UniqueDeck : null;
        }
    }

    void OnEnable()
    {
        EnsureSubscribed();
        UpdateView();
    }

    void OnDisable()
    {
        if (_subscribedTo != null)
            _subscribedTo.Changed -= UpdateView;
        _subscribedTo = null;
    }

    public void UpdateView()
    {
        EnsureSubscribed();

        var d = Deck;
        if (d == null)
        {
            if (deck != null) deck.sprite = null;
            if (deckName != null) deckName.text = string.Empty;
            if (deckInfo != null) deckInfo.text = string.Empty;
            return;
        }

        if (deck != null)
            deck.sprite = d.CardBack != null ? d.CardBack.sprite : null;

        if (deckName != null)
            deckName.text = d.CollectionName;

        if (deckInfo != null)
        {
            var s = d.GetStats();
            deckInfo.text = $"{s.Count}/{d.MaxSize}\n <color=#43A5BE>{s.AvgManaAll:F1}</color>\n <color=red>{s.AvgBonesAll:F1}</color>\n";
        }
    }

    void EnsureSubscribed()
    {
        var d = Deck;
        if (_subscribedTo == d) return;

        if (_subscribedTo != null)
            _subscribedTo.Changed -= UpdateView;

        _subscribedTo = d;

        if (_subscribedTo != null)
            _subscribedTo.Changed += UpdateView;
    }
}