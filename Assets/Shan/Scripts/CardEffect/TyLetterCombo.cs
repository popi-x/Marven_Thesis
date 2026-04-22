using UnityEngine;

[CreateAssetMenu(fileName = "TyLetterCombo", menuName = "Scriptable Objects/Combo/TyLetterCombo")]
public class TyLetterCombo: Combo
{
    [SerializeField] private ItemCardData _partner;
    [SerializeField] private int _plusAmount = 6;

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
        LevelManager.OnICPlayed += HandleSodaICePlayed;
    }

    public override void OnCardSubmit(ItemCardData cd = null)
    {
        LevelManager.OnICPlayed -= HandleSodaICePlayed;
        base.OnCardSubmit(cd);
    }

    private void HandleSodaICePlayed(ItemCardData card)
    {
        if (card.original == _partner.original)  
        {
            foreach (var c in Player.instance.itemCardDeck)
                if (c.combo is TyLetterCombo)
                    c.bonusPlus = 0;
            LevelManager.OnICPlayed -= HandleSodaICePlayed;
        }
    }
}
