using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

public enum LevelState
{
    Intro,
    Shop,
    ChooseEventCard,
    ChooseItemCard,
    Calculate,
    Reward
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public string enemyDescription;
    public string enemyName;
    public int money;
    public List<EventCardData> CardsInShop;
    public List<EventCardData> playedEventCards = new List<EventCardData>(); //Delete after playing item cards
    public List<ItemCardData> playedItemCards = new List<ItemCardData>();
    public int rewardCnt = 2; //the number of reward cards to choose from after winning a level, can be changed by some item cards
    public int shopRefreshTimes = 0;
    public int maxCardsBought = 10;
    public int shopCardAmt = 4;

    public double targetScore
    {
        get => _targetScore;
        set => _targetScore = value;
    }
    public double curScore => _curScore;
    public double totalMult = 1;
    public double totalPlus = 0;


    [SerializeField] private double _targetScore = 10;
    [SerializeField] private double _curScore = 0;

    [SerializeField] private LevelState _curLevelState = LevelState.Shop;
    [SerializeField] private List<EventCardData> _OwnedEventCards = new List<EventCardData>();
    [SerializeField] private List<ItemCardData> _OwnedItemCards = new List<ItemCardData>();
    [SerializeField] private List<ItemCardData> _earnedItemCards = new List<ItemCardData>(); //item cards earned by playing event cards, will be added to owned item cards after this level
    [SerializeField] private List<ItemCardData> _rewardItemCard;
    [SerializeField] private ItemCardData _badRewardItemCard;
    [SerializeField] private LevelModifier _LM;

