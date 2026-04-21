using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach to the Submit button GameObject alongside the Button component.
///
/// Behaviour:
///   Dim  — button starts dark (low brightness).
///   Lit  — fades to full brightness when both an event AND item card are in play.
///   Hover — small looping rotation shake (±3°) while the pointer is over it.
///   Click — quick scale flash before the Button's onClick fires.
/// </summary>
[RequireComponent(typeof(Button))]
public class SubmitLampButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Lamp")]
    [Tooltip("The Image that represents the lamp glow. Defaults to the Button's own Image.")]
    [SerializeField] private Image lampImage;
    [SerializeField] private Color dimColor  = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color litColor  = Color.white;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Hover Shake")]
    [SerializeField] private float shakeAngle = 3f;    // degrees
    [SerializeField] private float shakeSpeed = 12f;   // oscillations per second

    [Header("Click Flash")]
    [SerializeField] private float flashScale    = 1.2f;
    [SerializeField] private float flashDuration = 0.12f;

    // ── Internal ─────────────────────────────────────────────────────────────
    private Button    _button;
    private bool      _lit;
    private bool      _hovering;
    private Coroutine _fadeRoutine;
    private Coroutine _shakeRoutine;
    private Coroutine _flashRoutine;
    private Vector3   _restPosition;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _button       = GetComponent<Button>();
        _restPosition = transform.localPosition;

        if (lampImage == null)
            lampImage = GetComponent<Image>();

        if (lampImage != null)
            lampImage.color = dimColor;
    }

    private void Update()
    {
        bool conditionMet = ConditionMet();

        if (conditionMet && !_lit)
        {
            _lit = true;
            FadeTo(litColor);
        }
        else if (!conditionMet && _lit)
        {
            _lit = false;
            FadeTo(dimColor);

            // Stop shake if we dim while hovering (button became non-interactable)
            StopShake();
        }
    }

    // ── Pointer events ────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_lit) return;   // don't shake when dim / not interactable

        _hovering = true;
        if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
        _shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopShake();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_lit || !_button.interactable) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    // ── Lamp fade ─────────────────────────────────────────────────────────────

    private void FadeTo(Color target)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(target));
    }

    private IEnumerator FadeRoutine(Color target)
    {
        if (lampImage == null) yield break;

        Color start = lampImage.color;
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            lampImage.color = Color.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        lampImage.color = target;
        _fadeRoutine = null;
    }

    // ── Hover shake ───────────────────────────────────────────────────────────

    private IEnumerator ShakeRoutine()
    {
        while (_hovering)
        {
            float angle = Mathf.Sin(Time.time * shakeSpeed) * shakeAngle;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        transform.localRotation = Quaternion.identity;
        _shakeRoutine = null;
    }

    private void StopShake()
    {
        _hovering = false;
        // ShakeRoutine exits its loop on the next frame and resets rotation itself
    }

    // ── Click flash ───────────────────────────────────────────────────────────

    private IEnumerator FlashRoutine()
    {
        Vector3 baseScale = transform.localScale;
        Vector3 bigScale  = baseScale * flashScale;
        float   half      = flashDuration * 0.5f;

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(baseScale, bigScale, t / half);
            yield return null;
        }
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(bigScale, baseScale, t / half);
            yield return null;
        }

        transform.localScale = baseScale;
        _flashRoutine = null;
    }

    // ── Condition ─────────────────────────────────────────────────────────────

    private static bool ConditionMet()
    {
        var lm = LevelManager.instance;
        return lm != null
            && lm.playedEventCards.Count > 0
            && lm.playedItemCards.Count  > 0;
    }
}
