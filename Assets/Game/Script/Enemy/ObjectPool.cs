using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();
    [SerializeField] private List<PoolItem> poolItems;

    [System.Serializable]
    private class PoolItem
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    private void Awake()
    {
        Instance = this;
        foreach (var item in poolItems)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < item.size; i++)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            pool.Add(item.tag, objectPool);
        }
    }

    public GameObject GetPooledObject(string tag)
    {
        if (!pool.ContainsKey(tag) || pool[tag].Count == 0) return null;
        GameObject obj = pool[tag].Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!pool.ContainsKey(tag)) return;
        obj.SetActive(false);
        pool[tag].Enqueue(obj);
    }
}