using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CardDeckManager : MonoBehaviour
{
    public static CardDeckManager instance;
    
    private List<NormalItemCardData> _normalItemCardDeck = new List<NormalItemCardData>();
    private List<EventCardData> _eventCardDeck = new List<EventCardData>();
    private List<BadItemCardData> _badItemCardDeck = new List<BadItemCardData>();

    public List<NormalItemCardData> normalItemCardDeck => _normalItemCardDeck;
    public List<EventCardData> eventCardDeck => _eventCardDeck;
    public List<BadItemCardData> badItemCardDeck => _badItemCardDeck;

    private void Awake()
    {
       if (instance != null)
       {
           Destroy(gameObject);
           return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllCards();
    }

    private void LoadAllCards()
    {
        _normalItemCardDeck = new List<NormalItemCardData>(Resources.LoadAll<NormalItemCardData>("CardData/ItemCardData"));
        _eventCardDeck = new List<EventCardData>(Resources.LoadAll<EventCardData>("CardData/EventCardData"));
        _badItemCardDeck = new List<BadItemCardData>(Resources.LoadAll<BadItemCardData>("CardData/BadItemCardData"));
    }

    public List<EventCardData> RandomDrawEventCards(int n)
    {
        //draw n random event cards from the event card deck
        List<EventCardData> drawnCards = new List<EventCardData>();

        for (int i = 0; i < n; i++)
        {
            if (_eventCardDeck.Count == 0)
            {
                Debug.LogWarning("Event card deck is empty!");
                break;
            }
            int randomIndex = Random.Range(0, _eventCardDeck.Count);
            drawnCards.Add((EventCardData)_eventCardDeck[randomIndex]);
        }
        return drawnCards;
    }

    public List<NormalItemCardData> RandomDrawItemCards(int n)
    {
        //draw n random item cards from the item card deck
        List<NormalItemCardData> drawnCards = new List<NormalItemCardData>();
        for (int i = 0; i < n; i++)
        {
            if (_normalItemCardDeck.Count == 0)
            {
                Debug.LogWarning("Item card deck is empty!");
                break;
            }
            int randomIndex = Random.Range(0, _normalItemCardDeck.Count);
            drawnCards.Add(_normalItemCardDeck[randomIndex]);
        }
        return drawnCards;
    }

    public List<BadItemCardData> RandomDrawBadItemCards(int n)
    {
        //draw n random bad item cards from the bad item card deck
        List<BadItemCardData> drawnCards = new List<BadItemCardData>();
        for (int i = 0; i < n; i++)
        {
            if (_badItemCardDeck.Count == 0)
            {
                Debug.LogWarning("Bad item card deck is empty!");
                break;
            }
            int randomIndex = Random.Range(0, _badItemCardDeck.Count);
            drawnCards.Add(_badItemCardDeck[randomIndex]);
        }
        return drawnCards;
    }

}
