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
    private Color originalColor;


    [Header("Setting")]
    [SerializeField] private int startColumn;
    [SerializeField] private int columnWidth;


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
        currentHP = Math.Max(0,currentHP);

        StartCoroutine(FlashEffect());

        if (currentHP <= 0)
        {
            Die();
        }
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
        // Thêm hiệu ứng chết ở đây nếu cần
        if(ObjectPooler.Instance != null)
        {
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
