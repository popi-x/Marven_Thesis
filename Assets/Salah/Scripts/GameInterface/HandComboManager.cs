using UnityEngine;

/// <summary>
/// Attach to any persistent scene object (e.g. GameUI / HandUI).
/// Listens to card-play events and lights up / dims the combo glow
/// on every item card currently sitting in the hand.
/// </summary>
public class HandComboManager : MonoBehaviour
{
    public static HandComboManager instance { get; private set; }

    [Tooltip("The Transform that holds the spawned item-card objects (itemHandContent).")]
    [SerializeField] private Transform itemHandContent;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        LevelManager.OnECPlayed += OnCardPlayed;
        LevelManager.OnICPlayed += OnCardPlayed;
    }

    private void OnDisable()
    {
        LevelManager.OnECPlayed -= OnCardPlayed;
        LevelManager.OnICPlayed -= OnCardPlayed;
    }

    // ── Event callbacks ───────────────────────────────────────────────────────

    private void OnCardPlayed(EventCardData _) => RefreshHighlights();
    private void OnCardPlayed(ItemCardData  _) => RefreshHighlights();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Re-evaluate every item card in the hand and update its highlight.
    /// Call this after Rebuild() or any time the hand's card list changes.
    /// </summary>
    public void RefreshHighlights()
    {
        if (itemHandContent == null) return;

        foreach (Transform child in itemHandContent)
        {
            var highlighter = child.GetComponent<ComboHighlighter>();
            if (highlighter == null) continue;

            var card = child.GetComponent<Card>();
            if (card?.cardData is ItemCardData ic && HasActiveCombo(ic))
                highlighter.StartHighlight();
            else
                highlighter.StopHighlight();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool HasActiveCombo(ItemCardData ic)
    {
        // Only show glow when at least one event card is in play —
        // that's what "Card A activates Card B's combo" means in the task.
        var lm = LevelManager.instance;
        if (lm == null || lm.playedEventCards.Count == 0)
            return false;

        return ic.combo != null
            && !(ic.combo is BlankCombo)
            && !ic.comboDisabled
            && ic.combo.canPlay;
    }
}
