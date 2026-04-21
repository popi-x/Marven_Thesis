// CHANGED: removed "using NUnit.Framework" — that is a test-only namespace, not needed here.
// using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;


public enum EventCardTag
{
    安全感,
    链接,
    自我价值,
    None
}


[CreateAssetMenu(fileName = "EventCardData", menuName = "Scriptable Objects/CardData/EventCardData")]
public class EventCardData : CardData
{
    [SerializeField] private int _price = 2;
    [SerializeField] private double _mult;
    [SerializeField] private List<EventCardTag> _tags;
    [SerializeField] private ItemCardData _itemCard;
    [SerializeField] private string _narrration;

    public int price { get => _price; set => _price = value; }
    public double mult { get => _mult; set => _mult = value; }
    public List<EventCardTag> tags => _tags;
    public ItemCardData itemCard => _itemCard;
    public string narration => _narrration;
    public bool priceModified = false;
    public bool multModified = false;

    //TODO: add mult to the totalMult
    public override void OnCardPlay()
    {
        var LM = LevelManager.instance;
        LM.totalMult *= _mult;
        // removed LM.playedEventCards.Add(this) cuz drop zone handles list tracking

        // CHANGED: added null check — _itemCard may be null for cards that don't award an item.
        // Original (no guard): LM.earnedItemCards.Add(_itemCard);
        if (_itemCard != null)
            LM.earnedItemCards.Add(_itemCard);
    }


}
