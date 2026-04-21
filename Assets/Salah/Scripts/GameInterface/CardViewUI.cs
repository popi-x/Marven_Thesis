using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardViewUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text line1;     // price/mult/plus
    [SerializeField] private TMP_Text line2;     // optional desc/tags later

    [Header("Badge")]
    [SerializeField] private TMP_Text typeBadge; // "EVENT" / "ITEM" (optional)

    public CardData Data { get; private set; }

    public void Set(CardData data)
    {
        Data = data;

        if (title) title.text = data ? data.cardName : "";
        if (icon) icon.sprite = data ? data.cardImage : null;

        // Defaults
        if (line1) line1.text = "";
        if (line2) line2.text = "";
        if (typeBadge) typeBadge.text = "";

        if (data == null) return;

        if (data is EventCardData ec)
        {
            if (typeBadge) typeBadge.text = "EVENT";
            if (line1) line1.text = $"Cost: {ec.price}   Mult: x{ec.mult}";
            if (line2) line2.text = string.IsNullOrEmpty(ec.narration) ? "" : ec.narration;
        }
        else if (data is ItemCardData ic)
        {
            if (typeBadge) typeBadge.text = "ITEM";
            if (line1) line1.text = $"+{ic.plus}";
            if (line2) line2.text = string.IsNullOrEmpty(ic.description) ? "" : ic.description;
        }
        else
        {
            // Fallback for any future CardData subclasses
            if (typeBadge) typeBadge.text = "CARD";
        }
    }
}