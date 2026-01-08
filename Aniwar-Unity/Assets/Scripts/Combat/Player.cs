using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static event Action<int, int> OnHealthChanged; // currentHP, maxHP
    public static event Action<int, int> OnManaChanged; // currentMana, maxMana
    public static event Action OnPlayerDied;

    [Header("Player Stats")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP;
    [SerializeField] private int maxMana = 10;
    [SerializeField] private int currentMana;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.red;
    private Color originalColor;

    private bool isDead = false;

    private void Awake()
    {
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

        OnHealthChanged?.Invoke(currentHP, maxHP);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Math.Max(0, currentHP);

        OnHealthChanged?.Invoke(currentHP, maxHP);
        StartCoroutine(FlashEffect());

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void AddMana(int amount)
    {
        currentMana += amount;
        currentMana = Math.Min(currentMana, maxMana);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public bool UseMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
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
        OnHealthChanged?.Invoke(currentHP, maxHP);
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
        // TODO: Trigger game over
    }

    // Getters
    public int GetCurrentHP() => currentHP;
    public int GetMaxHP() => maxHP;
    public int GetCurrentMana() => currentMana;
    public int GetMaxMana() => maxMana;
    public bool IsDead() => isDead;
}

