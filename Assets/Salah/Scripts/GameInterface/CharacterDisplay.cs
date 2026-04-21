using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the character_sprite Image.
/// Exposes position / size tweaks that can be adjusted live in the Inspector
/// without touching code or prefabs.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class CharacterDisplay : MonoBehaviour
{
    [Header("Position Offset")]
    [Tooltip("Shift the character left/right and up/down from the anchor point.")]
    public Vector2 positionOffset = Vector2.zero;

    [Header("Size")]
    [Tooltip("Width and height of the character sprite in pixels.")]
    public Vector2 size = new Vector2(400f, 600f);

    [Header("Anchor")]
    [Tooltip("Normalised anchor position on the parent. (0.5, 0) = bottom-centre.")]
    public Vector2 anchor = new Vector2(0.5f, 0.1f);

    private RectTransform _rt;

    private void OnValidate() => Apply();
    private void Awake()      => Apply();

    private void Apply()
    {
        if (_rt == null) _rt = GetComponent<RectTransform>();
        if (_rt == null) return;

        _rt.anchorMin = anchor;
        _rt.anchorMax = anchor;
        _rt.pivot     = new Vector2(0.5f, 0f);
        _rt.sizeDelta = size;
        _rt.anchoredPosition = positionOffset;
    }
}
