using UnityEngine;

public abstract class CardData : ScriptableObject
{
    public int copies => _copies;
    public string cardName => _cardName; 
    public Sprite cardImage => _cardImage;
    public bool disposable = false;

    [Header("Card Basic Info)")]
    [SerializeField] private string _cardName;
    [SerializeField] private Sprite _cardImage;
    [SerializeField] private int _copies = 2; // how many copies of this card to add to the deck

    [HideInInspector] public CardData original;

    
    //Called when the card is dropped to the zone
    public abstract void OnCardPlay();

    //Called when the submit button is clicked
    public virtual void OnCardSubmit()
    {
        //CHANGED: remove discard logic cuz played cards are exhausted.
        //aka they are removed from the game instead of going to a discard pile.

        var p = Player.instance;
        if (p.eventCardDeck.Contains(this as EventCardData))
        {
            p.eventCardDeck.Remove(this as EventCardData);
        }
        else if (p.itemCardDeck.Contains(this as ItemCardData))
        {
            p.itemCardDeck.Remove(this as ItemCardData);
        }
    }


}
