using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostManager : MonoBehaviour
{
    public static BoostManager Instance;
    
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Player player;


    [Header("VFX")]
    [SerializeField] private GameObject thunder;
    [SerializeField] private GameObject heal;

    private Boost activeBoost;
    private bool isWaitingForGemSelection = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }
        
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
    }
    
    public bool ActivateBoost(Boost boost)
    {
        if (boost == null)
        {
            Debug.LogWarning("Boost is null!");
            return false;
        }
        
        // Check if we're in a valid state to use boost
        if (!gridManager.StateManager.IsIdleState())
        {
            Debug.Log("Không thể sử dụng boost lúc này!");
            return false;
        }
        
        // Check if player has enough mana
        if (!player.UseMana(boost.manaCost))
        {
            UIManager.Instance.ShowInteract("Không đủ mana!");
            Debug.Log($"Không đủ mana! Cần {boost.manaCost} mana.");
            return false;
        }
        
        activeBoost = boost;

        // Handle different boost types
        switch (boost.boostType)
        {
            case BoostType.DestroyRowAndColumn:
            case BoostType.DestroyGem:
                // These boosts need gem selection
                isWaitingForGemSelection = true;
                // Change to boost selection state
                UIManager.Instance.ShowInteract("Chọn một viên ngọc");
                gridManager.StateManager.ChangeState(new BoostSelectionState(gridManager, this));
                break;
                
            case BoostType.Heal:
                // Heal boost doesn't need gem selection, execute immediately
                ExecuteHealBoost();
                activeBoost = null;
                break;
        }
        
        return true;
    }
    
    public void ExecuteBoostWithGem(Gem gem)
    {
        if (!isWaitingForGemSelection || activeBoost == null)
        {
            Debug.LogWarning("Không có boost đang chờ chọn gem!");
            return;
        }
            
        if (gem == null)
        {
            Debug.LogWarning("Gem is null!");
            return;
        }
        
        // Execute the boost based on type
        switch (activeBoost.boostType)
        {
            case BoostType.DestroyRowAndColumn:
                ExecuteDestroyRowAndColumnBoost(gem);
                break;
                
            case BoostType.DestroyGem:
                ExecuteDestroyGemBoost(gem);
                break;
        }
        
        // Reset boost state
        isWaitingForGemSelection = false;
        activeBoost = null;
    }
    
    private void ExecuteDestroyRowAndColumnBoost(Gem gem)
    {
        if (gem == null || gridManager == null)
            return;
        
        // Deselect the gem first
        gridManager.DeselectGem();
        
        // Clear the row and column of the selected gem
        if (BoardEffectManager.Instance != null)
        {
            BoardEffectManager.Instance.ClearRow(gem.row);
            BoardEffectManager.Instance.ClearColumn(gem.column);
            gem.SpawnDestroyColumnVFX();
            gem.SpawnDestroyRowVFX();
        }
        
        // Also mark the gem itself as matched to destroy it
        gem.SetMatched(true);
        
        // Trigger destroying state to process the matched gems
        gridManager.StateManager.ChangeState(new DestroyingState(gridManager));
    }
    
    private void ExecuteDestroyGemBoost(Gem gem)
    {
        if (gem == null || gridManager == null)
            return;
        
        // Deselect the gem first
        gridManager.DeselectGem();
        
        // Just mark the gem as matched to destroy it
        gem.SetMatched(true);
        if (thunder != null)
        {
            Instantiate(thunder, gem.transform.position, Quaternion.identity);
        }
        AudioManager.Instance.DestroyBoost();
        // Trigger destroying state to process the matched gem
        gridManager.StateManager.ChangeState(new DestroyingState(gridManager));
    }
    
    private void ExecuteHealBoost()
    {
        if (player == null)
            return;
        
        // Heal for 30 HP (you can adjust this value or make it configurable)
        int healAmount = 250;
        player.Heal(healAmount);
        if (heal != null)
        {
            Instantiate(heal, player.transform.position + new Vector3(0, -1f), Quaternion.identity);
        }
        Debug.Log($"Hồi máu {healAmount} HP!");
    }
    
    public void CancelBoostSelection()
    {
        isWaitingForGemSelection = false;
        activeBoost = null;
    }
    
    public bool IsWaitingForGemSelection()
    {
        return isWaitingForGemSelection;
    }
}

