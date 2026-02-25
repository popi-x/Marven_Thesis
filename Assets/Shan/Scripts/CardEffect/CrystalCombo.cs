using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[CreateAssetMenu(fileName = "CrystalCombo", menuName = "Scriptable Objects/Combo/CrystalCombo")]
public class CrystalCombo : Combo
{


    public override void Execute(ItemCardData cd = null)
    {
        var playedItemCards = LevelManager.instance.playedItemCards;

        if (playedItemCards[0] == cd)
        {
            canPlay = false;
            Debug.Log("Combo Crystal cannot be played as the first card.");
        }
        else
        {
            var lastPlayedCard = playedItemCards[playedItemCards.Count - 1];
            LevelManager.instance.totalMult *= lastPlayedCard.plus;
            LevelManager.instance.totalPlus -= lastPlayedCard.plus;
        }
    }
}
