using UnityEngine;

public class ResourceViewGroup : MonoBehaviour
{
    [SerializeField] ResourceView magicCrystacleView;

    private void OnEnable()
    {
        if (magicCrystacleView != null)
        {
            GameController.Instance.playerData.MagicCrystals.OnChanged += UpdateMagicCrystalsView;
            UpdateMagicCrystalsView(GameController.Instance.playerData.MagicCrystals.Value);
        }
    }

    private void OnDisable()
    {
        if (magicCrystacleView != null)
        {
            GameController.Instance.playerData.MagicCrystals.OnChanged -= UpdateMagicCrystalsView;
        }
    }

    private void UpdateMagicCrystalsView(int newValue)
    {
        if (magicCrystacleView != null)
        {
            magicCrystacleView.UpdateText(newValue);
        }
    }
}
