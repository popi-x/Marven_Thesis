using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class HandCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler
{
    [Tooltip("How many pixels the card rises on hover.")]
    [SerializeField] private float liftY = 50f;

    [Tooltip("Scale multiplier on hover (e.g. 1.12 = 12% larger).")]
    [SerializeField] private float hoverScale = 1.12f;

    [Tooltip("Duration of all animations in seconds.")]
    [SerializeField] private float duration = 0.1f;

    // True while hovered OR while animating back — HandFanLayout skips this card during both.
    public bool IsControlled { get; private set; }

    private RectTransform    _rt;
    private Canvas           _overrideCanvas;    // used for z-order — avoids sibling reordering
    private GraphicRaycaster _overrideRaycaster; // keeps pointer events alive on the nested Canvas
    private Coroutine        _routine;
    private Vector2          _restPos;           // card's position at hover-start (set by GridLayoutGroup)

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();

        // Pre-create BOTH components immediately so there is no difference between the
        // first hover and every subsequent hover.  Adding them lazily (inside
        // OnPointerEnter) caused Unity to re-register the card's Graphics to the new
        // Canvas mid-frame, producing a small but permanent position shift on first hover.
        //
        // overrideSorting starts false → no visual change until we actually need z-boost.
        // GraphicRaycaster must accompany the Canvas so the card's own nested Canvas
        // handles pointer events (nested Canvas without a raycaster = card goes dead).
        _overrideCanvas            = gameObject.AddComponent<Canvas>();
        _overrideCanvas.overrideSorting = false;
        _overrideRaycaster         = gameObject.AddComponent<GraphicRaycaster>();
    }

    // Only active when the DIRECT parent has HandFanLayout (i.e. card is in the hand).
    private HandFanLayout GetFan() => transform.parent?.GetComponent<HandFanLayout>();

    // ── Pointer events ────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        var fan = GetFan();
        if (fan == null) return;   // not in hand — CardHoverEffect handles other contexts

        IsControlled = true;
        SetOnTop(true);

        // Capture current anchoredPosition (set by GridLayoutGroup) as the rest position.
        // Do NOT use fan.CalculatePosition — that returns positions in a different coordinate
        // space and would move the card to the wrong place on first hover.
        _restPos = _rt.anchoredPosition;

        Animate(_restPos + new Vector2(0f, liftY), Quaternion.identity, hoverScale, releaseOnDone: false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsControlled) return;

        SetOnTop(false);

        var   fan     = GetFan();
        int   index   = transform.GetSiblingIndex();
        int   total   = transform.parent != null ? transform.parent.childCount : 1;
        float restRot = fan != null ? fan.CalculateRotation(index, total) : 0f;

        // Return to _restPos (captured at hover-start from GridLayoutGroup) — NOT a
        // fan.CalculatePosition value, which lives in a different coordinate space.
        Animate(_restPos, Quaternion.Euler(0f, 0f, restRot), 1f, releaseOnDone: true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // DraggableCard takes over from here
        IsControlled = false;
        SetOnTop(false);
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
    }

    // ── Z-order via Canvas override (no sibling reordering = no index shuffle) ─

    private void SetOnTop(bool onTop)
    {
        // Both components are guaranteed to exist (created in Awake).
        if (onTop)
        {
            _overrideCanvas.overrideSorting = true;
            _overrideCanvas.sortingOrder    = 10;
        }
        else
        {
            _overrideCanvas.overrideSorting = false;
        }
    }

    // ── Animation ─────────────────────────────────────────────────────────────

    private void Animate(Vector2 targetPos, Quaternion targetRot, float targetScale, bool releaseOnDone)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(AnimRoutine(targetPos, targetRot, targetScale, releaseOnDone));
    }

    private IEnumerator AnimRoutine(Vector2 targetPos, Quaternion targetRot, float targetScale, bool releaseOnDone)
    {
        Vector2    startPos   = _rt.anchoredPosition;
        Quaternion startRot   = transform.localRotation;
        Vector3    startScale = transform.localScale;
        Vector3    endScale   = Vector3.one * targetScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / duration);
            _rt.anchoredPosition = Vector2.Lerp   (startPos,   targetPos,  lerp);
            transform.localRotation  = Quaternion.Lerp(startRot,   targetRot,  lerp);
            transform.localScale     = Vector3.Lerp   (startScale, endScale,   lerp);
            yield return null;
        }

        _rt.anchoredPosition     = targetPos;
        transform.localRotation  = targetRot;
        transform.localScale     = endScale;

        if (releaseOnDone)
        {
            IsControlled = false;
            // _dirty on HandFanLayout is only set when children change — it stays false
            // during a pure hover cycle, so Refresh() would never auto-correct drift.
            // Force it here so the fan is always the final authority on resting position.
            GetFan()?.Refresh();
        }

        _routine = null;
    }
}
