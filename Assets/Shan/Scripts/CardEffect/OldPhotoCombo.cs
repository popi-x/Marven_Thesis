using UnityEngine;


[CreateAssetMenu(fileName = "OldPhotoCombo", menuName = "Scriptable Objects/Combo/OldPhotoCombo")]
public class OldPhotoCombo : Combo
{

    public override void OnCardSubmit(ItemCardData cd = null)
    {
        base.OnCardSubmit();
        if (LevelManager.instance.totalMult > 5)
        {
            var eventCard = CardDeckManager.instance.RandomDrawEventCards(1)[0];
            eventCard.OnCardPlay();
        }
    }
}
