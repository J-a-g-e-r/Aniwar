using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefabs;
        public int size;
    }


    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Khởi tạo poolDictionary trong Awake để đảm bảo sẵn sàng trước khi Start() của các script khác chạy
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefabs);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject GetObject(string tag, Vector3 position, Quaternion rotation)
    {
        if (poolDictionary == null)
        {
            Debug.LogError("PoolDictionary is not initialized!");
            return null;
        }

        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        if (poolDictionary[tag].Count == 0)
        {
            Debug.LogWarning("Pool with tag " + tag + " is empty. Consider increasing pool size.");
            return null;
        }

        GameObject obj = poolDictionary[tag].Dequeue();
        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }

    public void ReturnObject(string tag, GameObject obj)
    {
        if (obj == null) return;

        if (poolDictionary == null)
        {
            Debug.LogError("PoolDictionary is not initialized!");
            return;
        }

        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist. Cannot return object.");
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolDictionary[tag].Enqueue(obj);
    }
}
