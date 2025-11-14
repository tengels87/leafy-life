using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crop : MonoBehaviour {
    enum GrowthState {
        SEED,
        MATURE,
        DONE
    }

    public GameObject seedGameobject;
    public GameObject matureGameobject;
    public GameObject prefab_itemToSpawn;
    public Transform itemToSpawnContainer;
    public int numberOfItems = 1;
    public bool initializeOnStart = false;

    [Tooltip("units per ingame hour")]
    public float timeToGrow = 2;
    public float timeToFinish = 4;
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

    public void setDoneState() {
        growthState = GrowthState.DONE;

        seedGameobject.SetActive(false);
        matureGameobject.SetActive(false);

        spawnItem();
    }

    private void spawnItem() {
        if (prefab_itemToSpawn != null) {
            List<Transform> itemSlots = new List<Transform>();

            if (itemToSpawnContainer != null) {
                foreach (Transform t in itemToSpawnContainer.gameObject.GetComponentsInChildren<Transform>()) {
                    if (t != itemToSpawnContainer) {
                        itemSlots.Add(t);
                    }
                }
            } else {
                itemSlots.Add(this.transform);
            }

            foreach (Transform itemSlot in itemSlots) {
                GameObject instance = Instantiate(prefab_itemToSpawn);
                instance.transform.position = itemSlot.position;
                instance.transform.SetParent(null);
                harvestables.Add(instance);
            }
        }
    }

    public bool harvestAll() {
        bool canHarvest = (harvestables.Count > 0);
        if (canHarvest) {
            foreach (GameObject harvest in harvestables) {
                ItemController collectable = harvest.GetComponent<ItemController>();
                if (collectable != null) {
                    collectable.collect();
                }
            }

            // reset crop
            age = 0;
            harvestables.Clear();
            setSeedState();
        }

        return canHarvest;
    }

    private void OnHourTick(float timestamp) {

        if (growthState == GrowthState.DONE)
            return;

        /// grow 1 unit per hour
        age = age + 1;

        if (age >= timeToGrow && growthState == GrowthState.SEED) {
            setMatureState();
        }

        if (age >= timeToFinish && growthState == GrowthState.MATURE) {
            setDoneState();
        }
    }
}
