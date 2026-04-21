using UnityEngine;

public class CardFactory : MonoBehaviour
{
    public static CardFactory instance;

    [Header("Card Prefab")]
    public GameObject cardPrefab;

    [Tooltip("Size to enforce on every spawned card (overrides prefab RectTransform and any ContentSizeFitter).")]
    public Vector2 cardSize = new Vector2(150f, 330f);


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    /// <summary>
    /// Spawns a card and applies <paramref name="size"/> to its RectTransform.
    /// Omit <paramref name="size"/> (or pass null) to use the default <see cref="cardSize"/>.
    /// </summary>
    public Card Spawn(CardData data, Transform parent, Vector2? size = null)
    {
        Card card = Card.Spawn(cardPrefab, data, parent);
        if (card != null)
        {
            var rt = card.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = size ?? cardSize;

            CardViewUI view = card.GetComponent<CardViewUI>();
            if (view != null)
                view.Set(data);
        }
        return card;
    }


}
