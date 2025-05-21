using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
    enum GrowthState {
        SEED,
        MATURE
    }

    public GameObject seedGameobject;
    public GameObject prefab_itemToSpawn;

    public float timeToGrow = 2;

    private GameObject plantGO;
    private bool isInstantiated = false;
    private float timeOfInit = 0;
    private float age = 0;
    private GrowthState growthState = GrowthState.SEED;

    void Start()
    {
        
    }

    void Update()
    {
        if (isInstantiated) {
            age = Time.time - timeOfInit;
        }

        if (age >= timeToGrow && growthState != GrowthState.MATURE) {
            setMature();
            spawnItem();
        }
    }

    public void init() {
        timeOfInit = Time.time;

        seedGameobject.SetActive(true);

        isInstantiated = true;
    }

    public void setMature() {
        growthState = GrowthState.MATURE;

        seedGameobject.SetActive(false);
    }

    private void spawnItem() {
        GameObject instance = Instantiate(prefab_itemToSpawn);

        instance.transform.SetParent(this.transform);
        instance.transform.localPosition = Vector3.zero;
    }
}
