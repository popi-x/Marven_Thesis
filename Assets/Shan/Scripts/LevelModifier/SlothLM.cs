using UnityEngine;

[CreateAssetMenu(fileName = "SlothLM", menuName = "Scriptable Objects/LM/SlothLM")]
public class SlothLM : LevelModifier
{

    [SerializeField] private int _money = 1;
    [Header("plus multiplier")]
    [SerializeField] private double _pMult = 2;

    public override void Apply()
    {
        Player.instance.money = _money;

    }

    public override void DoublePlus()
    {
        LevelManager.instance.totalPlus *= _pMult;
    
    }


}
