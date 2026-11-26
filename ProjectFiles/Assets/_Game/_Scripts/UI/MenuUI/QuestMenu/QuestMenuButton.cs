using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuestMenuButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private GameObject notificationBadge;

    [Header("Colors Design")]
    [SerializeField] private Color normalColor = new Color32(0x21, 0x21, 0x26, 0xFF);     // #212126
    [SerializeField] private Color highlightedColor = new Color32(0x32, 0x33, 0x3B, 0xFF); // #32333B
    [SerializeField] private Color questAvailableColor = new Color32(0x3A, 0x3F, 0x50, 0xFF);

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (GameController.Instance != null && GameController.Instance.playerData != null)
        {
            GameController.Instance.playerData.ActiveQuests.OnChanged += HandleQuestsChanged;
            CheckQuestsStatus(GameController.Instance.playerData.ActiveQuests.Value);
        }
    }

    private void OnDisable()
    {
        if (GameController.Instance != null && GameController.Instance.playerData != null)
        {
            GameController.Instance.playerData.ActiveQuests.OnChanged -= HandleQuestsChanged;
        }
    }

    private void HandleQuestsChanged(List<DailyQuestData> quests)
    {
        CheckQuestsStatus(quests);
    }

    private void CheckQuestsStatus(List<DailyQuestData> quests)
    {
        var playerData = GameController.Instance.playerData;
        bool hasUncompletedQuests = quests != null && quests.Exists(q => !q.isCompleted);

        bool shouldHighlight = hasUncompletedQuests && !playerData.QuestsViewed;

        SetButtonVisualState(shouldHighlight);
    }

    private void SetButtonVisualState(bool highlight)
    {
        if (buttonImage != null)
        {
            var colors = _button.colors;
            colors.normalColor = highlight ? questAvailableColor : normalColor;
            colors.highlightedColor = highlightedColor;
            _button.colors = colors;
        }

        if (notificationBadge != null)
        {
            notificationBadge.SetActive(highlight);
        }
    }
}