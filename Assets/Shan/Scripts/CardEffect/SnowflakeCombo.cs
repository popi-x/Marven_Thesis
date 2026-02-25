using UnityEngine;

public class SnowflakeCombo : Combo
{
    public override void Execute(ItemCardData cd = null)
    {
        LevelManager.instance.rewardCnt++;
        base.Execute(cd);
    }
}
