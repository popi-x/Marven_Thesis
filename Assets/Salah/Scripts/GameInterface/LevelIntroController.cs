using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Plays the level intro sequence:
///   1. Level name  — fade in (0.3s), stay (1.0s)
///   2. Character   — fade in + scale 0.95→1.0 (0.3s)
///   3. Level effect — fade in + 10px upward slide (0.3s)
///   4. Token cards — spawn from screen centre, fly to hand (0.4s each, 0.15s apart)
///   5. Shop        — slides in from the side (0.3s)
///
/// Setup in the scene:
///   • Attach to any persistent GameObject.
///   • Build an intro overlay panel (full-screen, appears on top of everything).
///     Inside it, place the levelNameGroup, characterGroup, effectGroup children.
///   • Assign all serialized fields in the Inspector.
/// </summary>
public class LevelIntroController : MonoBehaviour
{
    public static LevelIntroController instance;

    // ── Level Name ────────────────────────────────────────────────────────────
    [Header("Level Name")]
    [SerializeField] private CanvasGroup levelNameGroup;
    [SerializeField] private TMP_Text    levelNameText;

    // ── Character ─────────────────────────────────────────────────────────────
    [Header("Character")]
    [SerializeField] private CanvasGroup characterGroup;
    [SerializeField] private Image       characterImage;

    // ── Level Effect ──────────────────────────────────────────────────────────
    [Header("Level Effect")]
    [SerializeField] private CanvasGroup  effectGroup;
    [SerializeField] private RectTransform effectPanel;   // the box/panel that moves up
    [SerializeField] private TMP_Text     effectNameText;
    [SerializeField] private TMP_Text     effectDescText;

    // ── Hand references ───────────────────────────────────────────────────────
    [Header("Hand (token card spawn)")]
    [Tooltip("The item hand content RectTransform — token cards live here.")]
    [SerializeField] private RectTransform itemHandContent;
    [SerializeField] private HandUIController handUI;

    // ── Shop ──────────────────────────────────────────────────────────────────
    [Header("Shop")]
    [SerializeField] private ShopUIController shopUI;

