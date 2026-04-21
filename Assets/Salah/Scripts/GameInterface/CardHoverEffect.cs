using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler
{
    [Tooltip("How much bigger the card gets on hover. 1.12 = 12% larger.")]
    [SerializeField] private float hoverScale = 1.12f;

    [Tooltip("How fast the scale animation plays, in seconds.")]
    [SerializeField] private float duration = 0.08f;

    // NOT stored in Awake — captured fresh each time hover starts,
    // so it correctly reflects whatever scale CardCellAdapter has set.
    private Vector3 _baseScale;
    private Coroutine _routine;

    // HandCardHover handles all hover behaviour while the card is in the hand.
    // This effect should only run in other contexts (play area, shop, etc.).
    private bool IsInHand => transform.parent?.GetComponent<HandFanLayout>() != null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsInHand) return;   // HandCardHover owns hover in the hand

        // Capture the current resting scale right now (e.g. 0.5 in play area, 1 in hand).
        _baseScale = transform.localScale;
        ScaleTo(hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsInHand) return;
        ScaleTo(1f);
    }

    // Reset to base scale when drag starts so the card doesn't fly at hover size.
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsInHand) return;
        ScaleTo(1f);
    }

    private void ScaleTo(float target)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ScaleRoutine(_baseScale * target));
    }

    private IEnumerator ScaleRoutine(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }

        transform.localScale = target;
        _routine = null;
    }
}
