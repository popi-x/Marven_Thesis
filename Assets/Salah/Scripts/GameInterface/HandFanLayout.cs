using UnityEngine;

[ExecuteAlways]
public class HandFanLayout : MonoBehaviour
{
    [Tooltip("X = card count, Y = total horizontal spread in pixels.")]
    [SerializeField] private AnimationCurve spreadWidthCurve = new AnimationCurve(
        new Keyframe(1,  80f), new Keyframe(2, 100f), new Keyframe(3, 120f),
        new Keyframe(6, 160f), new Keyframe(9, 200f), new Keyframe(12, 240f)
    );

    [Tooltip("X = card count, Y = how much the edges dip below the centre.")]
    [SerializeField] private AnimationCurve arcDepthCurve = new AnimationCurve(
        new Keyframe(1, 40f), new Keyframe(2, 35f), new Keyframe(3, 30f),
        new Keyframe(6, 25f), new Keyframe(9, 20f), new Keyframe(12, 15f)
    );

    [Tooltip("X = card count, Y = max rotation of outermost card in degrees.")]
    [SerializeField] private AnimationCurve maxRotationCurve = new AnimationCurve(
        new Keyframe(1,  4f), new Keyframe(2,  6f), new Keyframe(3, 12f),
        new Keyframe(6, 12f), new Keyframe(9, 20f), new Keyframe(12, 24f)
    );

    [Header("Multipliers — scale curve output without editing curves")]
    [Tooltip("Scales horizontal spread. 1 = default, 2 = twice as wide.")]
    [SerializeField] private float spreadMultiplier  = 1f;
    [Tooltip("Scales arc depth. 0 = flat line, 2 = twice as curved.")]
    [SerializeField] private float arcMultiplier     = 1f;
    [Tooltip("Scales card rotation. 0 = no tilt, 2 = twice the angle.")]
    [SerializeField] private float rotationMultiplier = 1f;

    // Dirty flag — set whenever children change, cleared in Update after one refresh.
    // Using Update() as the trigger (not direct OnTransformChildrenChanged call) is more
    // reliable in play mode because Unity may reparent cards through intermediate parents
    // (e.g. canvas root during drag) and miss some OnTransformChildrenChanged callbacks.
    private bool _dirty = true;

    private void OnValidate()
    {
        _dirty = true;
        Refresh(); // immediate in editor so Inspector tweaks feel instant
    }

    private void OnTransformChildrenChanged() => _dirty = true;

    private void Start()
    {
        _dirty = true;
    }

    private void Update()
    {
        if (!_dirty) return;
        _dirty = false;
        Refresh();
    }

    public void Refresh()
    {
        int count = transform.childCount;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            var child = transform.GetChild(i);

            // CardHover owns this card's position while it is hovered or animating back
            var hover = child.GetComponent<CardHover>();
            if (hover != null && hover.IsControlled) continue;

            var rt = child.GetComponent<RectTransform>();
            if (rt == null) continue;

            rt.anchoredPosition = CalculatePosition(i, count);
            child.localRotation = Quaternion.Euler(0f, 0f, CalculateRotation(i, count));
        }
    }

    // ── Public helpers so HandCardHover can query resting values ─────────────

    public Vector2 CalculatePosition(int index, int total)
    {
        float spread = spreadWidthCurve.Evaluate(total) * spreadMultiplier;
        float arc    = arcDepthCurve   .Evaluate(total) * arcMultiplier;
        float c      = total > 1 ? (float)index / (total - 1) - 0.5f : 0f;
        return new Vector2(c * spread, -(c * c) * arc * 4f);
    }

    public float CalculateRotation(int index, int total)
    {
        float maxRot = maxRotationCurve.Evaluate(total) * rotationMultiplier;
        float c      = total > 1 ? (float)index / (total - 1) - 0.5f : 0f;
        return c * -maxRot * 2f;
    }
}
