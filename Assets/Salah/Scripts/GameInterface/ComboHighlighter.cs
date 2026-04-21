using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the card prefab root.
/// Creates its own glow Image child so it works regardless of the card's
/// internal hierarchy. Shows a coloured glow + subtle scale pulse when active.
/// Pulse pauses while CardHover controls the scale.
/// </summary>
public class ComboHighlighter : MonoBehaviour
{
    [Header("Glow")]
    [SerializeField] private Color glowColor   = new Color(0.4f, 1f, 0.6f, 0.6f);
    [SerializeField] private float glowPadding = 8f;   // pixels the glow extends beyond the card

    [Header("Pulse")]
    [SerializeField] private float pulseMin   = 1.00f;
    [SerializeField] private float pulseMax   = 1.05f;
    [SerializeField] private float pulseSpeed = 1.5f;  // full cycles per second

    private CardHover _hover;
    private Image     _glowImage;
    private Coroutine _pulseRoutine;
    private bool      _active;

    public bool IsActive => _active;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _hover = GetComponent<CardHover>();
        CreateGlowImage();
    }

    private void CreateGlowImage()
    {
        var go = new GameObject("ComboGlow",
                     typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);
        go.transform.SetAsFirstSibling(); // render behind all card children

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-glowPadding, -glowPadding);
        rt.offsetMax = new Vector2( glowPadding,  glowPadding);

        _glowImage = go.GetComponent<Image>();
        _glowImage.raycastTarget = false;
        _glowImage.color = Color.clear;   // hidden until StartHighlight
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void StartHighlight()
    {
        if (_active) return;
        _active = true;

        _glowImage.color = glowColor;

        if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
        _pulseRoutine = StartCoroutine(PulseRoutine());
    }

    public void StopHighlight()
    {
        if (!_active) return;
        _active = false;

        _glowImage.color = Color.clear;

        if (_pulseRoutine != null)
        {
            StopCoroutine(_pulseRoutine);
            _pulseRoutine = null;
        }

        if (_hover == null || !_hover.IsControlled)
            transform.localScale = Vector3.one;
    }

    // ── Pulse ─────────────────────────────────────────────────────────────────

    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            if (_hover != null && _hover.IsControlled)
            {
                yield return null;
                continue;
            }

            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float s = Mathf.Lerp(pulseMin, pulseMax, t);
            transform.localScale = Vector3.one * s;
            yield return null;
        }
    }
}
