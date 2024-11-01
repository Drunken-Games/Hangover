using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject layerPrefab;
    [SerializeField] private int poolSize = 25;
    
    private List<GameObject> pool;
    
    private void Awake()
    {
        pool = new List<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(layerPrefab, transform);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }
    
    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }
        return null;
    }
    
    public void ResetAllObjects()
    {
        foreach (var obj in pool)
        {
            obj.SetActive(false);
        }
    }
}