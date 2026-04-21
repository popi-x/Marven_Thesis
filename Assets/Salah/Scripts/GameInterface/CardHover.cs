using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Unified hover handler — replaces both HandCardHover and CardHoverEffect.
///
/// In the hand (parent has HandFanLayout):
///   • Lifts the card upward + scales it.
///   • Uses a nested Canvas to bring it to front without shuffling sibling order.
///   • Sets IsControlled so HandFanLayout skips repositioning during animation.
///
/// Everywhere else (play area, shop, …):
///   • Scale-only hover — no lift, no Canvas z-trick.
///
/// Swap this onto the card prefab in place of HandCardHover + CardHoverEffect.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler
{
    [Header("Shared")]
    [Tooltip("Scale multiplier on hover (1.12 = 12% larger).")]
    [SerializeField] private float hoverScale = 1.12f;
    [Tooltip("Duration of all hover animations in seconds.")]
    [SerializeField] private float duration = 0.1f;

    [Header("Hand only")]
    [Tooltip("How many pixels the card rises when hovered in the hand.")]
    [SerializeField] private float liftY = 50f;

    /// <summary>
    /// True while the card is hovered OR animating back to rest.
    /// HandFanLayout reads this to skip repositioning during the animation.
    /// </summary>
    public bool IsControlled { get; private set; }

    private RectTransform    _rt;
    private Canvas           _overrideCanvas;
    private GraphicRaycaster _overrideRaycaster;
    private Coroutine        _routine;

    // Hand hover: the anchoredPosition at hover-start (set by GridLayoutGroup).
    private Vector2 _restPos;

    // Non-hand hover: the localScale at hover-start (set by CardCellAdapter / parent layout).
    private Vector3 _baseScale;

    private HandFanLayout GetFan()  => transform.parent?.GetComponent<HandFanLayout>();
    private bool          IsInHand => GetFan() != null;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();

        // Pre-create Canvas + GraphicRaycaster in Awake (not lazily on first hover).
        // Lazy creation caused Unity to re-register Graphics mid-frame → 1-frame
        // position shift on first hover that became permanent.
        // overrideSorting starts false → invisible until we actually need the z-boost.
        _overrideCanvas                 = gameObject.AddComponent<Canvas>();
        _overrideCanvas.overrideSorting = false;
        _overrideRaycaster              = gameObject.AddComponent<GraphicRaycaster>();
    }

    // ── Pointer events ────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsInHand)
        {
            IsControlled = true;
            SetOnTop(true);

            // Capture the GridLayoutGroup position now — not fan.CalculatePosition,
            // which operates in a different coordinate space.
            _restPos = _rt.anchoredPosition;

            AnimateFull(_restPos + new Vector2(0f, liftY),
                        Quaternion.identity, hoverScale, releaseOnDone: false);
        }
        else
        {
            // Capture scale set by CardCellAdapter / parent layout.
            _baseScale = transform.localScale;
            AnimateScale(_baseScale * hoverScale);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsInHand)
        {
            if (!IsControlled) return;
            SetOnTop(false);

            var   fan     = GetFan();
            int   index   = transform.GetSiblingIndex();
            int   total   = transform.parent != null ? transform.parent.childCount : 1;
            float restRot = fan != null ? fan.CalculateRotation(index, total) : 0f;

            // Return to the captured rest position, not a recalculated fan position.
            AnimateFull(_restPos, Quaternion.Euler(0f, 0f, restRot), 1f, releaseOnDone: true);
        }
        else
        {
            // _baseScale is zero if OnPointerEnter never fired (e.g. rapid drag).
            // Guard against scaling the card to nothing.
            if (_baseScale != Vector3.zero)
                AnimateScale(_baseScale);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsInHand)
        {
            IsControlled = false;
            SetOnTop(false);
        }
        else
        {
            // Reset to base scale so the card isn't dragged at hover size.
            if (_baseScale != Vector3.zero)
                AnimateScale(_baseScale);
        }
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
    }

    /// <summary>
    /// Called by DraggableCard when the card is programmatically returned to hand.
    /// Cancels any in-flight hover animation and clears controlled state so
    /// HandFanLayout.Refresh() can position the card immediately.
    /// </summary>
    public void ResetHoverState()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        IsControlled = false;
        SetOnTop(false);
        _baseScale = Vector3.zero;
    }

    /// <summary>
    /// Locks this card so HandFanLayout skips it during an external animation
    /// (e.g. the level intro spawn sequence). Call ResetHoverState() to release.
    /// </summary>
    public void LockForAnimation()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        IsControlled = true;
    }

    // ── Z-order (hand only) ────────────────────────────────────────────────────

    private void SetOnTop(bool onTop)
    {
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

    /// Position + rotation + scale — used in the hand.
    private void AnimateFull(Vector2 targetPos, Quaternion targetRot, float targetScale, bool releaseOnDone)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(FullRoutine(targetPos, targetRot, Vector3.one * targetScale, releaseOnDone));
    }

    /// Scale only — used outside the hand.
    private void AnimateScale(Vector3 targetScale)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ScaleRoutine(targetScale));
    }

    private IEnumerator FullRoutine(Vector2 targetPos, Quaternion targetRot, Vector3 targetScale, bool releaseOnDone)
    {
        Vector2    startPos   = _rt.anchoredPosition;
        Quaternion startRot   = transform.localRotation;
        Vector3    startScale = transform.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / duration);
            _rt.anchoredPosition    = Vector2.Lerp   (startPos,   targetPos,   lerp);
            transform.localRotation = Quaternion.Lerp(startRot,   targetRot,   lerp);
            transform.localScale    = Vector3.Lerp   (startScale, targetScale, lerp);
            yield return null;
        }

        _rt.anchoredPosition    = targetPos;
        transform.localRotation = targetRot;
        transform.localScale    = targetScale;

        if (releaseOnDone)
        {
            IsControlled = false;
            // HandFanLayout._dirty is only set by children-changed events, not by hover cycles.
            // Force a refresh here so the fan immediately snaps the card to its exact resting
            // position/rotation rather than waiting until the next dirty cycle.
            GetFan()?.Refresh();
        }
        _routine = null;
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        Vector3 start = transform.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, targetScale, Mathf.Clamp01(t / duration));
            yield return null;
        }

        transform.localScale = targetScale;
        _routine = null;
    }
}
