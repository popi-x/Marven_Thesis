using UnityEngine;

[CreateAssetMenu(fileName = "HmDollCombo", menuName = "Scriptable Objects/Combo/HmDollCombo")]
public class HmDollCombo : Combo
{
    private bool _waiting = true;
    public override void OnCardPlay(ItemCardData cd = null)
    {
        base.OnCardPlay(cd);
        //draw event cards last played
        /*var pool = LevelManager.instance.playedEventCards;
        var ri = Random.Range(0, pool.Count);
        Player.instance.eventCardDeck.Add(pool[ri]);*/
        _waiting = true;
        LevelManager.OnNextECPlayed += AddNextEventCard;

    }

    private void AddNextEventCard(EventCardData ecd)
    {
        if (_waiting)
        {
            var copy = Instantiate(ecd);
            copy.disposable = true;
            Player.instance.eventCardDeck.Add(copy); // CHANGED: was ecd (the original), now correctly adds the disposable copy
            _waiting = false;
            LevelManager.OnNextECPlayed -= AddNextEventCard;
        }
    }
}
