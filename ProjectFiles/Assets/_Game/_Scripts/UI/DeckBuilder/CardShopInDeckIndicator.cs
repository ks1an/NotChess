using UnityEngine;

public class CardShopInDeckIndicator : MonoBehaviour
{
    Card card;
    CardsInDeckViewModel viewModel;

    public void SetCard(Card card, CardsInDeckViewModel vm)
    {
        this.card = card;
        viewModel = vm;
    }

    public void OnClick() => viewModel.RemoveCard(card);
}
