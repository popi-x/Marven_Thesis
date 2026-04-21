using UnityEngine;

[CreateAssetMenu(fileName = "BlankCombo", menuName = "Scriptable Objects/Combo/BlankCombo")]
public class BlankCombo : Combo
{
    public override void OnCardSubmit(ItemCardData cd = null)
    {
        var LM = LevelManager.instance;
        if (LM.totalPlus - cd.plus < 8)
        {
            LM.totalPlus += 15 - cd.plus;
        }
    }
}
