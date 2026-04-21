using UnityEngine;
using System.Collections.Generic;

public class Shop : MonoBehaviour
{
    public static Shop instance;

    // Runtime values
    public int buyCnt;
    public int maxBuyCnt;
    public int freeRefreshCnt;
    public int refreshCost;
    public int cardCnt;
    public List<EventCardData> cardsInShop = new List<EventCardData>();

    // Default values
    [Header("Default Values")]
    [SerializeField] private int _defaultMaxBuyCnt = 100;
    [SerializeField] private int _defaultRefreshCost = 1;
    [SerializeField] private int _defaultCardCnt = 4;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        ResetShop(true); // moved from Start so shop cards exist before GameManager.Start() runs
    }

    public void ResetShop(bool isStart = false)
    {
        buyCnt = 0;
        maxBuyCnt = _defaultMaxBuyCnt;
        refreshCost = _defaultRefreshCost;
        cardCnt = _defaultCardCnt;
        freeRefreshCnt = 0;

        //CardDeckManager.instance.DiscardCard(card);

        if (!isStart)
        {
            foreach (var card in cardsInShop)
            {
                CardDeckManager.instance.DiscardCard(card);
            }
        }
       
        cardsInShop.Clear();
    }

    public void OpenShop()
    {
        cardsInShop.AddRange(CardDeckManager.instance.RandomDrawEventCards(cardCnt));
    }

    public void AddCostToCards(int n)
    {
        foreach (var card in cardsInShop)
        {
            card.price += n;
            card.priceModified = true;
        }
    }

    public void Refresh()
    {
        if (freeRefreshCnt > 0)
        {
            freeRefreshCnt--;
            RefreshCards();
        }
        else if (Player.instance.money >= refreshCost)
        {
            Player.instance.money -= refreshCost;
            RefreshCards();
        }
        else
        {
            Debug.Log("Not enough money to refresh the shop!");
        }
    }

    private void RefreshCards()
    {
        foreach (var card in cardsInShop)
        {
            CardDeckManager.instance.DiscardCard(card);
        }
        cardsInShop.Clear();
        cardsInShop.AddRange(CardDeckManager.instance.RandomDrawEventCards(cardCnt));
    }

    public void Buy(Card card)
    {
        if (card.cardData is EventCardData ecd)
        {
            if (Player.instance.money >= ecd.price && buyCnt < maxBuyCnt)
            {
                Player.instance.money -= ecd.price;
                Player.instance.eventCardDeck.Add(ecd);
                cardsInShop.Remove(ecd);
                buyCnt++;
                LevelManager.instance.enemy?.OnCardBought();
            }
            else
            {
                Debug.Log("Not enough money to buy this card!");
            }
        }
    }
}