using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextMeshPool : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI prefab;
    [SerializeField] int poolSize = 5;
    Queue<TextMeshProUGUI> availableObjects = new();

    private void Awake()
    {
        GrowPool();
    }

    void GrowPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var instanceToAdd = Instantiate(prefab);
            instanceToAdd.transform.SetParent(transform, false);

            AddToPool(instanceToAdd);
        }
    }

    public void AddToPool(TextMeshProUGUI instance)
    {
        instance.gameObject.SetActive(false);
        availableObjects.Enqueue(instance);
    }

    public TextMeshProUGUI GetFromPool()
    {
        if (availableObjects.Count == 0)
        {
            GrowPool();
        }

        var instance = availableObjects.Dequeue();
        instance.gameObject.SetActive(true);
        return instance;
    }
}
