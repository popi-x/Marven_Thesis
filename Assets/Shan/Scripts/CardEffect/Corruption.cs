using UnityEngine;

[CreateAssetMenu(fileName = "Corruption", menuName = "Scriptable Objects/Combo/Corruption")]
public class Corruption : Combo
{
    public override void OnCardPlay(ItemCardData cd = null)
    {
        if (cd is BadItemCardData bd)
        {
            // CHANGED: increment first, then check — was checking before incrementing
            bd.corruptionCnt++;
            if (bd.corruptionCnt >= 4)
                GameManager.instance.Corrupt();

            if (GameManager.instance.isCorrupted)
            {
                LevelManager.instance.totalMult += bd.plus;
                LevelManager.instance.targetScore *= bd.scoreMult;
            }
            else
            {
                Debug.Log("This card will corrupt after " + (4 - bd.corruptionCnt) + " more uses.");
                base.OnCardPlay(cd);
            }
        }
    }
}
