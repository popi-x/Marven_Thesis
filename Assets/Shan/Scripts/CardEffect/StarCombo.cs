using UnityEngine;

[CreateAssetMenu(fileName = "StarCombo", menuName = "Scriptable Objects/Combo/StarCombo")]
public class StarCombo : Combo
{
    public override void OnCardSubmit(ItemCardData cd = null)
    {
        var cnt = LevelManager.instance.playedItemCards.Count;
        if (cnt == 1)
        {
            LevelManager.instance.SkipLevel();
        }
    }
}
