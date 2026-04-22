using UnityEngine;

public abstract class Combo : ScriptableObject
{
    public bool canPlay = true;

    public virtual void OnCardPlay(ItemCardData cd = null)
    {
        LevelManager.instance.totalPlus += cd.plus;
        LevelManager.instance.playedItemCards.Add(cd);
    }

    public virtual void OnCardSubmit(ItemCardData cd = null)
    {
        return;
    }
}