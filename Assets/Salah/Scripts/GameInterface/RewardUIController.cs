using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RewardUIController : MonoBehaviour
{
    public static RewardUIController instance;

    [Header("Panel")]
    [SerializeField] private GameObject rewardPanel;

    [Header("Card Option A")]
    [SerializeField] private CardViewUI cardViewA;
    [SerializeField] private Button chooseButtonA;

    [Header("Card Option B")]
    [SerializeField] private CardViewUI cardViewB;
    [SerializeField] private Button chooseButtonB;

    private List<ItemCardData> _rewardPool;

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
    }

    private void OnEnable()
    {
        LevelManager.OnRewardReady += CacheRewardPool;
    }

    private void OnDisable()
    {
        LevelManager.OnRewardReady -= CacheRewardPool;
    }

    // Called when OnRewardReady fires — stores the cards for later display
    private void CacheRewardPool(List<ItemCardData> pool)
    {
        _rewardPool = pool;
    }

    // Called by ResultUIController after player clicks "Claim Reward"
    public void OpenReward()
    {
        if (_rewardPool == null || _rewardPool.Count < 2)
        {
            Debug.LogWarning("RewardUIController: reward pool is empty or missing — skipping to next round.");
            GameManager.instance.EndLevel(true);
            return;
        }

        cardViewA?.Set(_rewardPool[0]);
        cardViewB?.Set(_rewardPool[1]);

        chooseButtonA?.onClick.RemoveAllListeners();
        chooseButtonA?.onClick.AddListener(() => OnCardChosen(0));

        chooseButtonB?.onClick.RemoveAllListeners();
        chooseButtonB?.onClick.AddListener(() => OnCardChosen(1));

        rewardPanel.SetActive(true);
    }

    private void OnCardChosen(int index)
    {
        if (_rewardPool == null || index >= _rewardPool.Count) return;

        LevelManager.instance.Claim(_rewardPool[index]);
        _rewardPool = null;

        rewardPanel.SetActive(false);
        GameManager.instance.EndLevel(true);
    }
}
