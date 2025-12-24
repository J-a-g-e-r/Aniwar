using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public static event Action<Gem> OnGemClicked;
    public static event Action<Gem> OnGemMatched;
    public static event Action<Gem> OnGemDestroyed;

    [Header("Gem Properties")]
    public Vector2Int gridPosition;
    public int column;
    public int row;
    public bool isMatched { get; private set; }


    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        OnGemClicked?.Invoke(this);
    }


    // Set vị trí trong component 
    public void SetGridPosition(int x, int y)
    {
        gridPosition = new Vector2Int(x, y);
        column = x;
        row = y;
    }

    // Set gem được chọn
    public void SetSelected(bool selected)
    {
        spriteRenderer.color = selected ? Color.gray : Color.white;
    }

    // Đánh dấu gem đã được match
    public void SetMatched(bool value)
    {
        if (isMatched == value) return;
        isMatched = value ;
        if (isMatched)
        {
            OnGemMatched?.Invoke(this);
        }
    }

    public void DestroyGem()
    {
        OnGemDestroyed?.Invoke(this);
        Destroy(gameObject);
    }
}


