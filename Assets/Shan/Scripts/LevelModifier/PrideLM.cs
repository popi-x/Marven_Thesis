using UnityEngine;

[CreateAssetMenu(fileName = "PrideLM", menuName = "Scriptable Objects/LM/PrideLM")]
public class PrideLM : LevelModifier
{
    [SerializeField] private int _max = 2;
    [SerializeField] private int _extra = 1;


    public override void Apply()
    {
        LevelManager.instance.maxCardPlay = _max;
        LevelManager.OnECPlayed += HandleEventCardPlayed;
    }

    public override void Remove()
    {
        LevelManager.OnECPlayed -= HandleEventCardPlayed;
    }

    private void HandleEventCardPlayed(EventCardData card)
    {

        if (card.tags.Contains(EventCardTag.自我价值))
            LevelManager.instance.totalMult += _extra;
    } 
}
