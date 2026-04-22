using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool isCorrupted = false;

    [Header("Level Settings")]
    [SerializeField] private List<LevelModifier> enemyOrder;
    [SerializeField] private List<int> targetScoreList;
    [SerializeField] private int totalLevel = 7;
    [SerializeField] private int currentLevel = 0;
    public List<ItemCardData> rewardCardList = new List<ItemCardData>();

    private LevelManager LM;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    void Start()
    {
        LM = LevelManager.instance;
        LM.Reset();   // initialize all defaults before first level
        StartLevel();
    }

    public void SkipLevel() => EndLevel(true);
 

    public void StartLevel()
    {
        // CHANGED: added bounds check — handles game complete state gracefully.
        if (currentLevel >= enemyOrder.Count || currentLevel >= targetScoreList.Count)
        {
            Debug.Log("All levels complete — game finished!");
            return;
        }

        if (isCorrupted)
            Corrupt();

        LM.enemy = enemyOrder[currentLevel];
        LM.rewardNormalCard = (ItemCardData)rewardCardList[currentLevel].GetCopy();
        LM.rewardBadCard = LM.enemy.bcd;
        LM.targetScore = targetScoreList[currentLevel];
        LM.StartLevel();
    }

    public void EndLevel(bool win)
    {
        if (win)
        {
            currentLevel++;
        }
        else
        {
            currentLevel = 0;
            isCorrupted = false; // reset corruption on full restart
        }

        // CHANGED: pass fullReset=true on loss so Player.RestoreInitialCards() is called
        // instead of ResetCardStats() — this restores initial tokens and clears bought cards.
        // Original: LM.Reset();
        LM.Reset(fullReset: !win);
        Shop.instance.ResetShop();
        CardDeckManager.instance.LevelReset();
        StartLevel();
    }

    public void Corrupt()
    {
        isCorrupted = true;
        foreach (var card in Player.instance.itemCardDeck)
            card.comboDisabled = true;
    }
}
