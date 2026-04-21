using UnityEngine;


[CreateAssetMenu(fileName = "BadItemCardData", menuName = "Scriptable Objects/CardData/BadItemCardData")]
public class BadItemCardData : ItemCardData
{
    public int corruptionCnt = 0;

    public double scoreMult => _scoreMult;

    [SerializeField] private double _scoreMult = 1.8;

}
