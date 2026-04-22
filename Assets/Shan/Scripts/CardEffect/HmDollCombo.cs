using UnityEngine;

[CreateAssetMenu(fileName = "HmDollCombo", menuName = "Scriptable Objects/Combo/HmDollCombo")]
public class HmDollCombo : Combo
{
    public override void OnCardPlay(ItemCardData cd = null)
    {
        base.OnCardPlay(cd);
        //draw event cards last played
        var pool = LevelManager.instance.playedEventCards;
        var ri = Random.Range(0, pool.Count);
        Player.instance.eventCardDeck.Add(pool[ri]);
        LevelManager.instance.handUI.Rebuild();
    }

    
}
