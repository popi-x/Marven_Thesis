using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[CreateAssetMenu(fileName = "CrystalCombo", menuName = "Scriptable Objects/Combo/CrystalCombo")]
public class CrystalCombo : Combo
{
    public override void OnCardPlay(ItemCardData cd = null)
    {
        var playedItemCards = LevelManager.instance.playedItemCards;

        // CHANGED: was playedItemCards[0] == cd which crashes when list is empty
        if (playedItemCards.Count == 0)
        {
            LevelManager.instance.CardCannotPlay("Crystal cannot be played as the first card.");
            return;
        }

        var lastPlayedCard = playedItemCards[playedItemCards.Count - 1];
        LevelManager.instance.totalMult *= lastPlayedCard.plus;
        LevelManager.instance.totalPlus -= lastPlayedCard.plus;
        playedItemCards.Add(cd);
    }
}
