using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public partial class CardSystemSync : NetworkBehaviour
{
    void OnEnable()
    {
        playerDeck = PlayerDeck.Instance;
        enemyDeck = EnemyDeck.Instance;
    }

    [Rpc(SendTo.NotMe)]
    public void UseCardRpc(int cardId, int[] movesX, int[] movesY)
    {
        GameController.Instance.globalCards.GlobalCardsDictionary.TryGetValue(cardId, out Card card);
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
    public void ShowSpyInfoAboutCardRpc(int cardID)
    {
        NotificationPanelConroller.Instance.ShowNotification(
            GameController.Instance.globalCards.GetImortantWordsFromDescriptionOfCard(cardID), () => { });
    }
}

//DoSomeWithEnemyCards
public partial class CardSystemSync : NetworkBehaviour
{
    EnemyDeck enemyDeck;

    [Rpc(SendTo.NotMe)]
    public void Enemy_SetDefaultRpc() => enemyDeck.SetDefaultSettings();

    [Rpc(SendTo.NotMe)]
    public void Enemy_DestroyAllRpc() => enemyDeck.DestroyAllCard();
    [Rpc(SendTo.NotMe)]
    public void Enemy_DestroyCardRpc() => enemyDeck.DestroyCardInHand(enemyDeck.GetRandomCardFromHand());

    [Rpc(SendTo.NotMe)]
    public void Enemy_DestroyAllCardsInHandRpc(bool b) => enemyDeck.DestroyAllCardsIn(b);


    [Rpc(SendTo.NotMe)]
    public void Enemy_AddToDeckRpc(int count) => enemyDeck.AddToDeck(count);
    [Rpc(SendTo.NotMe)]
    public void Enemy_AddToGraveyardRpc(int count) => enemyDeck.AddToGraveyardMirror(count);

    [Rpc(SendTo.NotMe)]
    public void Enemy_DrawHandRandomFromDeckRpc(int count, bool ignoreCardsLimit) => enemyDeck.DrawHandRandomFromDeck(count, ignoreCardsLimit);

    [Rpc(SendTo.NotMe)]
    public void Enemy_DrawLastFromGraveyardRpc() => enemyDeck.DrawLastFromGraveyard();

    [Rpc(SendTo.NotMe)]
    public void Enemy_CardHandUpDownMoveRpc(int cardIndex, bool toUp, bool liftSlightly) =>
        enemyDeck.hand.CardUpDownMove(cardIndex, toUp, liftSlightly);
}

//DoSomeWithPlayerCards
public partial class CardSystemSync : NetworkBehaviour
{
    PlayerDeck playerDeck;

    [Rpc(SendTo.NotMe)]
    public void Player_DrawCardInHandRpc(int cardID)
    {
        playerDeck.DrawCardInHand(GameController.Instance.globalCards.GlobalCardsDictionary[cardID]);
    }
}
