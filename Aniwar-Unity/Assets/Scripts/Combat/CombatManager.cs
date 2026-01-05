using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Gem.OnGemDestroyed += HandleGemDestroyed;
    }

    private void OnDisable()
    {
        Gem.OnGemDestroyed -= HandleGemDestroyed;
    }


    private void HandleGemDestroyed(Gem gem)
    {
        SpawnBullet(gem);
    }

    private void SpawnBullet(Gem gem)
    {
        GameObject bullet = ObjectPooler.Instance.GetObject("Bullet", gem.transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().Init(gem.Variant.color, gem.column);
    }
}
