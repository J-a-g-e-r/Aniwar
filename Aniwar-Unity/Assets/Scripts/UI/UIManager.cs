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
    [SerializeField] private HealUI healPrefab;
    [SerializeField] private WaveUI waveUIPrefab;
    [SerializeField] private InteractUI interactPrefab;
    [SerializeField] private RectTransform canvasRoot;
    [SerializeField] private Camera mainCamera; 
    


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

    public void ShowHealAmount(int amount)
    {
        HealUI uI = Instantiate(healPrefab, canvasRoot);
        uI.transform.localPosition = new Vector3(-4.3f, -258f, 0);
        uI.Play(amount);
    }

    public void ShowInteract(string text)
    {
        InteractUI uI = Instantiate(interactPrefab, canvasRoot);
        uI.transform.localPosition = new Vector3(-64, -174f, 0);
        uI.Play(text);
    }

    public void ShowWaveUI(int waveIndex, int totalWave)
    {
        WaveUI uI = Instantiate(waveUIPrefab, canvasRoot);
        uI.transform.localPosition = new Vector3(0, 125, 0);
        uI.Play(waveIndex,totalWave);
    }

    public void ShowPointUI(int point, Gem gem)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(gem.transform.position);
        PointUI uI = ObjectPooler.Instance.GetObject("PointUI",Vector3.zero,Quaternion.identity).GetComponent<PointUI>();
        uI.transform.SetParent(canvasRoot, false);
        uI.transform.position = screenPos;
        uI.Play(point, gem.Variant.color);
    }
}
