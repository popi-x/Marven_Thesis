using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform OriginalParent { get; private set; }
    public int OriginalSiblingIndex { get; private set; }
    public bool IsLockedInSlot { get; private set; } = false;

    [Header("Smooth Return")]
    [SerializeField] private float returnDuration = 0.2f;

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Canvas _rootCanvas;
    private Coroutine _moveRoutine;

    private GameObject _placeholder;
    private LayoutElement _placeholderLayout;

    // World position of the card at the moment dragging started.
    // Used as the animation target on return when no placeholder exists
    // (i.e. the card was dropped into a play slot and is being click-returned).
    private Vector3 _dragStartWorldPos;

    // Anchor values saved at drag-start (before GridLayoutGroup in CardContent overwrites them).
    // Restored on return so anchoredPosition is interpreted in the correct coordinate space.
    private Vector2 _originalAnchorMin;
    private Vector2 _originalAnchorMax;

    [Header("Drop Feedback")]
    [SerializeField] private float shakeDuration  = 0.2f;
    [SerializeField] private float shakeAmount    = 5f;
    [SerializeField] private float bounceDuration = 0.15f;
    [SerializeField] private float bounceScale    = 1.1f;

    private bool _feedbackActive; // prevents OnEndDrag double-returning after shake

    private void Awake()
    {
        _canvasGroup   = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();

        // GetComponentInParent finds the NEAREST Canvas, which may be the card's own
        // nested Canvas (added by HandCardHover.Awake). We want the ROOT canvas, so
        // take the last element — GetComponentsInParent returns nearest-first.
        var canvases = GetComponentsInParent<Canvas>(true);
        _rootCanvas  = (canvases != null && canvases.Length > 0)
            ? canvases[canvases.Length - 1]
            : null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Left-drag on a locked card (in CardContent) — unlock it and drag back to hand
        if (IsLockedInSlot)
        {
            UnlockFromCardContent();
            // OriginalParent already points to the hand from the original drag — keep it.
            // Create a placeholder in the hand so ReturnToOriginSmooth knows where to land.
            CreatePlaceholder();
            transform.SetParent(_rootCanvas.transform, true);
            transform.SetAsLastSibling();
            transform.localRotation = Quaternion.identity;
            _canvasGroup.blocksRaycasts = false;
            return;
        }

        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);

        OriginalParent = transform.parent;
        OriginalSiblingIndex = transform.GetSiblingIndex();
        _dragStartWorldPos = transform.position;
        _originalAnchorMin = _rectTransform.anchorMin;
        _originalAnchorMax = _rectTransform.anchorMax;

        CreatePlaceholder();

        transform.SetParent(_rootCanvas.transform, true);
        transform.SetAsLastSibling();

        // Reset any fan tilt so the card looks straight while being dragged
        transform.localRotation = Quaternion.identity;

        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsLockedInSlot)
            return;

        _rectTransform.anchoredPosition += eventData.delta / _rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsLockedInSlot) return;

        _canvasGroup.blocksRaycasts = true;

        // If still under canvas root, no valid drop zone accepted it
        // and feedback hasn't already started (wrong-drop shake handles its own return)
        if (transform.parent == _rootCanvas.transform && !_feedbackActive)
            ReturnToOriginSmooth();
    }

    public void ReturnToOriginSmooth()
    {
        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);

        _feedbackActive = false;
        _moveRoutine = StartCoroutine(SmoothReturnRoutine());
    }

    /// <summary>
    /// Called by DropZone on wrong drop: shake + red tint flash, then return to hand.
    /// </summary>
    public void ReturnWithFeedback()
    {
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _feedbackActive = true;
        _moveRoutine    = StartCoroutine(ShakeFeedbackRoutine());
    }

    /// <summary>
    /// Called by drop zones on valid accept: quick scale bounce 1 → 1.1 → 1.
    /// </summary>
    public void SnapBounce()
    {
        StartCoroutine(BounceRoutine());
    }

    private IEnumerator ShakeFeedbackRoutine()
    {
        var   image         = GetComponent<Image>();
        Color originalColor = image != null ? image.color : Color.white;
        Vector3 startPos    = transform.localPosition;

        for (float t = 0f; t < shakeDuration; t += Time.deltaTime)
        {
            // Shake: oscillate left/right
            float xOffset = Mathf.Sin(t / shakeDuration * Mathf.PI * 6f) * shakeAmount;
            transform.localPosition = startPos + new Vector3(xOffset, 0f, 0f);

            // Red flash: peaks at midpoint
            if (image != null)
            {
                float red = Mathf.Sin(t / shakeDuration * Mathf.PI); // 0→1→0
                image.color = Color.Lerp(originalColor, Color.red, red * 0.5f);
            }
            yield return null;
        }

        transform.localPosition = startPos;
        if (image != null) image.color = originalColor;

        _feedbackActive = false;
        ReturnToOriginSmooth();
    }

    private IEnumerator BounceRoutine()
    {
        // Wait one frame so CardCellAdapter.Refresh() can set the correct scale first
        yield return null;

        Vector3 baseScale = transform.localScale;
        Vector3 bigScale  = baseScale * bounceScale;
        float   half      = bounceDuration * 0.5f;

        // Scale up
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(baseScale, bigScale, t / half);
            yield return null;
        }
        // Scale back down
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(bigScale, baseScale, t / half);
            yield return null;
        }
        transform.localScale = baseScale;
    }

    public void PlaceIntoSlot(Transform slot)
    {
        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);

        transform.SetParent(slot, false);
        transform.localScale    = Vector3.one;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;  // clear any fan tilt from the hand

        _canvasGroup.blocksRaycasts = true;

        DestroyPlaceholder();
    }

    public void LockInSlot()
    {
        IsLockedInSlot = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public void UnlockFromSlot()
    {
        IsLockedInSlot = false;
    }

    private IEnumerator SmoothReturnRoutine()
    {
        Vector3 worldStartPos = transform.position;
        Vector3 startScale    = transform.localScale;   // capture current scale (may be 0.5 in play area)
        Vector3 worldEndPos;

        if (_placeholder != null)
            worldEndPos = _placeholder.transform.position;
        else
            worldEndPos = _dragStartWorldPos; // card was dropped into a slot; fly back to where it came from

        // Lift the card to the root canvas immediately (same as OnBeginDrag does).
        // Without this, the card stays inside EventCardContent during the animation,
        // so CardCellAdapter resets its scale and CardHover fights the scale lerp.
        if (_rootCanvas != null && transform.parent != _rootCanvas.transform)
        {
            transform.SetParent(_rootCanvas.transform, true);
            transform.SetAsLastSibling();
        }
        _canvasGroup.blocksRaycasts = false; // ignore hover/click events during flight

        float time = 0f;

        while (time < returnDuration)
        {
            time += Time.deltaTime;
            float t = time / returnDuration;
            transform.position   = Vector3.Lerp(worldStartPos, worldEndPos,   t);
            transform.localScale = Vector3.Lerp(startScale,    Vector3.one,   t); // grow back to full size
            yield return null;
        }

        _canvasGroup.blocksRaycasts = true;

        // worldPositionStays=true: the animation ended at the placeholder's world position,
        // so the card appears in the correct visual spot immediately on reparent.
        // Using false left a raw rootCanvas localPosition in EventHandContent space —
        // a garbage anchoredPosition that the canvas system could read before fan.Refresh().
        transform.SetParent(OriginalParent, true);

        // GridLayoutGroup in CardContent overwrites anchorMin/anchorMax to (0,1).
        // Restore the original anchors so anchoredPosition is interpreted correctly
        // in EventHandContent (which expects (0.5,0.5) anchors).
        _rectTransform.anchorMin = _originalAnchorMin;
        _rectTransform.anchorMax = _originalAnchorMax;

        if (_placeholder != null)
            transform.SetSiblingIndex(_placeholder.transform.GetSiblingIndex());
        else
            transform.SetSiblingIndex(OriginalSiblingIndex);

        transform.localScale = Vector3.one;

        // Cancel any in-flight hover animation so the card isn't stuck in hovered state
        // after returning to hand (e.g. click-return from CardContent).
        GetComponent<CardHover>()?.ResetHoverState();

        // If returning to a fan hand, position immediately via the fan layout so the
        // card never flashes at (0,0).  Destroy() is deferred, so we must detach the
        // placeholder from the hierarchy NOW (SetParent null) before calling Refresh()
        // so that childCount is accurate when fan positions are calculated.
        var fan = OriginalParent?.GetComponent<HandFanLayout>();
        if (fan != null)
        {
            if (_placeholder != null)
            {
                _placeholder.transform.SetParent(null);   // immediately removes from HandContent
                Destroy(_placeholder);
                _placeholder     = null;
                _placeholderLayout = null;
            }
            fan.Refresh();
        }
        else
        {
            DestroyPlaceholder();
            transform.localPosition = Vector3.zero;
            if (OriginalParent is RectTransform rt)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        _moveRoutine = null;
    }

    // Removes card data from played lists and gives it back to the player's deck.
    // Called when the player left-drags a locked card out of CardContent.
    private void UnlockFromCardContent()
    {
        IsLockedInSlot = false;
        var lm   = LevelManager.instance;
        var card = GetComponent<Card>();

        if (card?.cardData is EventCardData ec)
        {
            lm?.playedEventCards.Remove(ec);
            Player.instance.eventCardDeck.Add(ec);
        }
        else if (card?.cardData is ItemCardData ic)
        {
            lm?.playedItemCards.Remove(ic);
            Player.instance.itemCardDeck.Add(ic);
        }

        lm?.RecalculateAccumulators();
    }

    private void CreatePlaceholder()
    {
        if (_placeholder != null) return;

        _placeholder = new GameObject("CardPlaceholder", typeof(RectTransform), typeof(LayoutElement));
        _placeholder.transform.SetParent(OriginalParent, false);
        _placeholder.transform.SetSiblingIndex(OriginalSiblingIndex);

        _placeholderLayout = _placeholder.GetComponent<LayoutElement>();

        LayoutElement myLayout = GetComponent<LayoutElement>();
        if (myLayout != null)
        {
            _placeholderLayout.preferredWidth = myLayout.preferredWidth;
            _placeholderLayout.preferredHeight = myLayout.preferredHeight;
            _placeholderLayout.flexibleWidth = myLayout.flexibleWidth;
            _placeholderLayout.flexibleHeight = myLayout.flexibleHeight;
        }
        else
        {
            _placeholderLayout.preferredWidth = _rectTransform.rect.width;
            _placeholderLayout.preferredHeight = _rectTransform.rect.height;
        }
    }

    private void DestroyPlaceholder()
    {
        if (_placeholder != null)
        {
            Destroy(_placeholder);
            _placeholder = null;
            _placeholderLayout = null;
        }
    }
}