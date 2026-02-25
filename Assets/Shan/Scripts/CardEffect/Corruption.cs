using UnityEngine;

public class Corruption : Combo
{
    public override void Execute(ItemCardData cd = null)
    {
        if (cd is BadItemCardData bd)
        {
            if (bd.isCorrupted)
            {
                Debug.Log("This card is corrupted");
                LevelManager.instance.totalMult += bd.plus;
                LevelManager.instance.targetScore *= bd.scoreMult;
            }
            else
            {
                bd.corruptionCnt++;
                Debug.Log("This card will corrupt after " + (4 - bd.corruptionCnt) + " more uses.");
                base.Execute(cd);
            }
        }
    }
}
