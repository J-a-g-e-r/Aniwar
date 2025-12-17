using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    [Header("Gem Properties")]
    public Vector2Int gridPosition;
    public int column;
    public int row;
    [Header("Visual Feedback")]

    private bool isSelected = false;
    public bool isMatch = false;
    private GridManager gridManager;
    private bool isBeingDestroyed = false;
    private SpriteRenderer gemSprite;
    void Start()
    {
        column = this.gridPosition.x;
        row = this.gridPosition.y;
        gridManager = FindObjectOfType<GridManager>();
        gemSprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Kiểm tra click chuột
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            // Kiểm tra xem có click vào gem này không
            if (GetComponent<Collider2D>().bounds.Contains(mousePos2D))
            {
                if (gridManager == null) return;
                // Thông báo cho GridManager về việc gem này đã được click
                gridManager.SwapGems(this);
            }
        }

        FindMatches();



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
        isSelected = selected;
         
        gemSprite.color = Color.green;
        if (isSelected == false)
            gemSprite.color = Color.white;
    }

    // Set gem có thể swap
    public void SetCanSwap(bool canSwap)
    {
        if (canSwap && !isSelected)
        {
            //spriteRenderer.color = canSwapColor;
        }
        else if (!isSelected)
        {
            //spriteRenderer.color = normalColor;
        }
    }

    public void FindMatches()
    {
        if (gridManager == null || gridManager._allGems == null) return;

        // Kiểm tra match ngang (horizontal)
        if (column > 0 && column < gridManager._width - 1)
        {
            GameObject leftGem = gridManager._allGems[column - 1, row];
            GameObject rightGem = gridManager._allGems[column + 1, row];

            if (leftGem != null && rightGem != null)
            {
                if (leftGem.tag == this.gameObject.tag && rightGem.tag == this.gameObject.tag)
                {
                    leftGem.GetComponent<Gem>().isMatch = true;
                    rightGem.GetComponent<Gem>().isMatch = true;
                    this.isMatch = true;
                }
            }
        }

        // Kiểm tra match dọc (vertical)
        if (row > 0 && row < gridManager._height - 1)
        {
            GameObject bottomGem = gridManager._allGems[column, row - 1];
            GameObject topGem = gridManager._allGems[column, row + 1];

            if (bottomGem != null && topGem != null)
            {
                if (bottomGem.tag == this.gameObject.tag && topGem.tag == this.gameObject.tag)
                {
                    bottomGem.GetComponent<Gem>().isMatch = true;
                    topGem.GetComponent<Gem>().isMatch = true;
                    this.isMatch = true;
                }
            }
        }
        if (isMatch)
        {
            // Thông báo để GridManager cập nhật lưới trước khi phá hủy
            if (!isBeingDestroyed && gridManager != null)
            {
                isBeingDestroyed = true;
                gridManager.RemoveGem(this);
            }
            
            Destroy(this.gameObject);
        }
    }
}
