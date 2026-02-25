using UnityEngine;

public abstract class ItemCardData : CardData
{
    
    [SerializeField] private int _plus;
    [SerializeField] private string _description;
    [Header("combo effect")]
    [SerializeField] private Combo _combo;

    public int plus => _plus;
    public string description => _description;
    public Combo combo => null; // TODO: implement combo
}
