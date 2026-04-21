using UnityEngine;

public abstract class Combo : ScriptableObject
{
    public bool canPlay = true;

    public virtual void OnCardPlay(ItemCardData cd = null)
    {
        LevelManager.instance.totalPlus += cd.plus;
        // CHANGED: removed playedItemCards.Add(cd) cuz again drop zone handles list tracking
    }

    public virtual void OnCardSubmit(ItemCardData cd = null)
    {
        return;
    }
}