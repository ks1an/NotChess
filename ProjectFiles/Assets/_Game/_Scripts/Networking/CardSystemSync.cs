using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public sealed class CardSystemSync : NetworkBehaviour
{
    EnemyDeck EnemyDeck;
    private void OnEnable()
    {
        EnemyDeck = EnemyDeck.Instance;
    }

    [Rpc(SendTo.NotMe)]
    public void UseCardRpc(int cardId, int[] movesX, int[] movesY)
    {
        GameController.Instance.globalCardCollection.GlobalCardsDictionary.TryGetValue(cardId, out Card card);
        if (card != null)
        {
            Card cardInScene = Instantiate(card.gameObject).GetComponent<Card>();

            List<Vector2Int> moves = new();
            for (int i = 0; i < movesX.Length; i++)
                moves.Add(new Vector2Int(movesX[i], movesY[i]));

            cardInScene.UseCard(moves, true);
        }
        else
            Debug.LogError($"Card with ID:({cardId}) in not find! Error sync.");
    }

    [Rpc(SendTo.NotMe)]
    public void SetDefaultRpc() => EnemyDeck.SetDefaultSettings();


    [Rpc(SendTo.NotMe)]
    public void DestroyAllRpc() => EnemyDeck.DestroyAllCard();
    [Rpc(SendTo.NotMe)]
    public void DestroyCardRpc() => EnemyDeck.DestroyCardInHand(EnemyDeck.GetRandomCardFromHand());

    [Rpc(SendTo.NotMe)]
    public void DestroyAllCardsInHandRpc(bool b) => EnemyDeck.DestroyAllCardsInHand(b);


    [Rpc(SendTo.NotMe)]
    public void AddToDeckRpc(int count) => EnemyDeck.AddToDeck(count);
    [Rpc(SendTo.NotMe)]
    public void AddToGraveyardRpc(int count) => EnemyDeck.AddToGraveyard(count);

    [Rpc(SendTo.NotMe)]
    public void DrawInHandRpc() => EnemyDeck.DrawCardInHand(EnemyDeck.GetCardBack());

    [Rpc(SendTo.NotMe)]
    public void DrawHandRandomFromDeckRpc(int count, bool ignoreCardsLimit) => EnemyDeck.DrawHandRandomFromDeck(count, ignoreCardsLimit);

    [Rpc(SendTo.NotMe)]
    public void DrawLastFromGraveyardRpc() => EnemyDeck.DrawLastFromGraveyard();

}
