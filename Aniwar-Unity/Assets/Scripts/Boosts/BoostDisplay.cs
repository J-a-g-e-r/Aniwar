using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoostDisplay : MonoBehaviour
{
    public Boost boost;
    public TextMeshProUGUI manaText;
    public Image image;
    private Button button;
    
    void Start()
    {
        // Get or add Button component
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        
        // Set up button click listener
        button.onClick.AddListener(OnBoostClicked);
        
        // Update display
        if (boost != null)
        {
            if (manaText != null)
                manaText.text = boost.manaCost.ToString();
            if (image != null)
                image.sprite = boost.icon;
        }
    }
    
    private void OnBoostClicked()
    {
        if (boost == null)
        {
            Debug.LogWarning("Boost is null!");
            return;
        }
        
        if (BoostManager.Instance == null)
        {
            Debug.LogWarning("BoostManager.Instance is null!");
            return;
        }
        
        // Activate the boost
        BoostManager.Instance.ActivateBoost(boost);
    }
}
