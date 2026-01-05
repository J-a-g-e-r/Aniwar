using UnityEngine;

[CreateAssetMenu(fileName = "GemVariant", menuName = "Match-3/GemVariant", order = 1)]
public class GemVariant : ScriptableObject 
{
    public GemColor color;
    public GemType type = GemType.Normal;
    public Sprite sprite;
}

public enum GemColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange
}

public enum GemType
{
    Normal,
    AreaExplode,
    HorizontalExplode,
    VerticalExplode,
    ColorExplode
}