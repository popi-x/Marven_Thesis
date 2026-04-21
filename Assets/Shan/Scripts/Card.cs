using UnityEngine;

public class Card : MonoBehaviour
{
    public CardData cardData;
    
    public static Card Spawn(GameObject prefab, CardData data, Transform parent)
    {
        if (data == null || parent == null) return null;
        Card card = Instantiate(prefab, parent).GetComponent<Card>();
        card.cardData = data;
        return card;
    }

    public void Play()
    {
        cardData.OnCardPlay();
    }

    public void Submit()
    {
        cardData.OnCardSubmit();
    }
}
