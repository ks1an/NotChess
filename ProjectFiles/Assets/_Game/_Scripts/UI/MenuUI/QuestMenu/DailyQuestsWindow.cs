using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuestsWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform questsCommonContainer;
    [SerializeField] private Transform hardQuestsContainer;
    [SerializeField] private GameObject questItemPrefab_common, questItemPrefab_hard;
    [SerializeField] private Button closeButton;

    [SerializeField] MenuUI menu;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseWindow);
    }

    private void OnEnable()
    {
        RefreshUI();

        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.MarkQuestsAsViewed();
        }
    }

    public void RefreshUI()
    {
        ClearContainer(questsCommonContainer);
        ClearContainer(hardQuestsContainer);

        var quests = GameController.Instance.playerData.ActiveQuests.Value;
        if (quests == null || quests.Count == 0) return;

        if (questItemPrefab_hard != null)
        {
            var hardQuests = quests.Where(q => q.tier == QuestTier.Hard);
            foreach (var quest in hardQuests)
            {
                if (hardQuestsContainer == null) continue;
                var vm = new DailyQuestItemViewModel(quest);
                GameObject obj = Instantiate(questItemPrefab_hard, hardQuestsContainer);
                if (obj.TryGetComponent<DailyQuestItemView>(out var view))
                {
                    view.Setup(vm);
                }
            }
        }

        if (questItemPrefab_common != null)
        {
            var commonQuests = quests
    .Where(q => q.tier != QuestTier.Hard)
    .OrderBy(q => (int)q.tier)
    .ThenBy(q => q.isCompleted);

            foreach (var quest in commonQuests)
            {
                if (questsCommonContainer == null) continue;
                var vm = new DailyQuestItemViewModel(quest);

                GameObject obj = Instantiate(questItemPrefab_common, questsCommonContainer);
                if (obj.TryGetComponent<DailyQuestItemView>(out var view))
                {
                    view.Setup(vm);
                }
            }
        }
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    public void CloseWindow()
    {
        menu.SetActiveQuestMenu(false);
        menu.SetActiveMainMenuUI(true);
    }
}