using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script demo để test mechanic swap gem
/// Cung cấp các method để test và debug
/// </summary>
public class SwapDemo : MonoBehaviour
{
    [Header("Demo Settings")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("Test Controls")]
    [SerializeField] private KeyCode resetGridKey = KeyCode.R;
    [SerializeField] private KeyCode randomSwapKey = KeyCode.Space;
    
    private void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
            
        if (enableDebugLogs)
            Debug.Log("SwapDemo: Ready to test gem swapping mechanic!");
    }
    
    private void Update()
    {
        // Test controls
        if (Input.GetKeyDown(resetGridKey))
        {
            ResetGrid();
        }
        
        if (Input.GetKeyDown(randomSwapKey))
        {
            PerformRandomSwap();
        }
    }
    
    /// <summary>
    /// Reset grid về trạng thái ban đầu
    /// </summary>
    public void ResetGrid()
    {
        if (enableDebugLogs)
            Debug.Log("SwapDemo: Resetting grid...");
            
        // Có thể thêm logic reset grid ở đây
        // Hiện tại chỉ log để demo
    }
    
    /// <summary>
    /// Thực hiện swap ngẫu nhiên 2 gem
    /// </summary>
    public void PerformRandomSwap()
    {
        if (gridManager == null) return;
        
        if (enableDebugLogs)
            Debug.Log("SwapDemo: Performing random swap...");
            
        // Logic để tìm 2 gem ngẫu nhiên và swap
        // Có thể implement sau khi có reference đến grid
    }
    
    /// <summary>
    /// Log thông tin về gem được chọn
    /// </summary>
    public void LogGemInfo(Gem gem)
    {
        if (!enableDebugLogs) return;
        
        //Debug.Log($"Gem Info - Position: ({gem.gridPosition.x}, {gem.gridPosition.y}), Type: {gem.gemType}");
    }
    
    /// <summary>
    /// Log thông tin về swap operation
    /// </summary>
    public void LogSwapInfo(Gem gem1, Gem gem2)
    {
        if (!enableDebugLogs) return;
        
        Debug.Log($"Swap Info - Gem1: ({gem1.gridPosition.x}, {gem1.gridPosition.y}) <-> Gem2: ({gem2.gridPosition.x}, {gem2.gridPosition.y})");
    }
}
