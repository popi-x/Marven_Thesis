using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[CreateAssetMenu(fileName = "FreeTimeCombo", menuName = "Scriptable Objects/Combo/FreeTimeCombo")]
public class FreeTimeCombo : Combo
{
    [Header("Shop Refresh Times Gained")]
    [SerializeField] private int _cnt = 2;
   public override void Execute(ItemCardData cd = null)
    {
        base.Execute(cd);
        if (LevelManager.instance.playedItemCards.Count == 1)
        {
            LevelManager.instance.shopRefreshTimes += _cnt;
        }
    }
}
