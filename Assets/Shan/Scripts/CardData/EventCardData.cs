using NUnit.Framework;
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
    [SerializeField] private double _price = 2;
    [SerializeField] private double _mult;
    [SerializeField] private List<EventCardTag> _tags;
    [SerializeField] private ItemCardData _itemCard;
    [SerializeField] private string _narrration;

    public double price => _price;
    public double mult => _mult;
    public List<EventCardTag> tags => _tags;
    public ItemCardData itemCard => _itemCard;
    public string narration => _narrration;

}
