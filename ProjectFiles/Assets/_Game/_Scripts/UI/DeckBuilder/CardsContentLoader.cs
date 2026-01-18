using UnityEngine;

public class CardsView : MonoBehaviour
{
    [SerializeField] CardShopView cardTemplate;
    [SerializeField] GameObject deckTitle;
    [SerializeField] Transform container;

    private void OnEnable()
    {
        cardTemplate.gameObject.SetActive(false);
        foreach (Card cardData in GameController.Instance.globalCards.GlobalCardsDictionary.Values)
        {
            CardShopView cardView = GameObject.Instantiate(cardTemplate, container);
            cardView.SetCardData(cardData);

            cardView.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        foreach (Transform child in container)
        {
            if (cardTemplate.transform == child || deckTitle.transform == child) continue;
            Destroy(child.gameObject);
        }
    }
}
