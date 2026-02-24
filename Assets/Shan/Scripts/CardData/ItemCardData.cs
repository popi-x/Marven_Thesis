using UnityEngine;

[CreateAssetMenu(fileName = "ItemCardData", menuName = "Scriptable Objects/CardData/ItemCardData")]
public class ItemCardData : CardData
{
    [SerializeField] private int _plus;
    [Header("combo effect")]
    //[SerializeField] private Combo _combo;

    public int plus => _plus;
    //public Combo combo => null; // TODO: implement combo
}
