using UnityEngine;

[CreateAssetMenu(fileName = "NormalItemCardData", menuName = "Scriptable Objects/CardData/NormalItemCardData")]
public class NormalItemCardData : ItemCardData
{

    public override void OnCardPlay()
    {
        if (comboDisabled == true)
        {
            Debug.Log("combo is disabled");
            return;
        }
        else
        {
            base.OnCardPlay();
        }
    }

    public override void OnCardSubmit()
    {
        if (comboDisabled == true)
        {
            Debug.Log("combo is disabled");
            return;
        }
        else
        {
            base.OnCardSubmit();
        }
    }

}
