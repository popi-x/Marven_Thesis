using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public static Player instance;

    public int money = 5;

    public List<EventCardData> eventCardDeck = new List<EventCardData>();
    public List<ItemCardData> itemCardDeck = new List<ItemCardData>();

    [SerializeField] private List<EventCardData> initialEventCards = new List<EventCardData>(); //Player doesn't have intialEventCards in current design
    [SerializeField] private List<ItemCardData> initialItemCards = new List<ItemCardData>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // CHANGED: moved initial card setup from Start() to Awake() so tokens exist
        // before any other script's Start() runs (e.g. GameManager calling LM.Reset()
        // which triggers handUI.Rebuild() — previously tokens could be missing at that point).

        // Fix any cards assigned directly in Inspector without an original reference
        for (int i = 0; i < itemCardDeck.Count; i++)
            if (itemCardDeck[i] != null && itemCardDeck[i].original == null)
                itemCardDeck[i].original = itemCardDeck[i];

        // Add initial item tokens as runtime copies (no initial event cards per design)
        foreach (var ic in initialItemCards)
        {
            var copy = Instantiate(ic);
            copy.original = ic;
            itemCardDeck.Add(copy);
        }
    }

    // CHANGED: Start() previously handled initial card setup — moved to Awake() above.
    void Start()
    {
        /*
        for (int i = 0; i < itemCardDeck.Count; i++)
            if (itemCardDeck[i] != null && itemCardDeck[i].original == null)
                itemCardDeck[i].original = itemCardDeck[i];

        foreach (var ic in initialItemCards)
        {
            var copy = Instantiate(ic);
            copy.original = ic;
            itemCardDeck.Add(copy);
        }
        */
    }


    // Called on full game restart (loss) to wipe all acquired cards and restore starting state.
    // On a normal round-to-round reset, ResetCardStats() is used instead.
    public void RestoreInitialCards()
    {
        // Destroy and clear bought event cards (player has none at game start)
        foreach (var ec in eventCardDeck)
            if (ec != null) Destroy(ec);
        eventCardDeck.Clear();

        // Destroy and clear all item cards, then restore the initial tokens
        foreach (var ic in itemCardDeck)
            if (ic != null) Destroy(ic);
        itemCardDeck.Clear();

        foreach (var ic in initialItemCards)
        {
            var copy = Instantiate(ic);
            copy.original = ic;
            itemCardDeck.Add(copy);
        }

        // Reset money to starting amount
        money = 5;
    }

    public void ResetCardStats()
    {
        for (int i = 0; i < eventCardDeck.Count; i++)
        {
            var original = eventCardDeck[i].original as EventCardData;
            if (original == null) continue;
            Destroy(eventCardDeck[i]);
            var fresh = Instantiate(original);
            fresh.original = original;
            eventCardDeck[i] = fresh;
        }

        for (int i = 0; i < itemCardDeck.Count; i++)
        {
            var original = itemCardDeck[i].original as ItemCardData;
            if (original == null) continue;
            Destroy(itemCardDeck[i]);
            var fresh = Instantiate(original);
            fresh.original = original;
            itemCardDeck[i] = fresh;
        }
    }
}
