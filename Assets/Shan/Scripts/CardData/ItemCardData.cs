using UnityEngine;

public abstract class ItemCardData : CardData
{
     
    [SerializeField] private int _plus;
    [SerializeField] private string _description;
    [Header("combo effect")]
    [SerializeField] private Combo _combo;

    public int plus { get => _plus; set => _plus = value; }
    public string description => _description;
    public Combo combo => _combo;
    public bool comboDisabled = false;
    public bool plusModified = false;
    public int bonusPlus = 0;

    public override void OnCardPlay()
    {
        _combo.OnCardPlay(this);
    }

    public override void OnCardSubmit()
    {
        _combo.OnCardSubmit(this);
        // CHANGED: call base function to remove cards
        base.OnCardSubmit();
    }
}
