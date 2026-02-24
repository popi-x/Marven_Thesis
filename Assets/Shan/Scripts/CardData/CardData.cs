using UnityEngine;

public abstract class CardData : ScriptableObject
{
    [Header("Card Basic Info)")]
    [SerializeField] private string _cardName;
    [SerializeField] private Sprite _cardImage;

    public string cardName => _cardName; 
    public Sprite cardImage => _cardImage;

}
