using UnityEngine;
using System.Collections.Generic;

public class HandUIController : MonoBehaviour
{
    [Header("Content Parents")]
    [SerializeField] private Transform eventHandContent;
    [SerializeField] private Transform itemHandContent;

    // Smart rebuild: only destroys cards whose data left the deck,
    // and only spawns cards that have no live object yet.
    // This prevents duplicates when a card is mid-animation returning from the play area.
    public void Rebuild()
    {
        if (LevelManager.instance == null)
        {
            Debug.Log("REBUILD FAILED: LevelManager is null");
            return;
        }

        var player = Player.instance;
        var CF     = CardFactory.instance;

        SmartRebuildEvent(eventHandContent, player.eventCardDeck, CF);
        SmartRebuildItem (itemHandContent,  player.itemCardDeck,  CF);

        // Tell both fan layouts to re-arrange after all spawns are done
        eventHandContent?.GetComponent<HandFanLayout>()?.Refresh();
        itemHandContent ?.GetComponent<HandFanLayout>()?.Refresh();

        // Refresh combo highlights now that the hand's card list may have changed
        HandComboManager.instance?.RefreshHighlights();
    }

    private void Start() => Rebuild();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void SmartRebuildEvent(Transform content, List<EventCardData> deck, CardFactory CF)
    {
        if (content == null || deck == null) return;

        // Remove objects whose data is no longer in the event deck
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var cv = content.GetChild(i).GetComponent<CardViewUI>();
            if (cv == null || !(cv.Data is EventCardData ec) || !deck.Contains(ec))
                Object.Destroy(content.GetChild(i).gameObject);
        }

        // Build set of data that still has a live object
        var present = new HashSet<CardData>();
        foreach (Transform t in content)
        {
            var cv = t.GetComponent<CardViewUI>();
            if (cv?.Data != null) present.Add(cv.Data);
        }

        // Spawn only cards missing a live object
        foreach (var ec in deck)
            if (!present.Contains(ec))
                CF.Spawn(ec, content);
    }

    private static void SmartRebuildItem(Transform content, List<ItemCardData> deck, CardFactory CF)
    {
        if (content == null || deck == null) return;

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var cv = content.GetChild(i).GetComponent<CardViewUI>();
            if (cv == null || !(cv.Data is ItemCardData ic) || !deck.Contains(ic))
                Object.Destroy(content.GetChild(i).gameObject);
        }

        var present = new HashSet<CardData>();
        foreach (Transform t in content)
        {
            var cv = t.GetComponent<CardViewUI>();
            if (cv?.Data != null) present.Add(cv.Data);
        }

        foreach (var ic in deck)
            if (!present.Contains(ic))
                CF.Spawn(ic, content);
    }
}
