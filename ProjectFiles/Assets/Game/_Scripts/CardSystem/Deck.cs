using DG.Tweening;
using UnityEngine;

public sealed class Deck : MonoBehaviour
{
    public static Deck Instance { get; private set; }

    public CardHand playerHand;
    [SerializeField] CardCollection playerDeck;
    

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    #region +- Cards
    public void DrawHand(int amount)
    {
        if (playerHand.CardsInHand.Count == MatchController.Instance.settings.maxCardsInHand)
            return;

        for (int i = 0; i < amount; i++)
        {
            Card card = playerDeck.CardsInCollection[Random.Range(0, playerDeck.CardsInCollection.Count)];

            Card newCard = Instantiate(card.gameObject, parent: playerHand.transform).GetComponent<Card>();
            newCard.gameObject.transform.localScale = Vector3.zero;
            newCard.gameObject.transform.DOScale(Vector3.one, 0.15f);
            StartCoroutine(playerHand.AddCard(newCard));

            newCard.Init();
        }
    }

    public void DestroyCard(Card card)
    {
        Destroy(card.gameObject);
        StartCoroutine(playerHand.RemoveCard(card));
    }

    public void DestroyAllCard()
    {
        for (int i = 0; i < playerHand.CardsInHand.Count; i++)
            Destroy(playerHand.CardsInHand[i].gameObject);
        playerHand.RemoveAllCards();
    }
    #endregion
}
