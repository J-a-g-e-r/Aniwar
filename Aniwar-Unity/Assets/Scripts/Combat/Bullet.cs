using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    private GemColor color;
    private int column;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(GemColor gemColor, int gemColumn)
    {
        color = gemColor;
        column = gemColumn;
        SetColor(color);
    }

    private void SetColor(GemColor gemColor)
    {
        switch (gemColor)
        {
            case GemColor.Red:
                spriteRenderer.color = Color.red;
                break;
            case GemColor.Blue:
                spriteRenderer.color = Color.blue;
                break;
            case GemColor.Yellow:
                spriteRenderer.color = Color.yellow;
                break;
            case GemColor.Purple:
                spriteRenderer.color = new Color(0.5f, 0f, 0.5f); // Purple color
                break;
            case GemColor.Green:
                spriteRenderer.color = Color.green;
                break;
            case GemColor.Orange:
                spriteRenderer.color = new Color(1f, 0.5f, 0f); // Orange color
                break;
            default:
                spriteRenderer.color = Color.white;
                break;
        }
    }

    private void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
        if (ObjectPooler.Instance != null)
        {
            if (transform.position.y > Camera.main.orthographicSize + 1f)
            {
                ObjectPooler.Instance.ReturnObject("Bullet", this.gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
     
    }
}
