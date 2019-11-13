using System.Collections.Generic;
using Narupa.Frame;
using NarupaXR;
using UnityEngine;
using UnityEngine.Assertions;

public class VisualisationManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] visualiserPrefabs;

    [SerializeField]
    private GameObject startingPrefab;

    [SerializeField]
    private NarupaXRPrototype prototype;

    private GameObject currentVisualiser;
    
    public IEnumerable<GameObject> GetVisualiserPrefabs()
    {
        return visualiserPrefabs;
    }

    public void Start()
    {
        Assert.IsNotNull(prototype);
        SpawnVisualiser(startingPrefab);
    }

    public void SpawnVisualiser(GameObject prefab)
    {
        if (currentVisualiser != null)
            Destroy(currentVisualiser);
        if (prefab != null)
        {
            currentVisualiser = Instantiate(prefab, transform);
            currentVisualiser.GetComponent<IFrameConsumer>().FrameSource =
                prototype.FrameSynchronizer;
        }
    }
}