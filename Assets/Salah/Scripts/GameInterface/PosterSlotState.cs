using UnityEngine;
using TMPro;

public class PosterSlotState : MonoBehaviour
{
    public CardData CurrentCard { get; private set; }
    public CardViewUI CurrentCardView { get; private set; }

    [SerializeField] private TMP_Text numberLabel;

    public bool HasCard => CurrentCard != null;

    public void SetCard(CardData data, CardViewUI view)
    {
        CurrentCard = data;
        CurrentCardView = view;

        RefreshLabel();
    }

    public void Clear()
    {
        CurrentCard = null;
        CurrentCardView = null;

        if (numberLabel != null)
            numberLabel.text = "";
    }

    public void RefreshLabel()
    {
        if (numberLabel == null)
            return;

        if (CurrentCard is EventCardData ec)
            numberLabel.text = $"x{ec.mult}";
        else if (CurrentCard is ItemCardData ic)
            numberLabel.text = $"+{ic.plus}";
        else
            numberLabel.text = "";
    }
}