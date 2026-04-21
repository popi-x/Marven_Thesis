using UnityEngine;

public class ItemPlaySlotDropZone : DropZone
{
    protected override bool CanAccept(CardData data) => data is ItemCardData;

    protected override void Accept(CardViewUI cardUI, DraggableCard draggable)
    {
        var lm = LevelManager.instance;
        var card = draggable.GetComponent<Card>();

        // Remove from player deck immediately so HandUIController.Rebuild()
        // does not re-spawn this card as a duplicate in the hand.
        if (card?.cardData is ItemCardData ic)
            Player.instance.itemCardDeck.Remove(ic);

        draggable.PlaceIntoSlot(transform);
        draggable.LockInSlot();
        draggable.SnapBounce();

        if (lm != null && card != null)
            lm.PlayCard(card);

        // PlaceIntoSlot sets localScale = Vector3.one after parenting the card,
        // which overrides the scale CardCellAdapter set in OnTransformChildrenChanged.
        // Calling Refresh() here corrects it after PlaceIntoSlot fully finishes.
        GetComponent<CardCellAdapter>()?.Refresh();
    }
}
