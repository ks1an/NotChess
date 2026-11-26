using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuestItemView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject completedCheckmark;
    [SerializeField] private Image backgroundPanel, avatarImage;

    [SerializeField] Transform rewardContent;
    [SerializeField] ResourceView rewardViewTemplate;

    public void Setup(DailyQuestItemViewModel vm)
    {
        if (descriptionText != null)
            descriptionText.text = vm.Description;

        if (progressText != null)
            progressText.text = $"{vm.CurrentProgress} / {vm.TargetProgress}";

        if (progressBar != null)
            progressBar.value = vm.ProgressNormalized;

        if (completedCheckmark != null)
            completedCheckmark.SetActive(vm.IsCompleted);

        if (avatarImage != null)
        {
            Sprite questSprite = vm.SpriteOfQuest;

            if (questSprite != null)
            {
                avatarImage.sprite = questSprite;
                avatarImage.gameObject.SetActive(true);
            }
            else
            {
                avatarImage.gameObject.SetActive(false);
            }
        }

        if (rewardContent != null)
        {
            if (vm.rewards.Count > 0)
            {
                rewardContent.gameObject.SetActive(true);
                foreach (QuestReward reward in vm.rewards)
                {
                    ResourceView resourceReward = Instantiate(rewardViewTemplate, rewardContent).GetComponent<ResourceView>();
                    resourceReward.SetResourceView(reward.sprite, reward.amount, reward.resourceType, true);
                }
            }
            else
                rewardContent.gameObject.SetActive(false);
        }
    }
}