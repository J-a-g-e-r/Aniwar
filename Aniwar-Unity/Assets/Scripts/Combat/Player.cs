using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Thêm dòng này

public class Player : MonoBehaviour
{
    public static event Action<int, int> OnHealthChanged;
    public static event Action<int, int> OnManaChanged;
    public static event Action OnPlayerDied;

    [Header("Player Stats")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP;
    [SerializeField] private int maxMana = 10;
    [SerializeField] private int currentMana;

    [Header("UI References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider manaBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private Animator animator; 
    
    
    

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.red;
    private Color originalColor;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize(int hp = -1, int mana = -1)
    {
        if (hp > 0) maxHP = hp;
        if (mana > 0) maxMana = mana;

        currentHP = maxHP;
        currentMana = maxMana;
        isDead = false;

        UpdateHealthBar();
        UpdateManaBar();
        UpdateText();

        OnHealthChanged?.Invoke(currentHP, maxHP);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Math.Max(0, currentHP);

        UpdateHealthBar();
        UpdateText();   
        OnHealthChanged?.Invoke(currentHP, maxHP);
        if (animator != null)
        {
            animator.SetTrigger("IsDamaged");
        }
        StartCoroutine(FlashEffect());
        UIManager.Instance.ShowDamage(damage);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void AddMana(int amount)
    {
        currentMana += amount;
        currentMana = Math.Min(currentMana, maxMana);

        UpdateManaBar();
        UpdateText();
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public bool UseMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            UpdateManaBar();
            UpdateText();
            OnManaChanged?.Invoke(currentMana, maxMana);
            return true;
        }
        return false;
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHP += amount;
        currentHP = Math.Min(currentHP, maxHP);

        UpdateHealthBar();
        UpdateText();
        UIManager.Instance.ShowHealAmount(amount);
        AudioManager.Instance.Heal();
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHP;
            healthBar.value = currentHP;
        }
    }

    private void UpdateManaBar()
    {
        if (manaBar != null)
        {
            manaBar.maxValue = maxMana;
            manaBar.value = currentMana;
        }
    }

    private void UpdateText()
    {
        healthText.text = $"{currentHP} / {maxHP}";
        manaText.text = $"{currentMana} / {maxMana}";
    }

    private IEnumerator FlashEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        OnPlayerDied?.Invoke();
        Debug.Log("Player died!");
    }

    public int GetCurrentHP() => currentHP;
    public int GetMaxHP() => maxHP;
    public int GetCurrentMana() => currentMana;
    public int GetMaxMana() => maxMana;
    public bool IsDead() => isDead;
}