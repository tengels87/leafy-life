using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crop : MonoBehaviour {
    enum GrowthState {
        SEED,
        MATURE
    }

    public GameObject seedGameobject;
    public GameObject matureGameobject;
    public GameObject prefab_itemToSpawn;
    public GameObject itemToSpawnContainer;
    public int numberOfItems = 1;
    public bool initializeOnStart = false;

    [Tooltip("units per ingame hour")]
    public float timeToGrow = 2;
    public bool destroyOnHarvest = false;

    private bool isInitialized = false;
    private float age = 0;
    private GrowthState growthState = GrowthState.SEED;
    private List<GameObject> harvestables = new List<GameObject>();

    void OnEnable() {
        DaytimeManager.hourTickEvent += OnHourTick;
    }

    void OnDisable() {
        DaytimeManager.hourTickEvent -= OnHourTick;
    }

    void Start() {
        if (initializeOnStart) {
            init();
        }
    }

    void Update() {

    }

    public void init() {
        if (!isInitialized) {
            setSeedState();

            isInitialized = true;
        }
    }

    public void setSeedState() {
        growthState = GrowthState.SEED;

        seedGameobject.SetActive(true);
        matureGameobject.SetActive(false);
    }

    public void setMatureState() {
        growthState = GrowthState.MATURE;

        seedGameobject.SetActive(false);
        matureGameobject.SetActive(true);
    }

    private void spawnItem() {
        if (prefab_itemToSpawn != null) {
            GameObject instance = Instantiate(prefab_itemToSpawn);

            if (itemToSpawnContainer != null) {
                instance.transform.SetParent(itemToSpawnContainer.transform);
            } else {
                instance.transform.SetParent(this.transform);
            }
            instance.transform.localPosition = Vector3.zero;

            harvestables.Add(instance);
        }
    }

    public void harvestAll() {
        if (harvestables.Count > 0) {
            foreach (GameObject harvest in harvestables) {
                Collectable collectable = harvest.GetComponent<Collectable>();
                if (collectable != null) {
                    collectable.collect();
                }
            }

            // reset crop
            age = 0;
            harvestables.Clear();
            setSeedState();
        }
    }

    private void OnHourTick(float timestamp) {

        if (growthState == GrowthState.MATURE)
            return;

        /// grow 1 unit per hour
        age = age + 1;

        if (age >= timeToGrow && growthState != GrowthState.MATURE) {
            setMatureState();

            // spawn items
            for (int i = 0; i < numberOfItems; i++) {
                spawnItem();
            }
        }
    }
}
