using UnityEngine;

public abstract class Combo: ScriptableObject
{
    public bool canPlay = true;

    public virtual void Execute(ItemCardData cd = null)
    {
        LevelManager.instance.totalPlus += cd.plus;
    }
}
