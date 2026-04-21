using UnityEngine;
using System.Collections.Generic;

public class CardDeckManager : MonoBehaviour
{
    public static CardDeckManager instance;

    private List<NormalItemCardData> _normalItemCardDeck = new List<NormalItemCardData>();
    private List<EventCardData> _eventCardDeck = new List<EventCardData>();
    private List<BadItemCardData> _badItemCardDeck = new List<BadItemCardData>();

    private List<NormalItemCardData> _normalDiscardPile = new List<NormalItemCardData>();
    private List<EventCardData> _eventDiscardPile = new List<EventCardData>();
    private List<BadItemCardData> _badDiscardPile = new List<BadItemCardData>();

    public List<NormalItemCardData> normalItemCardDeck => _normalItemCardDeck;
    public List<EventCardData> eventCardDeck => _eventCardDeck;
    public List<BadItemCardData> badItemCardDeck => _badItemCardDeck;

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAllCards();
    }

    private void LoadAllCards()
    {
        _normalItemCardDeck = LoadDeck<NormalItemCardData>("CardData/ItemCardData");
        _eventCardDeck = LoadDeck<EventCardData>("CardData/EventCardData");
        _badItemCardDeck = LoadDeck<BadItemCardData>("CardData/BadItemCardData");
    }

    private List<T> LoadDeck<T>(string path) where T : CardData
    {
        var deck = new List<T>();
        foreach (var card in Resources.LoadAll<T>(path))
        {
            for (int i = 0; i < card.copies; i++)
            {
                var copy = Instantiate(card);
                copy.original = card; 
                deck.Add(copy);
            }
        }
        return deck;
    }

    //CHANGED: called when a level is finished
    public void LevelReset()
    {
        foreach (var card in _normalItemCardDeck) DiscardCard(card);
        foreach (var card in _eventCardDeck) DiscardCard(card);
        foreach (var card in _badItemCardDeck) DiscardCard(card);

        _normalItemCardDeck.Clear();
        _eventCardDeck.Clear();
        _badItemCardDeck.Clear();

        // discard pile → deck
        _normalItemCardDeck.AddRange(_normalDiscardPile);
        _eventCardDeck.AddRange(_eventDiscardPile);
        _badItemCardDeck.AddRange(_badDiscardPile);

        _normalDiscardPile.Clear();
        _eventDiscardPile.Clear();
        _badDiscardPile.Clear();
    }

    //ADDED:called when the game is reset
    public void GameReset()
    {
        _normalDiscardPile.Clear();
        _eventDiscardPile.Clear();
        _badDiscardPile.Clear();

        _normalItemCardDeck.Clear();
        _eventCardDeck.Clear();
        _badItemCardDeck.Clear();

        LoadAllCards();
    }

 
    public List<NormalItemCardData> RandomDrawItemCards(int n) => RandomDraw(_normalItemCardDeck, n);
    public List<EventCardData> RandomDrawEventCards(int n) => RandomDraw(_eventCardDeck, n);
    public List<BadItemCardData> RandomDrawBadItemCards(int n) => RandomDraw(_badItemCardDeck, n);

    private List<T> RandomDraw<T>(List<T> deck, int n) where T : CardData
    {
        List<T> drawnCards = new List<T>();
        List<T> pool = new List<T>(deck);

        for (int i = 0; i < n; i++)
        {
            if (pool.Count == 0)
            {
                Debug.LogWarning($"{typeof(T).Name} deck is empty!");
                break;
            }
            int randomIndex = Random.Range(0, pool.Count);
            drawnCards.Add(pool[randomIndex]);
            deck.Remove(pool[randomIndex]); 
            pool.RemoveAt(randomIndex);
        }
        return drawnCards;
    }


    public void DiscardCard(CardData card)
    {
        if (card is NormalItemCardData normal)
            Discard(normal, _normalDiscardPile);
        else if (card is EventCardData eventCard)
            Discard(eventCard, _eventDiscardPile);
        else if (card is BadItemCardData bad)
            Discard(bad, _badDiscardPile);
        else
            Debug.LogWarning($"Unknown card type: {card.GetType()}");
    }

    private void Discard<T>(T card, List<T> discardPile) where T : CardData
    {
        //debugging to track why it crashes -.-
        Debug.Log($"Discarding: {card.cardName}, original null? {card.original == null}");

        if (card.disposable)
        {
            Destroy(card);
            return; // don't try to make a fresh copy of disposable cards
        }

        if (card.original == null)
        {
            Debug.LogError($"Card {card.cardName} has no original reference — skipping discard");
            return;
        }

        T freshCopy = Instantiate(card.original as T);
        freshCopy.original = card.original;
        discardPile.Add(freshCopy);
        Destroy(card);
    }

    public void AddMultToAllEventCards(double mult)
    {
        AddMultToEventCardsInShop(mult);
        AddMultToEventCardsInPlayer(mult);
        AddMultToPlayedEventCards(mult);
        AddMultToEventDeck(mult);
    }

     public void AddMultToEventCardsInShop(double mult)
    {
        foreach (var card in Shop.instance.cardsInShop)
        {
            card.mult += mult;
            card.multModified = true;
        }
    }

    public void AddMultToEventCardsInPlayer(double mult)
    {
        foreach (var card in Player.instance.eventCardDeck)
        {
            card.mult += mult;
            card.multModified = true;
        }
    }

    public void AddMultToPlayedEventCards(double mult)
    {
        foreach (var card in LevelManager.instance.playedEventCards)
        {
            card.mult += mult;
            card.multModified = true;
        }
    }

    public void AddMultToEventDeck(double mult)
    {
        foreach (var card in _eventCardDeck)
        {
            card.mult += mult;
            card.multModified = true;
        }
    }

    public void AddPlusToItemCardsInPlayer(int plus)
    {
        foreach (var card in Player.instance.itemCardDeck)
        {
            card.plus += plus;
            card.plusModified = true;
        }
    }

    public void AddPlusToPlayedItemCards(int plus)
    {
        foreach (var card in LevelManager.instance.playedItemCards)
        {
            card.plus += plus;
            card.plusModified = true;
        }
    }

    public void AddPlusToItemDeck(int plus)
    {
        foreach (var card in _normalItemCardDeck)
        {
            card.plus += plus;
            card.plusModified = true;
        }
        foreach (var card in _badItemCardDeck)
        {
            card.plus += plus;
            card.plusModified = true;
        }
    }

}