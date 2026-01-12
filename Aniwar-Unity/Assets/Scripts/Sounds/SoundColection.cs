using UnityEngine;

[CreateAssetMenu]
public class SoundColection : ScriptableObject
{
    [Header("Music")]
    public SoundData[] MusicGameplay;


    [Header("SFX")]
    public SoundData[] ButtonPress;
    public SoundData[] CandyLand;
    public SoundData[] CreateChocolate;
    public SoundData[] Combo;
    public SoundData[] Exclaimations;
    public SoundData[] LineBlast;
    public SoundData[] Swap;
    public SoundData[] WrapCandy;
    public SoundData[] WrongSwap;
    public SoundData[] Crack;
    public SoundData[] Bomb;
    public SoundData[] ColorBomb;
}

