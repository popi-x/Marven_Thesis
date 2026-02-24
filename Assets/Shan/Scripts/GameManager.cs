using UnityEngine;
using System.Collections.Generic;

public enum LevelState
{
    Intro,
    Shop,
    ChooseEventCard,
    ChooseItemCard,
    Calculate,
    Result
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public string enemyDescription;
    public string enemyName;
    public int money;
    public List<EventCardData> CardsInShop;

    [SerializeField] private double _targetScore = 10;
    [SerializeField] private double _curScore = 0;

    private LevelState _curLevelState = LevelState.Shop;
    private List<EventCardData> _OwnedEventCards = new List<EventCardData>();
    private List<ItemCardData> _OwnedItemCards = new List<ItemCardData>();
    private List<CardData> _playedCards = new List<CardData>(); //Delete after playing item cards
    private double _totalMult = 1;

    private void Awake()
    {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Game Starts!");
        Debug.Log("Your Enemy is " + enemyName);
        Debug.Log(enemyDescription);
        Debug.Log("The shop has the following cards");
        Debug.Log ("Card Name, Price, Multiplier");
        for (int i = 0; i < CardsInShop.Count; i++)
        {
            Debug.Log(CardsInShop[i].cardName + "    " + CardsInShop[i].price + "    " + CardsInShop[i].mult);
            Debug.Log("Press according number to buy cards.");
            Debug.Log("When you finish shopping, press space to start choosing event cards.");
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (_curLevelState == LevelState.Shop)
        {
           if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Shopping(1);
            }
           if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Shopping(2);
            }
           if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Shopping(3);
            }
           if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Shopping(4);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Finish shopping, start to choose event card");
                Debug.Log("You have the following event cards in hand:");
                for (int i = 0; i < _OwnedEventCards.Count; i++)
                {
                    Debug.Log(_OwnedEventCards[i].cardName + "    " + _OwnedEventCards[i].price + "    " + _OwnedEventCards[i].mult);
                    Debug.Log("Press according number to play event cards.");
                }
                _curLevelState = LevelState.ChooseEventCard;
            }
        }
        if (_curLevelState == LevelState.ChooseEventCard)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PlayEventCard(1);
            }
            if 
        }

    }

    private void Shopping(int index)
    {
        if (money >= CardsInShop[index - 1].price)
        {
            money -= (int)CardsInShop[index - 1].price;
            _OwnedEventCards.Add(CardsInShop[index - 1]);
            Debug.Log("You bought " + CardsInShop[index - 1].cardName);
        }
        else
        {
            Debug.Log("Not enough money to buy " + CardsInShop[index - 1].cardName);
        }
    }

    private void PlayEventCard(int index)
    {
        var ec = _OwnedEventCards[index - 1];
        Debug.Log("You played " + ec.cardName);
        _totalMult *= ec.mult;
        _OwnedItemCards.Add(ec.itemCard);
        
        _playedCards.Add(ec);
    }
}
