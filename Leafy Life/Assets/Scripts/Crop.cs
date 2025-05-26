using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crop : MonoBehaviour
{
    enum GrowthState {
        SEED,
        MATURE
    }

    public GameObject seedGameobject;
    public GameObject matureGameobject;
    public GameObject prefab_itemToSpawn;
    public bool initializeOnStart = false;

    public float timeToGrow = 2;

    private bool isInitialized = false;
    private float timeOfInit = 0;
    private float age = 0;
    private GrowthState growthState = GrowthState.SEED;
    private List<GameObject> harvestables = new List<GameObject>();

    void Start()
    {
        if (initializeOnStart) {
            init();
        }
    }

    void Update()
    {
        if (isInitialized) {
            age = Time.time - timeOfInit;
        }

        if (age >= timeToGrow && growthState != GrowthState.MATURE) {
            setMature();
            
            // spawn two items
            spawnItem();
            spawnItem();
        }
    }

    public void init() {
        if (!isInitialized) {
            timeOfInit = Time.time;

            seedGameobject.SetActive(true);
            matureGameobject.SetActive(false);

            isInitialized = true;
        }
    }

    public void setMature() {
        growthState = GrowthState.MATURE;

        seedGameobject.SetActive(false);
        matureGameobject.SetActive(true);
    }

    private void spawnItem() {
        GameObject instance = Instantiate(prefab_itemToSpawn);

        instance.transform.SetParent(this.transform);
        instance.transform.localPosition = Vector3.zero;

        harvestables.Add(instance);
    }

    public void harvestAll() {
        foreach (GameObject harvest in harvestables) {
            Collectable collectable = harvest.GetComponent<Collectable>();
            if (collectable != null) {
                collectable.collect();
            }
        }

        Object.Destroy(this.gameObject);
    }
}
