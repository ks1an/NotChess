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
    public void UseCardRpc(int cardId, int[] movesX, int[] movesY, int teamWhoUsed)
    {
        GameController.Instance.globalCards.GlobalCardsDictionary.TryGetValue(cardId, out Card card);
        if (card != null)
        {
            Card cardInScene = Instantiate(card.gameObject).GetComponent<Card>();

            List<Vector2Int> moves = new();
            for (int i = 0; i < movesX.Length; i++)
                moves.Add(new Vector2Int(movesX[i], movesY[i]));

            cardInScene.Init((Team)teamWhoUsed);
            cardInScene.UseCard(moves, true);
        }
        else
            Debug.LogError($"Card with ID:({cardId}) in not find! Error sync.");
    }

    [Rpc(SendTo.NotMe)]
    public void ShowSpyInfoAboutCardRpc(int cardID)
    {
        NotificationPanelConroller.Instance.ShowNotification(
            GameController.Instance.globalCards.GetImortantWordsFromDescription(cardID), () => { });
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
    public void Enemy_DestroyCardRpc(int cardIndex, bool needToGravejard) => 
        enemyDeck.DestroyCardInHand(EnemyDeck.Instance.hand.CardGameobjectsInHand[cardIndex], needToGravejard);

    [Rpc(SendTo.NotMe)]
    public void Enemy_DestroyAllCardsInHandRpc(bool b) => enemyDeck.DestroyAllCardsIn(b);

    [Rpc(SendTo.NotMe)]
    public void Enemy_AddToDeckRpc(int[] cardsIdAdded, bool needShuffle) => enemyDeck.AddCardsToDeck(cardsIdAdded, needShuffle);
    [Rpc(SendTo.NotMe)]
    public void Enemy_AddToDeckViewRpc(int count) => enemyDeck.AddToDeckView(count);    
    [Rpc(SendTo.NotMe)]
    public void Enemy_AddToGraveyardRpc(int count) => enemyDeck.AddToGraveyardMirror(count);

    [Rpc(SendTo.NotMe)]//TODO: NeedChange
    public void Enemy_DrawHandFromDeckRpc(int[] cardsIDs, bool ignoreCardsLimit) => enemyDeck.DrawHandFromDeck(cardsIDs, ignoreCardsLimit);

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
