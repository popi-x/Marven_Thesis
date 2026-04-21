using UnityEngine;

[CreateAssetMenu(fileName = "SnowflakeCombo", menuName = "Scriptable Objects/Combo/SnowflakeCombo")]
public class SnowflakeCombo : Combo
{
    public override void OnCardPlay(ItemCardData cd = null)
    {
        base.OnCardPlay(cd);
        LevelManager.instance.earnedItemCards.Add(Instantiate(cd));
    }
}
