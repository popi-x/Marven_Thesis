using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUIController : MonoBehaviour
{
    public static ShopUIController instance;

    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;

    [Header("Header")]
    [SerializeField] private TMP_Text coinText;

    [Header("Card Slots (4 slots in order)")]
    [SerializeField] private List<CardViewUI> cardViews;
    [SerializeField] private List<Button> buyButtons;
    [SerializeField] private List<GameObject> cardSlotObjects;

    [Header("Buttons")]
    [SerializeField] private TMP_Text refreshButtonText;

    [Header("Slide Animation")]
    [Tooltip("How far off-screen the panel starts (positive = from the right).")]
    [SerializeField] private float slideDistance = 600f;
    [SerializeField] private float slideDuration = 0.3f;

    private Shop Shop => Shop.instance;
    private Player Player => Player.instance;

    private RectTransform _shopPanelRT;
    private Vector2 _panelRestPos;
    private Coroutine _slideRoutine;

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        _shopPanelRT = shopPanel != null ? shopPanel.GetComponent<RectTransform>() : null;
    }

    private void Start()
    {
        // Save rest position here, not in Awake — Canvas layout finishes before Start,
        // so anchoredPosition is the correct designed value by this point.
        if (_shopPanelRT != null)
            _panelRestPos = _shopPanelRT.anchoredPosition;

        // Hide the shop — leave it visible in the editor for positioning,
        // the game always opens it via OpenShopAnimated().
        shopPanel.SetActive(false);
    }

    // ── Open / Close ──────────────────────────────────────────────────────────

    /// <summary>Instant open — no animation (legacy / fallback).</summary>
    public void OpenShop()
    {
        shopPanel.SetActive(true);
        Refresh();
    }

    /// <summary>Opens the shop with a slide-in from the right.</summary>
    public void OpenShopAnimated()
    {
        if (_slideRoutine != null) StopCoroutine(_slideRoutine);
        shopPanel.SetActive(true);
        Refresh();
        if (_shopPanelRT != null)
            _slideRoutine = StartCoroutine(SlidePanel(_panelRestPos + new Vector2(slideDistance, 0f), _panelRestPos, slideDuration));
    }

    /// <summary>Closes the shop instantly.</summary>
    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    /// <summary>Closes the shop with a slide-out to the right.</summary>
    public void CloseShopAnimated()
    {
        if (_slideRoutine != null) StopCoroutine(_slideRoutine);
        if (_shopPanelRT != null)
            _slideRoutine = StartCoroutine(SlidePanel(_panelRestPos, _panelRestPos + new Vector2(slideDistance, 0f), slideDuration, hideOnDone: true));
    }

    private IEnumerator SlidePanel(Vector2 from, Vector2 to, float duration, bool hideOnDone = false)
    {
        _shopPanelRT.anchoredPosition = from;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            // Ease-out cubic — starts fast, slows into final position
            float eased = 1f - Mathf.Pow(1f - p, 3f);
            _shopPanelRT.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
            yield return null;
        }
        _shopPanelRT.anchoredPosition = to;
        if (hideOnDone) shopPanel.SetActive(false);
        _slideRoutine = null;
    }

    public void Refresh()
    {
        UpdateCoinDisplay();
        UpdateRefreshButton();
        UpdateCardSlots();
    }

    private void UpdateCoinDisplay()
    {
        if (coinText != null)
            coinText.text = $"{Player.money}";
    }

    private void UpdateRefreshButton()
    {
        if (refreshButtonText == null) return;

        if (Shop.freeRefreshCnt > 0)
            refreshButtonText.text = $"Refresh (Free x{Shop.freeRefreshCnt})";
        else
            refreshButtonText.text = $"Refresh ({Shop.refreshCost}g)";
    }

    private void UpdateCardSlots()
    {
        if (Shop == null)
        {
            Debug.LogError("ShopUIController: Shop.instance is null — is there a Shop GameObject in the scene?");
            return;
        }

        var cards = Shop.cardsInShop;

        for (int i = 0; i < cardSlotObjects.Count; i++)
        {
            if (cardSlotObjects[i] == null)
            {
                Debug.LogError($"ShopUIController: cardSlotObjects[{i}] is null — check Inspector assignment.");
                continue;
            }

            bool hasCard = i < cards.Count;
            cardSlotObjects[i].SetActive(hasCard);

            if (!hasCard) continue;

            if (cardViews[i] == null || buyButtons[i] == null)
            {
                Debug.LogError($"ShopUIController: cardViews[{i}] or buyButtons[{i}] is null — check Inspector assignment.");
                continue;
            }

            var card = cards[i];
            cardViews[i].Set(card);

            var buyLabel = buyButtons[i].GetComponentInChildren<TMP_Text>();
            if (buyLabel != null)
                buyLabel.text = $"{card.price}";

            int index = i;
            buyButtons[i].onClick.RemoveAllListeners();
            buyButtons[i].onClick.AddListener(() => BuyCardAt(index));

            buyButtons[i].interactable = Player.money >= card.price
                                      && Shop.buyCnt < Shop.maxBuyCnt;

            // Add/update ShopCardInteraction on the CardViewUI object (has the Image + raycast target)
            var interaction = cardViews[i].GetComponent<ShopCardInteraction>();
            if (interaction == null)
                interaction = cardViews[i].gameObject.AddComponent<ShopCardInteraction>();
            interaction.Init(card, i);
        }
    }

    /// <summary>
    /// Executes the purchase for the card at the given shop index.
    /// Called by the slot buy button AND by ShopPreviewController after the preview buy.
    /// </summary>
    public void BuyCardAt(int index)
    {
        var cards = Shop.cardsInShop;
        if (index >= cards.Count) return;

        var card = cards[index];
        if (Player.money < card.price || Shop.buyCnt >= Shop.maxBuyCnt) return;

        Player.money -= card.price;

        var copy = Instantiate(card);
        copy.original = card.original != null ? card.original : card;
        Player.eventCardDeck.Add(copy);

        Shop.cardsInShop.RemoveAt(index);
        Shop.buyCnt++;
        LevelManager.instance.enemy?.OnCardBought();

        LevelManager.instance.handUI?.Rebuild();
        Refresh();
    }

    public void OnRefreshPressed()
    {
        Shop.Refresh();
        Refresh();
    }
}
