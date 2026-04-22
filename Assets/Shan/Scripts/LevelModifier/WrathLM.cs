using UnityEngine;

[CreateAssetMenu(fileName = "WrathLM", menuName = "Scriptable Objects/LM/WrathLM")]
public class WrathLM : LevelModifier
{
    [SerializeField] private double _prob = 0.5;
    
    public override void Apply()
    {
        LevelManager.OnECPlayed += HandleEventCardPlayed;
    }

     private void HandleEventCardPlayed(EventCardData card)
    {
        if (Random.value > _prob) return; 

        var itemDeck = Player.instance.itemCardDeck;
        if (itemDeck.Count == 0) return;

        int randomIndex = Random.Range(0, itemDeck.Count);
        Destroy(itemDeck[randomIndex]);
        itemDeck.RemoveAt(randomIndex);
    }
}
