using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the score bar, discrete bulb lights, and the two score text labels.
///
/// Hierarchy expected:
///   score_bar
///   ├── scorebar_base        RectTransform (the outer frame, sets the bar width)
///   │   └── scorebar_fill    Image — Type: Filled, Fill Method: Horizontal, Origin: Left
///   ├── light_off            Image — static background (all bulbs unlit)
///   ├── light_on             Image — Type: Filled, Fill Method: Horizontal, Origin: Left
///   ├── current_score_text   TMP_Text  (moves along bar with fill edge)
///   └── target_score_text    TMP_Text  (fixed at right end of bar)
/// </summary>
public class ScoreBarUI : MonoBehaviour
{
    [Header("Bar")]
    [SerializeField] private Image         scorebarFill;
    [SerializeField] private RectTransform scorebarBase;   // used to read bar width

    [Header("Bulbs")]
    [SerializeField] private Image lightOn;                // Filled image on top of light_off
    [SerializeField] private int   totalBulbs = 24;

    [Header("Score Text")]
    [SerializeField] private RectTransform currentTextRT;  // the RectTransform that moves
    [SerializeField] private TMP_Text      currentLabel;   // e.g. "72"
    [SerializeField] private TMP_Text      targetLabel;    // e.g. "100"  (fixed at right end)

    [Header("Text Layout")]
    [Tooltip("How far below the bar edge the current score text sits (negative = below).")]
    [SerializeField] private float textYOffset = -22f;

    [Tooltip("Minimum pixel gap between current text and target text before clamping kicks in.")]
    [SerializeField] private float minGap = 55f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void LateUpdate()
    {
        if (LevelManager.instance == null) return;
        Refresh();
    }

    // ── Public so BuildPhaseUIController can call it too ──────────────────────

    public void Refresh()
    {
        var lm = LevelManager.instance;

        double current = lm.totalMult * lm.totalPlus;
        double target  = lm.targetScore;
        float  pct     = target > 0 ? Mathf.Clamp01((float)(current / target)) : 0f;

        RefreshFill(pct);
        RefreshBulbs(pct);
        RefreshText(current, target, pct);
    }

    // ── Fill bar ──────────────────────────────────────────────────────────────

    private void RefreshFill(float pct)
    {
        if (scorebarFill != null)
            scorebarFill.fillAmount = pct;
    }

    // ── Bulbs ─────────────────────────────────────────────────────────────────

    private void RefreshBulbs(float pct)
    {
        if (lightOn == null || totalBulbs <= 0) return;

        // Snap to discrete whole-bulb count so they switch cleanly
        int litCount = Mathf.FloorToInt(pct * totalBulbs);
        lightOn.fillAmount = (float)litCount / totalBulbs;
    }

    // ── Score text ────────────────────────────────────────────────────────────

    private void RefreshText(double current, double target, float pct)
    {
        // Target label — fixed, always show target value
        if (targetLabel != null)
            targetLabel.text = $"{target:F0}";

        if (currentLabel != null)
            currentLabel.text = $"{current:F0}";

        // Move current text to sit below the fill edge
        if (currentTextRT == null || scorebarBase == null) return;

        float barWidth   = scorebarBase.rect.width;
        float fillEdgeX  = pct * barWidth;          // 0 = left end, barWidth = right end

        // Right edge X where target label sits (used for gap clamping)
        float targetEdgeX = barWidth;

        // Clamp so current text never gets closer than minGap to the target label
        float clampedX = Mathf.Min(fillEdgeX, targetEdgeX - minGap);

        // Keep a small left margin so text doesn't hide behind the frame at 0
        clampedX = Mathf.Max(clampedX, 0f);

        currentTextRT.anchoredPosition = new Vector2(clampedX, textYOffset);
    }
}
