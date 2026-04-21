using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "SodaIceCombo", menuName = "Scriptable Objects/Combo/SodaIceCombo")]
public class SodaIceCombo : Combo
{
    [SerializeField] private ItemCardData _partner;
    [SerializeField] private int _plusAmount = 6;

    public override void OnCardPlay(ItemCardData cd = null)
    {
        base.OnCardPlay(cd);
        if (cd.bonusPlus > 0)
        {
            LevelManager.instance.totalPlus += cd.bonusPlus;
            return;
        }
        foreach (var card in Player.instance.itemCardDeck)
            if (card.original == _partner.original)
                card.bonusPlus = _plusAmount; 
        LevelManager.OnICPlayed += HandleLetterPlayed;
    }

    private void HandleLetterPlayed(ItemCardData card)
    {
        if (card.original == _partner.original)  
        {

            foreach (var c in Player.instance.itemCardDeck)
                if (c.combo is TyLetterCombo)
                    c.bonusPlus = 0;
            LevelManager.OnICPlayed -= HandleLetterPlayed;
        }
    }
}
