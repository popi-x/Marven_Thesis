using UnityEngine;

[CreateAssetMenu(fileName = "SnowflakeCombo", menuName = "Scriptable Objects/Combo/SnowflakeCombo")]
public class SnowflakeCombo : Combo
{
    public override void Execute(ItemCardData cd = null)
    {
        LevelManager.instance.rewardCnt++;
        base.Execute(cd);
    }
}
