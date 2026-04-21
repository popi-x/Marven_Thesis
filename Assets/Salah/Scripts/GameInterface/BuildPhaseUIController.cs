using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildPhaseUIController : MonoBehaviour
{
    // ── Card containers ──────────────────────────────────────────────────────
    [Header("Card Containers")]
    [SerializeField] private Transform eventCardContainer;
    [SerializeField] private Transform itemCardContainer;

    // ── Aggregate score labels ───────────────────────────────────────────────
    [Header("Score Labels")]
    [SerializeField] private TMP_Text eventScoreLabel;
    [SerializeField] private TMP_Text itemScoreLabel;

    [SerializeField] private TMP_Text lastMultFlashLabel;

    // ── HUD ─────────────────────────────────────────────────────────────────
    [Header("HUD")]
    [SerializeField] private TMP_Text levelNameText;

    // ── Score Bar ────────────────────────────────────────────────────────────
    [Header("Score Bar")]
    [SerializeField] private Image         scoreBarFill;
    [SerializeField] private RectTransform scoreBarBase;

    [SerializeField] private Image lightOn;
    [SerializeField] private int   totalBulbs = 24;

    [SerializeField] private RectTransform currentScoreRT;
    [SerializeField] private TMP_Text      currentScoreLabel;
    [SerializeField] private TMP_Text      targetScoreLabel;

    [Tooltip("How far below the bar the current score text sits.")]
    [SerializeField] private float textYOffset = -22f;
    [Tooltip("Min pixel gap between current and target labels before clamping.")]
    [SerializeField] private float minLabelGap = 55f;

    // ── Submit ───────────────────────────────────────────────────────────────
    [Header("Buttons")]
    [SerializeField] private Button submitButton;

    // ── Internal ─────────────────────────────────────────────────────────────
    private LevelManager LM => LevelManager.instance;
    private Coroutine _flashRoutine;

    // ── Lifecycle ────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        LevelManager.OnECPlayed += OnEventCardPlayed;
    }

    private void OnDisable()
    {
        LevelManager.OnECPlayed -= OnEventCardPlayed;
    }

    private void Start()
    {
        if (lastMultFlashLabel != null)
            lastMultFlashLabel.gameObject.SetActive(false);

        RefreshHUD();
        RefreshSubmitState();
    }

    private void Update()
    {
        RefreshHUD();
        RefreshSubmitState();
    }

    // ── Flash ────────────────────────────────────────────────────────────────
    private void OnEventCardPlayed(EventCardData ec)
    {
        if (lastMultFlashLabel == null) return;
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashMult(ec.mult));
    }

    private IEnumerator FlashMult(double mult)
    {
        lastMultFlashLabel.text = $"x{mult}";
        lastMultFlashLabel.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        lastMultFlashLabel.gameObject.SetActive(false);
        _flashRoutine = null;
    }

    // ── Slot management ──────────────────────────────────────────────────────
    public void ClearSlots()
    {
        DestroyAllChildren(eventCardContainer);
        DestroyAllChildren(itemCardContainer);
        RefreshSubmitState();
    }

    public void ClearEventSlot()
    {
        if (eventCardContainer == null || eventCardContainer.childCount == 0) return;

        foreach (var ec in LM.playedEventCards)
            Player.instance.eventCardDeck.Add(ec);
        LM.playedEventCards.Clear();

        DestroyAllChildren(eventCardContainer);

        LM.RecalculateAccumulators();
        LM.handUI?.Rebuild();
        RefreshSubmitState();
    }

    public void ClearItemSlot()
    {
        if (itemCardContainer == null || itemCardContainer.childCount == 0) return;

        foreach (var ic in LM.playedItemCards)
            Player.instance.itemCardDeck.Add(ic);
        LM.playedItemCards.Clear();

        DestroyAllChildren(itemCardContainer);

        LM.RecalculateAccumulators();
        LM.handUI?.Rebuild();
        RefreshSubmitState();
    }

    // ── Submit ───────────────────────────────────────────────────────────────
    public void SubmitSelectedCards()
    {
        if (LM == null) return;

        bool hasEvent = eventCardContainer != null && eventCardContainer.childCount > 0;
        bool hasItem  = itemCardContainer  != null && itemCardContainer.childCount  > 0;

        if (!hasEvent || !hasItem) return;

        LM.Submit();

        DestroyAllChildren(eventCardContainer);
        DestroyAllChildren(itemCardContainer);

        RefreshHUD();
        RefreshSubmitState();
    }

    // ── Refresh ──────────────────────────────────────────────────────────────
    private void RefreshSubmitState()
    {
        if (submitButton == null) return;

        bool hasEvent = eventCardContainer != null && eventCardContainer.childCount > 0;
        bool hasItem  = itemCardContainer  != null && itemCardContainer.childCount  > 0;

        submitButton.interactable = hasEvent && hasItem;
    }

    public void RefreshHUD()
    {
        if (LM == null) return;

        if (levelNameText != null)
            levelNameText.text = LM.enemy != null ? LM.enemy.tempName : "";

        if (eventScoreLabel != null)
            eventScoreLabel.text = $"x{LM.totalMult:F2}";
        if (itemScoreLabel != null)
            itemScoreLabel.text = $"+{LM.totalPlus:F0}";

        // ── Score bar ──────────────────────────────────────────────────────
        double liveScore = LM.totalMult * LM.totalPlus;
        float  pct       = LM.targetScore > 0
                           ? Mathf.Clamp01((float)(liveScore / LM.targetScore))
                           : 0f;

        if (scoreBarFill != null)
            scoreBarFill.fillAmount = pct;

        if (lightOn != null && totalBulbs > 0)
        {
            int lit = Mathf.FloorToInt(pct * totalBulbs);
            lightOn.fillAmount = (float)lit / totalBulbs;
        }

        if (targetScoreLabel != null)
            targetScoreLabel.text = $"{LM.targetScore:F0}";

        if (currentScoreLabel != null)
            currentScoreLabel.text = $"{liveScore:F0}";

        if (currentScoreRT != null && scoreBarBase != null)
        {
            Vector3[] corners = new Vector3[4];
            scoreBarBase.GetWorldCorners(corners);
            Vector3 leftWorld  = corners[0];
            Vector3 rightWorld = corners[3];

            float guardedPct = Mathf.Clamp(pct, 0f,
                1f - minLabelGap / Vector3.Distance(leftWorld, rightWorld));

            Vector3 fillEdgeWorld = Vector3.Lerp(leftWorld, rightWorld, guardedPct);

            var parentRT = currentScoreRT.parent as RectTransform;
            if (parentRT != null)
            {
                Vector3 local = parentRT.InverseTransformPoint(fillEdgeWorld);
                currentScoreRT.localPosition = new Vector3(local.x, currentScoreRT.localPosition.y, 0f);
            }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private static void DestroyAllChildren(Transform t)
    {
        if (t == null) return;
        for (int i = t.childCount - 1; i >= 0; i--)
            Destroy(t.GetChild(i).gameObject);
    }
}
