using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[CreateAssetMenu(fileName = "FreeTimeCombo", menuName = "Scriptable Objects/Combo/FreeTimeCombo")]
public class FreeTimeCombo : Combo
{
    [Header("Shop Refresh Times Gained")]
    [SerializeField] private int _cnt = 3;
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

   public override void OnCardSubmit(ItemCardData cd = null)
    {
        if (LevelManager.instance.playedItemCards.Count == 1)
        {
            Shop.instance.freeRefreshCnt += _cnt;
        }
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
}
