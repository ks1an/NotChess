using UnityEngine;

public class CardShopInDeckIndicator : MonoBehaviour
{
    Card card;
    CardsInDeckViewModel viewModel;

    public void SetCard(Card card, CardsInDeckViewModel viewModel)
    {
        this.card = card;
        this.viewModel = viewModel;
    }

    public void OnClick() => viewModel.RemoveCard(card);
}
