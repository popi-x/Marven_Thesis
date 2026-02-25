using UnityEngine;

[CreateAssetMenu(fileName = "TrainTicketCombo", menuName = "Scriptable Objects/Combo/TrainTicketCombo")]
public class TrainTicketCombo: Combo
{
    [SerializeField] private ItemCardData _partner;
    [SerializeField] private int _plusAmount = 10;

    public override void Execute(ItemCardData cd = null)
    {
        base.Execute(cd);
        if (LevelManager.instance.playedItemCards.Contains(_partner))
        {
            LevelManager.instance.totalPlus += _plusAmount;
        }
    }
}
