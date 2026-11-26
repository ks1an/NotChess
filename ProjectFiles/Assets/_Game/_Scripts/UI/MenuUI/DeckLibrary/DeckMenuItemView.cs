using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DeckMenuItemView : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text statsText;
    [SerializeField] Image avatar;
    [SerializeField] GameObject activeBadge;

    [SerializeField] Button selectButton;
    [SerializeField] Button editButton;
    [SerializeField] Button deleteButton;

    string _deckId;
    Action<string> _onSelect;
    Action<string> _onEdit;
    Action<string> _onDelete;

    void Awake()
    {
        if (selectButton != null)
            selectButton.onClick.AddListener(() => _onSelect?.Invoke(_deckId));

        if (editButton != null)
            editButton.onClick.AddListener(() => _onEdit?.Invoke(_deckId));

        if (deleteButton != null)
            deleteButton.onClick.AddListener(() => _onDelete?.Invoke(_deckId));
    }

    public void Bind(
        DeckData data,
        DeckStats stats,
        bool isActive,
        bool canDelete,
        int maxDeckSize,
        Action<string> onSelect,
        Action<string> onEdit,
        Action<string> onDelete)
    {
        _deckId = data.id;
        _onSelect = onSelect;
        _onEdit = onEdit;
        _onDelete = onDelete;

        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(data.collectionName) ? "newCollection" : data.collectionName;

        if (avatar != null)
            avatar.sprite = GameController.Instance.globalCards.GetCardBackById(data.cardBackId).sprite;

        if (statsText != null)
        {
            statsText.text = $"{stats.Count}/{maxDeckSize}  <color=#43A5BE>{stats.AvgManaAll:F1}</color>  <color=red>{stats.AvgBonesAll:F1}</color>";
        }

        if (activeBadge != null)
            activeBadge.SetActive(isActive);

        if (deleteButton != null)
            deleteButton.gameObject.SetActive(canDelete); 
    }
}