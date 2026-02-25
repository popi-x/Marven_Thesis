using UnityEngine;


[CreateAssetMenu(fileName = "OldPhotoCombo", menuName = "Scriptable Objects/Combo/OldPhotoCombo")]
public class OldPhotoCombo : Combo
{
    public override void Execute(ItemCardData cd = null)
    {
        base.Execute(cd);
        if (LevelManager.instance.totalMult > 5)
        {
            var eventCard = CardDeckManager.instance.RandomDrawEventCards(1)[0];
            LevelManager.instance.PlayEventCard(eventCard);
        }
    }
}