    private int cardBoughtCnt = 0;


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
        Reset();   
    }

    private void Reset()
    {
         //Remove played cards from owned cards
        _OwnedEventCards.RemoveAll(x => playedEventCards.Contains(x));
        _OwnedItemCards.RemoveAll(x => playedItemCards.Contains(x));

        //Clear played cards and add earned item cards to owned item cards
        playedEventCards.Clear();
        playedItemCards.Clear();
        _earnedItemCards.Clear();
        totalMult = 1;
        totalPlus = 0;

        //reset money, shopRefreshTimes, rewardCnt, targetScore, _curScore
        shopRefreshTimes = 0;
        rewardCnt = 2;
        targetScore = 10;
        _curScore = 0;

        

        Debug.Log("Admin: choose the enemy. Press space after you finish.");

        //Resetting the state is always the last thing to do
        _curLevelState = LevelState.Intro;
    }



    // Update is called once per frame
    void Update()
    {
        if (_curLevelState == LevelState.Intro)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _LM.Apply();
                Debug.Log("Your Enemy is " + enemyName);
                Debug.Log(enemyDescription);

                //Refresh Shop
                CardsInShop = CardDeckManager.instance.RandomDrawEventCards(shopCardAmt);

                Debug.Log("Press space to start shopping.");
                _curLevelState = LevelState.Shop;
            }
        }

        else if (_curLevelState == LevelState.Shop)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("The shop has the following cards");
                Debug.Log("Card Name, Price, Multiplier");
                for (int i = 0; i < CardsInShop.Count; i++)
                {
                    Debug.Log(CardsInShop[i].cardName + "    " + CardsInShop[i].price + "    " + CardsInShop[i].mult);
                }
                Debug.Log("Press according number to buy cards. There is only one card for each");
                Debug.Log("You have " + money + " coin.");
                Debug.Log("When you finish shopping, press N to start choosing event cards.");
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Shopping(1);
                _LM.OnCardBought();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Shopping(2);
                _LM.OnCardBought();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Shopping(3);
                _LM.OnCardBought();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Shopping(4);
                _LM.OnCardBought();
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("Finish shopping, start to choose event card");
                Debug.Log("You have the following event cards in hand:");
                for (int i = 0; i < _OwnedEventCards.Count; i++)
                {
                    Debug.Log(_OwnedEventCards[i].cardName + "    " + _OwnedEventCards[i].price + "    " + _OwnedEventCards[i].mult);
                    
                }
                Debug.Log("Press according number to play event cards.");
                Debug.Log("After playing event cards, press N to start choosing item cards");
                _curLevelState = LevelState.ChooseEventCard;
            }
        }
        else if (_curLevelState == LevelState.ChooseEventCard)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playedEventCards.Add(_OwnedEventCards[0]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playedEventCards.Add(_OwnedEventCards[1]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playedEventCards.Add(_OwnedEventCards[2]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                playedEventCards.Add(_OwnedEventCards[3]);
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                PlayEventCards();
                Debug.Log("Finish playing event cards, start to choose item card");
                Debug.Log("You have the following item cards in hand:");
                for (int i = 0; i < _OwnedItemCards.Count; i++)
                {
                    Debug.Log(_OwnedItemCards[i].cardName + "    " + _OwnedItemCards[i].plus);

                }
                Debug.Log("Press according number to play item cards.");
                Debug.Log("After playing item cards, press N to calculate the score");
                _curLevelState = LevelState.ChooseItemCard;
            }

        }
        else if (_curLevelState == LevelState.ChooseItemCard)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playedItemCards.Add(_OwnedItemCards[0]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playedItemCards.Add(_OwnedItemCards[1]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playedItemCards.Add(_OwnedItemCards[2]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                playedItemCards.Add(_OwnedItemCards[3]);
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                PlayItemCards();
                Debug.Log("Finish playing item cards, start to calculate score");
                _curLevelState = LevelState.Calculate;
            }
        }
        else if (_curLevelState == LevelState.Calculate)
        {
            Calculate();
        }

        else if (_curLevelState == LevelState.Reward)
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("Choose one of the following cards to add to your hand: (Press 1 for the first card, 2 for the second card, etc. Press E for the bad item card)");
                _rewardItemCard = new List<ItemCardData>(CardDeckManager.instance.RandomDrawItemCards(rewardCnt-1));
                _badRewardItemCard = CardDeckManager.instance.RandomDrawBadItemCards(1)[0];
                for (int i = 0; i < _rewardItemCard.Count-1; i++)
                {
                    Debug.Log((i+1) + ". " + _rewardItemCard[i].cardName + "    " + _rewardItemCard[i].plus);
                }
                Debug.Log(rewardCnt + ". " + _badRewardItemCard.cardName + "    " + _badRewardItemCard.plus);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                var card = _rewardItemCard[0];
                _OwnedItemCards.Add(card);
                Debug.Log("You chose " + card.cardName);
                NextLevel();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                var card = _rewardItemCard[1]; 
                _OwnedItemCards.Add(card);
                Debug.Log("You chose " + card.cardName);
                NextLevel();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                _OwnedItemCards.Add(_badRewardItemCard);
                Debug.Log("You chose " + _badRewardItemCard.cardName);
                NextLevel();
            }

            
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

   
    //play single event card
    public void PlayEventCard(EventCardData card)
    {
        Debug.Log("You played " + card.cardName);
        totalMult *= card.mult;
        _earnedItemCards.Add(card.itemCard);
    }

    public void PlayEventCards()
    {
        for (int i = 0; i < playedEventCards.Count; i++)
        {
            var ec = playedEventCards[i];
            Debug.Log("You played " + ec.cardName);
            totalMult *= ec.mult;
            _earnedItemCards.Add(ec.itemCard);
        }
        
    }

    private void PlayItemCards()
    {
        for (int i = 0; i < playedItemCards.Count; i++)
        { 
            var ic = playedItemCards[i];
            Debug.Log("You played " + ic.cardName);
            ic.combo.Execute(ic);
            if (ic.combo.canPlay)
            {
                totalPlus += ic.plus;
            }
            else
            {
                playedItemCards.Clear();
                Debug.Log("You need to rechoose your item cards");
                _curLevelState = LevelState.ChooseItemCard;
                totalPlus = 0;
                return;
            }
            _LM.OnItemCardPlayed();
        }
    }

    public void Calculate()
    {
        Debug.Log("Total Multiplier is " + totalMult);
        Debug.Log("Total Plus is " + totalPlus);
        _curScore = totalMult * totalPlus;
        Debug.Log("Your score is " + _curScore);

        if (_curScore >= _targetScore)
        {
            Debug.Log("You win!");
            Debug.Log("Press N to show your rewards");
            _curLevelState = LevelState.Reward;
        }
        else
        {
            Debug.Log("You lose!");
            Reset();
        }
    }

    private void NextLevel()
    {
        Debug.Log("Start next level!");
        //Remove played cards from owned cards
        _OwnedEventCards.RemoveAll(x => playedEventCards.Contains(x));
        _OwnedItemCards.RemoveAll(x => playedItemCards.Contains(x));

        //Clear played cards and add earned item cards to owned item cards
        playedEventCards.Clear();
        playedItemCards.Clear();
        _OwnedItemCards.AddRange(_earnedItemCards);
        _earnedItemCards.Clear();

        //Reset mult and plus   
        totalMult = 1;
        totalPlus = 0;

        //reset money, shopRefreshTimes, rewardCnt, targetScore, _curScore
        shopRefreshTimes = 0;
        rewardCnt = 2;
        targetScore = 10;
        _curScore = 0;

        //Resetting the state is always the last thing to do
        _curLevelState = LevelState.Intro;
    }

    
}
