using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using System;



public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public static event Action<EventCardData> OnNextECPlayed;
    public static event Action<EventCardData> OnECPlayed;
    public static event Action<ItemCardData> OnICPlayed;
    public static event Action OnLevelEnd;

    public static event Action<List<ItemCardData>> OnRewardReady;
    public static event Action OnEnvyTrigger;
    public static event Action<bool> OnResultReady; // true = win, false = lose

    public LevelModifier enemy;
    public HandUIController handUI;
    public BuildPhaseUIController buildUI;

    // Runtime values
    public double totalMult;
    public double totalPlus;
    public int rewardCnt;
    public int maxCardPlay;
    public int playedCardCnt;
    public int envyMult = 0;
    public double targetScore { get => _targetScore; set => _targetScore = value; }
    public double curScore => _curScore;

    public List<EventCardData> playedEventCards = new List<EventCardData>();
    public List<ItemCardData> playedItemCards = new List<ItemCardData>();
    public List<ItemCardData> earnedItemCards = new List<ItemCardData>();

    // Default values
    [Header("Default Values")]
    [SerializeField] private double _defaultTotalMult = 1;
    [SerializeField] private double _defaultTotalPlus = 0;
    [SerializeField] private int _defaultRewardCnt = 2;
    [SerializeField] private int _defaultMaxCardPlay = 100;

    [Header("Money reward settings")]
    [SerializeField] private int _defaultTagMoney = 3;
    [SerializeField] private int _defaultScoreMoney = 5;
    [SerializeField] private int _defaultMoneyReward = 5;

    [Header("Reward card settings")]
    public ItemCardData rewardNormalCard;
    public ItemCardData rewardBadCard;

    // Serialized fields
    [Header("Level Settings")]
    [SerializeField] private double _targetScore = 10;
    [SerializeField] private double _curScore = 0;
    [SerializeField] private List<ItemCardData> _rewardItemCard;
    [SerializeField] private ItemCardData _badRewardItemCard;

    private int _lastEnvyThreshold = 1;

    // The target score as it stood right after enemy.Apply() — restored every
    // RecalculateAccumulators so mid-play card removal never permanently stacks
    // effects that modify targetScore (e.g. Corruption).
    private double _baseTargetScore;


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
        totalMult = _defaultTotalMult;
        totalPlus = _defaultTotalPlus;
        rewardCnt = _defaultRewardCnt;
        maxCardPlay = _defaultMaxCardPlay;
        playedCardCnt = 0;
        _curScore = 0;
        _lastEnvyThreshold = 1;

    }

    public void StartLevel()
    {
        enemy.Apply();

        // Snapshot targetScore after all level-start modifiers have been applied.
        // RecalculateAccumulators restores this before reapplying card effects so
        // mid-play card removal never permanently stacks targetScore modifications.
        _baseTargetScore = targetScore;

        // Draw cards into the shop data first so the UI has something to show
        Shop.instance.OpenShop();

        // Populate hand and open shop immediately — no waiting for intro animation
        handUI?.Rebuild();
        ShopUIController.instance?.OpenShopAnimated();

        // Intro plays as a pure visual overlay; gameplay is already ready
        LevelIntroController.instance?.PlayIntro();
    }

    
    public void PlayCard(Card c)
    {
        var cd = c.cardData;
        if (cd is EventCardData ecd)
        {
            if (playedCardCnt >= maxCardPlay)
            {
                CardCannotPlay("You have played the maximum number of cards allowed.");
                return;
            }
            playedCardCnt++;
            c.Play();
            OnNextECPlayed?.Invoke(ecd);
            OnECPlayed?.Invoke(ecd);

            while (envyMult != 0 && totalMult >= _lastEnvyThreshold + envyMult)
            {
                _lastEnvyThreshold += 3;
                OnEnvyTrigger?.Invoke();
            } 

            playedEventCards.Add(ecd);
            
        }
        else if (cd is ItemCardData icd)
        {
            c.Play();
            OnICPlayed?.Invoke(icd);
            playedItemCards.Add(icd);
        }

    }

    
    public void CardCannotPlay(string msg)
    {
        Debug.Log(msg);
    }

    // CHANGED: added fullReset parameter.
    // fullReset = true  → called on loss; wipes all acquired cards and restores initial tokens.
    // fullReset = false → called between normal rounds; only refreshes stats on current deck.
    public void Reset(bool fullReset = false)
    {
        enemy?.Remove();

        if (fullReset)
            Player.instance.RestoreInitialCards(); // wipes decks, restores starting tokens + money
        else
            Player.instance.ResetCardStats(); // original behaviour — kept for reference
        // Player.instance.ResetCardStats(); ← was the only call before fullReset was added

        playedEventCards.Clear();
        playedItemCards.Clear();
        earnedItemCards?.Clear();

        totalMult = _defaultTotalMult;
        totalPlus = _defaultTotalPlus;
        rewardCnt = _defaultRewardCnt;
        maxCardPlay = _defaultMaxCardPlay;
        playedCardCnt = 0;
        envyMult = 0;
        _curScore = 0;
        _lastEnvyThreshold = 1;

        // refresh UI
        buildUI?.ClearSlots();
        handUI?.Rebuild();

        //Resetting the state is always the last thing to do
    }

    public void SkipLevel()
    {
        Debug.Log("Level skipped!");
        Reward();

        GameManager.instance.EndLevel(true);
        //Player.instance.itemCardDeck.AddRange(earnedItemCards);
    }

    // Resets totalMult/totalPlus/earnedItemCards and recalculates from the card values
    // directly — does NOT call OnCardPlay() to avoid permanent side-effects (e.g.
    // Corruption.OnCardPlay increments corruptionCnt and multiplies targetScore, which
    // must only happen once when the card is first played, not on every recalculate).
    public void RecalculateAccumulators()
    {
        totalMult    = _defaultTotalMult;
        totalPlus    = _defaultTotalPlus;
        targetScore  = _baseTargetScore;   // restore before reapplying card effects
        earnedItemCards.Clear();

        foreach (var ec in playedEventCards)
        {
            totalMult *= ec.mult;
            if (ec.itemCard != null) earnedItemCards.Add(ec.itemCard);
        }

        foreach (var ic in playedItemCards)
        {
            totalPlus += ic.plus;

            // Reapply Corruption's targetScore multiplier without triggering the
            // one-time side-effects (corruptionCnt++ / Corrupt()) that belong only
            // in the first OnCardPlay call.
            if (ic is BadItemCardData bd && ic.combo is Corruption && GameManager.instance.isCorrupted)
                targetScore *= bd.scoreMult;
        }

        // Combo conditions may have changed — refresh highlights in the hand
        HandComboManager.instance?.RefreshHighlights();
    }

    // CHANGED: Item cards removal are now handled by its own OnCardSubmit() so remove playedItemCards.Remove()
    public void Submit()
    {
        var p = Player.instance;

        foreach (var ec in playedEventCards)
            ec.OnCardSubmit();

        foreach (var ic in playedItemCards)
        {
            ic.OnCardSubmit();
        }

        Calculate();
    }


    public void Calculate()
    {
        enemy.DoublePlus(); // Sloth modifier

        _curScore = totalMult * totalPlus;

        bool won = _curScore >= _targetScore;

        if (won)
        {
            OnLevelEnd?.Invoke();
            Player.instance.itemCardDeck.AddRange(earnedItemCards);
            Reward(); // fires OnRewardReady so reward panel can prepare cards
        }

        // Always fire OnResultReady — ResultUIController handles the rest
        // GameManager.EndLevel() is now called by the result UI, not here
        OnResultReady?.Invoke(won);
    }

    public void Reward()
    {
        // CHANGED: guard against empty CDM decks — [0] would throw IndexOutOfRangeException.
        // Original (no guard):
        // var rewardItemCard    = CardDeckManager.instance.RandomDrawItemCards(1)[0];
        // var rewardBadItemCard = CardDeckManager.instance.RandomDrawBadItemCards(1)[0];
        // var rewardPool = new List<ItemCardData> { rewardItemCard, rewardBadItemCard };
        // OnRewardReady?.Invoke(rewardPool);


        if (rewardNormalCard == null || rewardBadCard == null)
        {
            Debug.LogWarning("LevelManager.Reward: one or both card decks are empty — firing OnRewardReady with empty pool.");
            OnRewardReady?.Invoke(new List<ItemCardData>());
        }
        else
        {
            var rewardPool = new List<ItemCardData> {rewardNormalCard, rewardBadCard};
            OnRewardReady?.Invoke(rewardPool);
        }

        //add default reward moeny
        Player.instance.money += _defaultMoneyReward;

        //if tags of enemy match with played event cards, give more money
        foreach (var tag in enemy.tags)
        {
            foreach (var ec in playedEventCards)
            {
                if (ec.tags.Contains(tag))
                {
                    Player.instance.money += _defaultTagMoney;
                    break;
                }
            }
        }

        //if score is much higher than target, give more money
        if (_curScore >= _targetScore * 2)
        {
            Player.instance.money += _defaultScoreMoney;
        }

    }

    //Called by reward UI when player chooses a reward card
    public void Claim(ItemCardData c)
    {
        Player.instance.itemCardDeck.Add(c);
    }



}
