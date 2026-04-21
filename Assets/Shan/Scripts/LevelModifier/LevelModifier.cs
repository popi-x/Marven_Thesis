using UnityEngine;
using System.Collections.Generic;

public abstract class LevelModifier : ScriptableObject
{
    public string tempName;
    public List<EventCardTag> tags;

    [Header("Intro")]
    public Sprite characterSprite;
    public string effectName;

    [Header("Bad item card")]
    public ItemCardData bcd;

    [TextArea] public string effectDescription;

    public virtual void Apply() { }
    public virtual void Remove() { }
    public virtual void OnEventCardPlayed() { }
    public virtual void OnItemCardPlayed() { }
    public virtual void OnCardBought() { }
    public virtual void DoublePlus() { }
}
