using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class DeckView : MonoBehaviour
{
    public CardsInDeckViewModel cardsViewModel;
    [SerializeField] DeckSelectionMenuView deckLibrary;
    [SerializeField] Button renameButton;
    [SerializeField] TMP_Text deckNameText;

    [HideInInspector] public SpriteRenderer deckIcon;

    DeckService CurrentDeck => cardsViewModel != null
        ? cardsViewModel.CurrentDeck
        : GameController.Instance.player.UniqueDeck;

    bool needOpenDeckLibraryAfterDisable;

    void OnEnable()
    {
        if (cardsViewModel != null)
            cardsViewModel.DeckChanged += SyncFromModel;

        if (renameButton != null)
        {
            renameButton.onClick.RemoveListener(OnRenameClicked);
            renameButton.onClick.AddListener(OnRenameClicked);
        }

        SyncFromModel();
    }

    void OnDisable()
    {
        if (cardsViewModel != null)
            cardsViewModel.DeckChanged -= SyncFromModel;

        if (renameButton != null)
            renameButton.onClick.RemoveListener(OnRenameClicked);

        if (needOpenDeckLibraryAfterDisable && deckLibrary != null)
        {
            deckLibrary.gameObject.SetActive(true);
            needOpenDeckLibraryAfterDisable = false;
        }
    }

    public void SetOpenDeckLibraryAfterDisable(bool b) => needOpenDeckLibraryAfterDisable = b;

    void SyncFromModel()
    {
        var d = CurrentDeck;
        deckIcon = d?.CardBack;
        ShowDeckIcon();
        ShowDeckName(d);
    }

    void ShowDeckIcon()
    {
        var image = GetComponent<Image>();
        if (deckIcon == null) { image.sprite = null; image.enabled = false; return; }
        image.sprite = deckIcon.sprite;
        image.enabled = true;
    }

    void ShowDeckName(DeckService deck)
    {
        if (deckNameText == null) return;

        deckNameText.text = (deck != null && !string.IsNullOrWhiteSpace(deck.CollectionName))
            ? deck.CollectionName
            : "newCollection";
    }

    public void NextIcon()
    {
        var global = GameController.Instance.globalCards;
        var backs = global.GlobalCardBacks;
        if (backs == null || backs.Count == 0) return;

        int curIndex = deckIcon != null ? backs.IndexOf(deckIcon.gameObject) : -1;
        int nextIndex = (curIndex + 1) % backs.Count;
        var nextBack = backs[nextIndex] != null ? backs[nextIndex].GetComponent<SpriteRenderer>() : null;
        if (nextBack == null) return;
        CurrentDeck?.SetCardBack(nextBack);
    }

    void OnRenameClicked()
    {
        var deck = CurrentDeck;
        if (deck == null || ModalInputWindow.Instance == null)
            return;

        string currentName = !string.IsNullOrWhiteSpace(deck.CollectionName)
            ? deck.CollectionName
            : "newCollection";

        ModalInputWindow.Instance.Show(
            title: "Rename Deck",
            placeholder: currentName,
            onCancel: null,
            onOk: newName => ApplyNewName(deck, newName),
            characterLimit: 24
        );
    }

    void ApplyNewName(DeckService deck, string newName)
    {
        if (deck == null || string.IsNullOrWhiteSpace(newName))
            return;

        string trimmed = newName.Trim();
        deck.SetCollectionName(trimmed);
        ShowDeckName(deck);

        var manager = DeckLibraryManager.Instance;
        if (manager != null)
        {
            var playerDeck = GameController.Instance?.player?.UniqueDeck;
            if (deck == playerDeck && !string.IsNullOrEmpty(manager.ActiveDeckId))
            {
                var activeData = manager.Decks.FirstOrDefault(d => d != null && d.id == manager.ActiveDeckId);
                if (activeData != null)
                    manager.RenameDeck(activeData.id, trimmed);
            }
        }
    }
}