using UnityEngine;
using UnityEngine.EventSystems;

// Add this to the card prefab alongside DraggableCard.
// When the player clicks a card that is locked inside a play area (CardContent),
// it unlocks and smoothly returns to the hand.
public class PlayedCardClickReturn : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click only
        if (eventData.button != PointerEventData.InputButton.Right) return;

        var draggable = GetComponent<DraggableCard>();

        // Only act on locked cards (i.e. cards sitting in CardContent, not in hand/shop)
        if (draggable == null || !draggable.IsLockedInSlot) return;

        // Extra safety: parent must have CardCellAdapter (confirms we are in a play area)
        if (GetComponentInParent<CardCellAdapter>() == null) return;

        ReturnToHand(draggable);
    }

    private void ReturnToHand(DraggableCard draggable)
    {
        var lm  = LevelManager.instance;
        var card = GetComponent<Card>();
        if (card == null) return;

        // Remove from the played list and give the data back to the player's deck
        // so Rebuild() knows about it again.
        if (card.cardData is EventCardData ec)
        {
            lm?.playedEventCards.Remove(ec);
            Player.instance.eventCardDeck.Add(ec);
        }
        else if (card.cardData is ItemCardData ic)
        {
            lm?.playedItemCards.Remove(ic);
            Player.instance.itemCardDeck.Add(ic);
        }

        // Recalculate mult/plus now that this card is no longer played
        lm?.RecalculateAccumulators();

        // Unlock so the smooth return coroutine is allowed to run, then animate
        draggable.UnlockFromSlot();
        draggable.ReturnToOriginSmooth();
    }
}