    // ── Timings ───────────────────────────────────────────────────────────────
    [Header("Timings")]
    [SerializeField] private float fadeInDuration    = 0.3f;
    [SerializeField] private float levelNameStay     = 1.0f;
    [SerializeField] private float cardTravelTime    = 0.4f;
    [SerializeField] private float cardSpawnDelay    = 0.15f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
    }

    // ── Public entry point ────────────────────────────────────────────────────

    /// <summary>Called by LevelManager.StartLevel() to kick off the sequence.</summary>
    public void PlayIntro() => StartCoroutine(IntroSequence());

    // ── Main sequence ─────────────────────────────────────────────────────────

    private IEnumerator IntroSequence()
    {
        var enemy = LevelManager.instance.enemy;

        // Reset all groups to invisible before starting
        SetAlpha(levelNameGroup,  0f);
        SetAlpha(characterGroup,  0f);
        SetAlpha(effectGroup,     0f);

        // ── 1. Level Name ──────────────────────────────────────────────────────
        if (levelNameText != null)
            levelNameText.text = enemy.tempName;

        yield return FadeGroup(levelNameGroup, 0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(levelNameStay);

        // ── 2. Character ───────────────────────────────────────────────────────
        if (characterImage != null)
            characterImage.sprite = enemy.characterSprite;

        if (characterGroup != null)
            characterGroup.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

        yield return FadeGroupScale(characterGroup, 0f, 1f, 0.95f, 1.0f, fadeInDuration);

        // ── 3. Level Effect ────────────────────────────────────────────────────
        if (effectNameText != null) effectNameText.text = enemy.effectName;
        if (effectDescText != null) effectDescText.text = enemy.effectDescription;

        if (effectPanel != null)
        {
            // Record the intended resting position, then start 10px below it
            Vector2 restPos = effectPanel.anchoredPosition;
            effectPanel.anchoredPosition = restPos - new Vector2(0f, 10f);
            yield return FadeGroupMove(effectGroup, effectPanel, 0f, 1f, fadeInDuration, restPos);
        }
        else
        {
            yield return FadeGroup(effectGroup, 0f, 1f, fadeInDuration);
        }

        // Hand and shop are already ready (StartLevel handles that immediately).
        // Intro is visual-only — nothing to do after the effect panel fades in.
    }

    // ── Token card spawn ──────────────────────────────────────────────────────

    private IEnumerator SpawnTokenCards()
    {
        if (itemHandContent == null || handUI == null) yield break;

        // Rebuild so item cards are parented to itemHandContent and positioned by HandFanLayout
        handUI.Rebuild();

        // Wait one frame so HandFanLayout.Update() can run its Refresh()
        yield return null;

        int count      = itemHandContent.childCount;
        int toAnimate  = Mathf.Min(count, 2);
        if (toAnimate == 0) yield break;

        // Compute the screen-centre spawn point in itemHandContent local space
        Vector2 spawnLocal = ScreenCentreToLocal(itemHandContent);

        for (int i = 0; i < toAnimate; i++)
        {
            var rt    = itemHandContent.GetChild(i).GetComponent<RectTransform>();
            if (rt == null) continue;

            Vector2 target = rt.anchoredPosition;

            // Lock the card so HandFanLayout doesn't fight the animation
            var hover = rt.GetComponent<CardHover>();
            hover?.LockForAnimation();

            StartCoroutine(FlyCardToHand(rt, hover, spawnLocal, target));

            if (i < toAnimate - 1)
                yield return new WaitForSeconds(cardSpawnDelay);
        }

        // Wait for the last card to finish arriving
        yield return new WaitForSeconds(cardTravelTime);
    }

    private IEnumerator FlyCardToHand(RectTransform rt, CardHover hover, Vector2 from, Vector2 to)
    {
        rt.anchoredPosition = from;
        rt.localScale       = new Vector3(0.8f, 0.8f, 1f);

        float t = 0f;
        while (t < cardTravelTime)
        {
            t += Time.deltaTime;
            float p      = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / cardTravelTime));
            rt.anchoredPosition = Vector2.Lerp(from, to, p);
            rt.localScale       = Vector3.Lerp(new Vector3(0.8f, 0.8f, 1f), Vector3.one, p);
            yield return null;
        }

        rt.anchoredPosition = to;
        rt.localScale       = Vector3.one;
        hover?.ResetHoverState();   // unlock so normal hover + fan layout work again
    }

    // ── Coordinate helper ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns the screen centre (slightly above midpoint) converted to the local
    /// coordinate space of <paramref name="parent"/>.
    /// Works for both Screen Space Overlay and Screen Space Camera canvases.
    /// </summary>
    private static Vector2 ScreenCentreToLocal(RectTransform parent)
    {
        Canvas canvas = parent.GetComponentInParent<Canvas>();
        Camera uiCam  = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                        ? canvas.worldCamera : null;

        Vector2 screenPoint = new Vector2(Screen.width * 0.5f, Screen.height * 0.6f);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, uiCam, out Vector2 local);
        return local;
    }

    // ── Fade coroutine helpers ────────────────────────────────────────────────

    private static void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group != null) group.alpha = alpha;
    }

    private static IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;
        group.alpha = from;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }

    private static IEnumerator FadeGroupScale(CanvasGroup group,
        float fromAlpha, float toAlpha, float fromScale, float toScale, float duration)
    {
        if (group == null) yield break;
        group.alpha = fromAlpha;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float p = Mathf.Clamp01(t / duration);
            group.alpha = Mathf.Lerp(fromAlpha, toAlpha, p);
            float s = Mathf.Lerp(fromScale, toScale, p);
            group.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        group.alpha = toAlpha;
        group.transform.localScale = new Vector3(toScale, toScale, 1f);
    }

    private static IEnumerator FadeGroupMove(CanvasGroup group, RectTransform panel,
        float fromAlpha, float toAlpha, float duration, Vector2 targetPos)
    {
        if (group == null) yield break;
        Vector2 startPos = panel.anchoredPosition;
        group.alpha = fromAlpha;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float p = Mathf.Clamp01(t / duration);
            group.alpha          = Mathf.Lerp(fromAlpha, toAlpha, p);
            panel.anchoredPosition = Vector2.Lerp(startPos, targetPos, p);
            yield return null;
        }
        group.alpha            = toAlpha;
        panel.anchoredPosition = targetPos;
    }
}
