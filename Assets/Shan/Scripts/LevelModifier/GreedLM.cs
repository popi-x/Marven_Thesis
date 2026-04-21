using UnityEngine;

[CreateAssetMenu(fileName = "GreedLM", menuName = "Scriptable Objects/LM/GreedLM")]
public class GreedLM : LevelModifier
{
    [Header("Edited Shop Card Amount")]
    [SerializeField] private int _amt = 7;
    [Header("Limitations on cards player can buy")]
    [SerializeField] private int _cardLimit = 2;

    public override void Apply()
    {
        Shop.instance.cardCnt = _amt;
        Shop.instance.maxBuyCnt = _cardLimit;
    }
}
