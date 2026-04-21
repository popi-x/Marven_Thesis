using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DropZone : MonoBehaviour, IDropHandler
{
    [Tooltip("Maximum cards allowed in this zone. 0 = unlimited.")]
    [SerializeField] protected int maxCards = 1;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = eventData.pointerDrag;
        if (dragged == null) return;

        CardViewUI cardUI = dragged.GetComponent<CardViewUI>();
        DraggableCard draggable = dragged.GetComponent<DraggableCard>();

        if (cardUI == null || draggable == null) return;

        // Card is already locked in a play slot — OnBeginDrag returned early so it never
        // actually moved, but Unity still fires OnDrop. Accepting it would call PlayCard()
        // again and permanently stack mult/plus. Silently ignore.
        if (draggable.IsLockedInSlot) return;

        // 0 = unlimited. Otherwise reject when at capacity.
        if (maxCards > 0 && transform.childCount >= maxCards)
        {
            draggable.ReturnWithFeedback();
            return;
        }

        if (!CanAccept(cardUI.Data))
        {
            draggable.ReturnWithFeedback();
            return;
        }

        Accept(cardUI, draggable);
    }

    protected abstract bool CanAccept(CardData data);
    protected abstract void Accept(CardViewUI cardUI, DraggableCard draggable);
}
