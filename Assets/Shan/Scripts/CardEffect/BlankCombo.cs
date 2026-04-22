using UnityEngine;

[CreateAssetMenu(fileName = "BlankCombo", menuName = "Scriptable Objects/Combo/BlankCombo")]
public class BlankCombo : Combo
{
    public override void OnCardPlay(ItemCardData cd = null)
    {
        LevelManager LM = LevelManager.instance;
        if (LM.playedItemCards != null && LM.playedItemCards[-1].plus < 8)
        {
            cd.bonusPlus = 15 - cd.plus;
        }
        base.OnCardPlay(cd);
        LM.totalPlus += cd.bonusPlus;
    }

    
}
