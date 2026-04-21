using UnityEngine;
using UnityEngine.UI;

// Attach to CardContent (the GridLayoutGroup GameObject).
// Leave GridLayoutGroup Cell Size at the card's native size (150x330).
// Use the Scale slider to shrink cards visually without changing Width/Height.
[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class CardCellAdapter : MonoBehaviour
{
    [Range(0.1f, 2f)]
    public float scale = 1f;   // public so drop zones can read it if needed

    private void OnValidate()                 => Refresh();
    private void OnTransformChildrenChanged() => Refresh();
    private void Start()                      => Refresh();

    // Called explicitly by drop zones after PlaceIntoSlot(),
    // because PlaceIntoSlot sets localScale = Vector3.one after parenting,
    // which would override the scale set by OnTransformChildrenChanged.
    public void Refresh()
    {
        foreach (Transform child in transform)
            child.localScale = Vector3.one * scale;
    }
}
