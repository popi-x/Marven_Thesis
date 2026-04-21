using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the shop card preview overlay.
///
/// Scene hierarchy expected (add above ShopPanel, below LevelIntroPanel):
///
///   ShopPreviewPanel                 (this script + CanvasGroup)
///   ├── Overlay                      (Image, full screen dark bg — Button with OnClick → OnOverlayClicked)
///   ├── PreviewCard                  (RectTransform, centre screen — CardViewUI + CanvasGroup)
///   └── BuyButtonGroup               (child of ShopPreviewPanel, below PreviewCard)
///       └── BuyButton                (Button → OnClick → OnBuyPressed)
///           └── BuyLabel             (TMP_Text)
/// </summary>
public class ShopPreviewController : MonoBehaviour
{
    public static ShopPreviewController instance;

    [Header("Content (child of ShopPreviewPanel — gets shown/hidden)")]
    [SerializeField] private GameObject content;

    [Header("Overlay")]
    [SerializeField] private CanvasGroup overlayGroup;
    [SerializeField] private float       overlayAlpha = 0.4f;

    [Header("Preview Card")]
    [SerializeField] private RectTransform previewCardRT;   // centred card holder
    [SerializeField] private CardViewUI    previewCardView;
    [SerializeField] private float         previewScale = 2.5f;

    [Header("Buy Button")]
    [SerializeField] private Button   buyButton;
    [SerializeField] private TMP_Text buyLabel;

    [Header("Fly target")]
    [Tooltip("The hand content transform — bought card flies here.")]
    [SerializeField] private RectTransform handContent;

    [Header("Timings")]
    [SerializeField] private float openDuration = 0.2f;
    [SerializeField] private float flyDuration  = 0.4f;

    private EventCardData _card;
    private int           _index;
    private bool          _isOpen;
    private Coroutine     _routine;
    private CanvasGroup   _cardCG;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;

        // Ensure card has a CanvasGroup for fade-out during fly
        if (previewCardRT != null)
        {
            _cardCG = previewCardRT.GetComponent<CanvasGroup>();
            if (_cardCG == null) _cardCG = previewCardRT.gameObject.AddComponent<CanvasGroup>();

            // Disable gameplay components — preview card is display-only
            DisableIfPresent<DraggableCard>(previewCardRT.gameObject);
            DisableIfPresent<CardHover>(previewCardRT.gameObject);
            DisableIfPresent<PlayedCardClickReturn>(previewCardRT.gameObject);
        }

        // Hide via CanvasGroup instead of SetActive — this keeps Awake() running
        // so instance is always set regardless of scene state.
        HidePanel();
    }

    // ── Open ──────────────────────────────────────────────────────────────────

    public void OpenPreview(EventCardData card, int shopIndex)
    {
        if (_isOpen) return;
        _card   = card;
        _index  = shopIndex;
        _isOpen = true;

        ShowPanel();

        // Populate card view
        previewCardView.Set(card);

        // Buy button
        bool canAfford = Player.instance.money >= card.price;
        if (buyButton != null)  buyButton.interactable = canAfford;
        if (buyLabel  != null)  buyLabel.text          = $"Buy  ({card.price}g)";

        // Reset visual state before animating
        overlayGroup.alpha          = 0f;
        previewCardRT.localScale    = Vector3.one;
        if (_cardCG != null) _cardCG.alpha = 1f;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        for (float t = 0f; t < openDuration; t += Time.deltaTime)
        {
            float p     = Mathf.Clamp01(t / openDuration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);   // ease-out cubic
            overlayGroup.alpha       = Mathf.Lerp(0f, overlayAlpha, eased);
            float s = Mathf.Lerp(1f, previewScale, eased);
            previewCardRT.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        overlayGroup.alpha       = overlayAlpha;
        previewCardRT.localScale = Vector3.one * previewScale;
        _routine = null;
    }

    // ── Buy ───────────────────────────────────────────────────────────────────

    /// <summary>Called by the Buy button's OnClick event.</summary>
    public void OnBuyPressed()
    {
        if (!_isOpen || _card == null) return;
        if (Player.instance.money < _card.price) return;

        ShopUIController.instance.BuyCardAt(_index);

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(BuyFlyRoutine());
    }

    private IEnumerator BuyFlyRoutine()
    {
        // Fly card from screen centre toward hand, shrink + fade out
        Vector3 startWorldPos  = previewCardRT.position;
        Vector3 targetWorldPos = handContent != null
            ? handContent.position
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.1f, 0f);

        Vector3 startScale = previewCardRT.localScale;

        for (float t = 0f; t < flyDuration; t += Time.deltaTime)
        {
            float p     = Mathf.Clamp01(t / flyDuration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);

            previewCardRT.position   = Vector3.Lerp(startWorldPos, targetWorldPos, eased);
            float s = Mathf.Lerp(startScale.x, 0.8f, eased);
            previewCardRT.localScale = new Vector3(s, s, 1f);
            if (_cardCG != null)
                _cardCG.alpha        = Mathf.Lerp(1f, 0f, eased);
            overlayGroup.alpha       = Mathf.Lerp(overlayAlpha, 0f, eased);
            yield return null;
        }

        CloseImmediate();
    }

    // ── Close ─────────────────────────────────────────────────────────────────

    /// <summary>Called by the dark overlay Button's OnClick — click outside to close.</summary>
    public void OnOverlayClicked()
    {
        if (!_isOpen) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        float   startAlpha = overlayGroup.alpha;
        Vector3 startScale = previewCardRT.localScale;

        for (float t = 0f; t < openDuration; t += Time.deltaTime)
        {
            float p = Mathf.Clamp01(t / openDuration);
            overlayGroup.alpha       = Mathf.Lerp(startAlpha, 0f, p);
            float s = Mathf.Lerp(startScale.x, 1f, p);
            previewCardRT.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        CloseImmediate();
    }

    private static void DisableIfPresent<T>(GameObject go) where T : MonoBehaviour
    {
        var c = go.GetComponent<T>();
        if (c != null) c.enabled = false;
    }

    private void CloseImmediate()
    {
        _isOpen  = false;
        _card    = null;
        _routine = null;
        HidePanel();
    }

    // ── Show / Hide helpers ───────────────────────────────────────────────────

    private void ShowPanel()
    {
        content.SetActive(true);
        overlayGroup.alpha = 0f; // OpenRoutine fades this in
    }

    private void HidePanel()
    {
        content.SetActive(false);
        if (_cardCG != null) _cardCG.alpha = 1f;
        if (previewCardRT != null) previewCardRT.localScale = Vector3.one;
    }
}
