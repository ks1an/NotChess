using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DeckView : MonoBehaviour
{
    [HideInInspector] public GameObject deckIcon;

    void OnEnable()
    {
        deckIcon = GameController.Instance.player.cardCollection.cardBack;
        ShowDeckIcon();
    }

    void ShowDeckIcon()
    {
        deckIcon.TryGetComponent<SpriteRenderer>(out SpriteRenderer render);
        if (render != null)
            GetComponent<Image>().sprite = render.sprite;
        else
            Debug.LogError("Icon: " + deckIcon + " without sprite renderer!");
    }

    public void NextIcon()
    {
        int curIndex = GameController.Instance.globalCards.GlobalCardBacks.IndexOf(deckIcon);
        curIndex++;
        if (curIndex >= GameController.Instance.globalCards.GlobalCardBacks.Count)
            curIndex = 0;

        deckIcon = GameController.Instance.globalCards.GlobalCardBacks[curIndex];
        ShowDeckIcon();
    }
}
