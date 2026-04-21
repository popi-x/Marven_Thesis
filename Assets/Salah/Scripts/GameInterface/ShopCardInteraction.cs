using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to each shop card slot object.
/// Handles hover (scale + lift) and click (open preview).
/// ShopUIController adds this automatically — no manual setup needed.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ShopCardInteraction : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hover")]
    [SerializeField] private float hoverScale    = 1.08f;
    [SerializeField] private float liftY         = 5f;
    [SerializeField] private float hoverDuration = 0.1f;

    // Set by ShopUIController each time the slot is refreshed
    [HideInInspector] public EventCardData cardData;
    [HideInInspector] public int           slotIndex;

    private RectTransform _rt;
    private Vector3       _baseScale;
    private Vector2       _basePos;
    private Coroutine     _routine;

    private void Awake()
    {
        _rt         = GetComponent<RectTransform>();
        _baseScale  = transform.localScale;   // read whatever scale was set in the scene
        _basePos    = _rt.anchoredPosition;
    }

    // Called by ShopUIController whenever this slot gets new card data
    public void Init(EventCardData data, int index)
    {
        cardData  = data;
        slotIndex = index;
        ResetVisual();
    }

    public void ResetVisual()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        _basePos             = _rt.anchoredPosition;
        transform.localScale = _baseScale;    // restore to scene-defined scale, not hardcoded 1
        _rt.anchoredPosition = _basePos;
    }

    // ── Pointer events ────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData e)
        => Animate(_baseScale * hoverScale, _basePos + new Vector2(0f, liftY));

    public void OnPointerExit(PointerEventData e)
        => Animate(_baseScale, _basePos);

    public void OnPointerClick(PointerEventData e)
    {
        if (cardData == null) return;
        ShopPreviewController.instance?.OpenPreview(cardData, slotIndex);
    }

    // ── Animation ─────────────────────────────────────────────────────────────

    private void Animate(Vector3 targetScale, Vector2 targetPos)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(AnimRoutine(targetScale, targetPos));
    }

    private IEnumerator AnimRoutine(Vector3 targetScale, Vector2 targetPos)
    {
        Vector3 startScale = transform.localScale;
        Vector2 startPos   = _rt.anchoredPosition;

        for (float t = 0f; t < hoverDuration; t += Time.deltaTime)
        {
            float p = Mathf.Clamp01(t / hoverDuration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, p);
            _rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, p);
            yield return null;
        }

        transform.localScale = targetScale;
        _rt.anchoredPosition = targetPos;
        _routine             = null;
    }
}
