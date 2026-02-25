using UnityEngine;

[CreateAssetMenu(fileName = "GluttonyLM", menuName = "Scriptable Objects/LM/GluttonyLM")]
public class GluttonyLM : LevelModifier
{
    [Header("extra target score")]
    [SerializeField] private double _extra = 10;
    public override void OnCardBought()
    {
        LevelManager.instance.targetScore += _extra;
    }
}
