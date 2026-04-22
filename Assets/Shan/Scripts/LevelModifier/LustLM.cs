using UnityEngine;

[CreateAssetMenu(fileName = "LustLM", menuName = "Scriptable Objects/LM/LustLM")]
public class LustLM : LevelModifier
{

    [SerializeField] private int _extra = 1;

    public override void Apply()
    {
        var cards = Player.instance.eventCardDeck;
        var deckCards = Player.instance.eventCardDeck;
        cards.AddRange(deckCards);

        var linkCards = cards.FindAll(c => c.tags.Contains(EventCardTag.链接));
        foreach (var c in linkCards)
        {
            c.mult += _extra;
            c.multModified = true;
        }

        LevelManager.OnLevelEnd += HandleLevelEnd;
    }

    public override void Remove()
    {
        LevelManager.OnLevelEnd -= HandleLevelEnd;
    }

    private void HandleLevelEnd()
    {
        var itemDeck = Player.instance.itemCardDeck;
        if (itemDeck.Count == 0) return;

        int randomIndex = Random.Range(0, itemDeck.Count);
        ItemCardData removed = itemDeck[randomIndex];
        itemDeck.RemoveAt(randomIndex);
        Destroy(removed);
    }

}
