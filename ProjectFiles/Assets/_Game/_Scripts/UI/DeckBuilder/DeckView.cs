using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DeckView : MonoBehaviour
{
    void OnEnable()
    {
        PlayerDeck.Instance.cardCollectionFromSave.cardBack.TryGetComponent<SpriteRenderer>(out SpriteRenderer render);
        if (render != null)
            GetComponent<Image>().sprite = render.sprite;
    }
}
