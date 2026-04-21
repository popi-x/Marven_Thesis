using UnityEngine;

[CreateAssetMenu(fileName = "EnvyLM", menuName = "Scriptable Objects/LM/EnvyLM")]
public class EnvyLM : LevelModifier
{
    [SerializeField] private int _envyMult = 3;
    [SerializeField] private int _extraCost = 1;
    [SerializeField] private double _extraMult = 0.5;
    

    public override void Apply()
    {
        LevelManager.instance.envyMult = _envyMult;
        LevelManager.OnEnvyTrigger += HandleEnvyTrigger;
    }

    public override void Remove()
    {
        LevelManager.OnEnvyTrigger -= HandleEnvyTrigger;
    }


    private void HandleEnvyTrigger()
    {
        var CDM = CardDeckManager.instance;
        CDM.AddMultToEventCardsInShop(_extraMult);
        CDM.AddMultToEventCardsInPlayer(_extraMult);
        CDM.AddMultToEventDeck(_extraMult);

        Shop.instance.AddCostToCards(_extraCost);

    }


}
