using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultUIController : MonoBehaviour
{
    public static ResultUIController instance;

    [Header("Panel")]
    [SerializeField] private GameObject resultPanel;

    [Header("Score Display")]
    [SerializeField] private TMP_Text scoreText;       // e.g. "Score: 24"
    [SerializeField] private TMP_Text targetText;      // e.g. "Target: 20"
    [SerializeField] private TMP_Text breakdownText;   // e.g. "3.0 x 8 = 24"
    [SerializeField] private TMP_Text outcomeText;     // "YOU WIN" or "YOU LOSE"

    [Header("Buttons")]
    [SerializeField] private GameObject winButtons;    // parent holding the "Claim Reward" button
    [SerializeField] private GameObject loseButtons;   // parent holding the "Try Again" button

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
    }

    private void OnEnable()
    {
        LevelManager.OnResultReady += ShowResult;
    }

    private void OnDisable()
    {
        LevelManager.OnResultReady -= ShowResult;
    }

    private void ShowResult(bool won)
    {
        var lm = LevelManager.instance;

        if (scoreText)    scoreText.text    = $"Score: {lm.curScore:F0}";
        if (targetText)   targetText.text   = $"Target: {lm.targetScore:F0}";
        if (breakdownText) breakdownText.text = $"{lm.totalMult:F2} x {lm.totalPlus:F2} = {lm.curScore:F0}";
        if (outcomeText)  outcomeText.text  = won ? "YOU WIN!" : "YOU LOSE";

        if (winButtons)  winButtons.SetActive(won);
        if (loseButtons) loseButtons.SetActive(!won);

        resultPanel.SetActive(true);
    }

    // Called by "Claim Reward" button (win path) — opens reward panel
    public void OnClaimRewardPressed()
    {
        resultPanel.SetActive(false);
        RewardUIController.instance?.OpenReward();
    }

    // Called by "Try Again" button (lose path) — resets the game
    public void OnTryAgainPressed()
    {
        resultPanel.SetActive(false);
        GameManager.instance.EndLevel(false);
    }
}
