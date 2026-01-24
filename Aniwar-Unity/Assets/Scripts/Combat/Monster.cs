using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public static event Action<Monster> OnMonsterDied;


    [Header("Monster Properties")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP;
    [SerializeField] private int damage = 10;
    private bool isDead = false;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private Animator animator;
    [SerializeField] HealthBar healthBar;
    private Color originalColor;


    [Header("Setting")]
    [SerializeField] private int startColumn;
    [SerializeField] private int columnWidth;

    [SerializeField] private MonsterProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform player;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        healthBar = GetComponentInChildren<HealthBar>();
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
        healthBar.UpdateHealthBar(currentHP, maxHP);
    }

    public void Init(int hp, int damage, int column, int width)
    {
        maxHP = hp;
        currentHP = maxHP;
        this.damage = damage;
        startColumn = column;
        columnWidth = width;
        isDead = false;
    }

    public void SetupCollider(int column, int width)
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager != null)
        {
            float space = gridManager.GetSpace();
            collider.size = new Vector2(width * space, space);
            collider.offset = new Vector2((width - 1) * space * 0.5f, 0);
        }
        else
        {
            float space = 0.77f;
            collider.size = new Vector2(width * space, space);
            collider.offset = new Vector2((width - 1) * space * 0.5f, 0);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHP -= damage;
        healthBar.UpdateHealthBar(currentHP, maxHP);
        currentHP = Math.Max(0,currentHP);

        if(animator != null)
        {
            animator.SetTrigger("IsDamaged");
        }
        StartCoroutine(FlashEffect());

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // Monster tự tấn công player (dùng để dễ gắn animation)
    public void Attack(Player player)
    {
        if (isDead || player == null || player.IsDead())
            return;

        // Trigger animation tấn công (nếu có)
        if (animator != null)
        {
            animator.SetTrigger("IsAttack");
        }

        MonsterProjectile prj= Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        prj.Init(player.transform);
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
        OnMonsterDied?.Invoke(this);
        if(animator != null)
        {
            animator.SetTrigger("IsDead");
        }
        StartCoroutine(DelayReturnToPool());
    }

    private IEnumerator DelayReturnToPool()
    {
        yield return new WaitForSeconds(1f);
        if (ObjectPooler.Instance != null)
        {
            // Trả về pool chung "Monster"
            ObjectPooler.Instance.ReturnObject("Monster", this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public int GetCurrentHP() => currentHP;
    public int GetMaxHP() => maxHP;
    public int GetDamage() => damage;
    public bool IsDead() => isDead;
    public int GetStartColumn() => startColumn;
    public int GetColumnWidth() => columnWidth;

}
