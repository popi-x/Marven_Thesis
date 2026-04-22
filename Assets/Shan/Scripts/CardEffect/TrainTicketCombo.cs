using UnityEngine;

[CreateAssetMenu(fileName = "TrainTicketCombo", menuName = "Scriptable Objects/Combo/TrainTicketCombo")]
public class TrainTicketCombo: Combo
{
    [SerializeField] private ItemCardData _partner;
    [SerializeField] private int _plusAmount = 10;

    public override void OnCardPlay(ItemCardData cd = null)
    {
        base.OnCardPlay();
        if (cd.bonusPlus > 0)
        {
            LevelManager.instance.totalPlus += cd.bonusPlus;
            return;
        }
        foreach (var card in Player.instance.itemCardDeck)
            if (card.original == _partner.original)
                card.bonusPlus = _plusAmount; 
        LevelManager.OnICPlayed += HandlePartnerPlayed;
    }

    private void HandlePartnerPlayed(ItemCardData card)
    {
        if (card.original == _partner.original)  
        {
            foreach (var c in Player.instance.itemCardDeck)
                if (c.combo is TyLetterCombo)
                    c.bonusPlus = 0;
            LevelManager.OnICPlayed -= HandlePartnerPlayed;
        }
    }

    public override void OnCardSubmit(ItemCardData cd = null)
    {
        base.OnCardSubmit(cd);
        LevelManager.OnICPlayed -= HandlePartnerPlayed;
    }

}

