using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Match-3/LevelData", order = 1)]
public class LevelData : ScriptableObject
{
    [Header("Level Configuration")]
    public int levelNumber;
    
    [Header("Gem Variants (Normal Gems)")]
    [Tooltip("Danh sách Addressable keys cho các gem variants được sử dụng trong level này")]
    public List<string> gemVariantAddresses = new List<string>();
    
    [Header("Special Gem Variants")]
    [Tooltip("Danh sách Addressable keys cho các special gem variants được sử dụng trong level này")]
    public List<string> specialGemVariantAddresses = new List<string>();
    
    [Header("Grid Settings")]
    public int width = 8;
    public int height = 8;
}
