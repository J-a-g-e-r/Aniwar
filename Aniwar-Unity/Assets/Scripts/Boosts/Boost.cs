using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoostType
{
    DestroyRowAndColumn,
    DestroyGem,
    Heal
}

[CreateAssetMenu(fileName = "New Boost", menuName = "Boosts/Boost")]
public class Boost : ScriptableObject
{
    public string boostName;
    public Sprite icon;
    public BoostType boostType;
    public int manaCost;

}
