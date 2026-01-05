using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public static event Action<Gem> OnGemClicked;
    public static event Action<Gem> OnGemDestroyed;

    [Header("Gem Properties")]
    public Vector2Int gridPosition;
    public int column;
    public int row;
    public bool isMatched { get; private set; }
    public GemVariant Variant { get; private set; }
    public bool IsSpecial => Variant.type != GemType.Normal;


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
    }

    public void DestroyGem()
    {
        ActivateSpecial();
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnObject("Gem",this.gameObject);
            OnGemDestroyed?.Invoke(this);
        }
    }

    public void Init(GemVariant variant)
    {
        Variant = variant;
        spriteRenderer.sprite = variant.sprite;
        ResetState();
    }

    public void ResetState()
    {
        isMatched = false;
        SetSelected(false);
    }

    private void ActivateSpecial()
    {
        if (Variant.type == GemType.Normal) return;

        switch (Variant.type)
        {
            case GemType.HorizontalExplode:
                BoardEffectManager.Instance.ClearRow(row);
                break;

            case GemType.VerticalExplode:
                BoardEffectManager.Instance.ClearColumn(column);
                break;

            //case GemType.AreaExplode:
            //    BoardEffectManager.Instance.ClearArea(column, row, 1);
            //    break;

            //case GemType.ColorExplode:
            //    BoardEffectManager.Instance.ClearColor(Variant.color);
            //    break;
        }
    }

}


