using UnityEngine;

public abstract class LevelModifier : ScriptableObject
{
    public virtual void Apply() { }
    public virtual void OnEventCardPlayed() { }
    public virtual void OnItemCardPlayed() { }
    public virtual void OnCardBought() { }
}
