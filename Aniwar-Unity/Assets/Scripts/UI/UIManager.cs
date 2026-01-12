using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Sprite Exclaimations")]
    public Sprite tasty;
    public Sprite delicious;
    public Sprite sweet;
    public Sprite divine;



    [Header("References")]
    [SerializeField] private ExclamationUI exclaimationPrefab;
    [SerializeField] private DamageUI damagePrefab;
    [SerializeField] private RectTransform canvasRoot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowExclaimation(int matchCount)
    {
        Sprite sprite = null;
        if (matchCount == 3)
        {
            sprite = sweet;
        }
        else if (matchCount == 4)
        {
            sprite = tasty;
        }
        else if (matchCount == 5)
        {
            sprite = delicious;
        }
        else if (matchCount >5)
        {
            sprite = divine;
        }

        if(sprite == null)
        {
            return;
        }

        ExclamationUI ui = Instantiate(exclaimationPrefab, canvasRoot);
        ui.transform.localPosition = new Vector3(0, -75, 0);
        ui.Play(sprite);
    }

    public void ShowDamage(int damage)
    {
        DamageUI ui = Instantiate(damagePrefab, canvasRoot);
        ui.transform.localPosition = new Vector3(-4.3f, -258f, 0);
        ui.Play(damage);
    }
}
