using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crop : MonoBehaviour {
    enum GrowthState {
        SEED,
        MATURE,
        RIPE_FRUIT
    }

    [SerializeField] private GameObject seedGameobject;
    [SerializeField] private GameObject matureGameobject;
    [SerializeField] private GameObject prefab_itemToSpawn;
    [SerializeField] private Transform itemToSpawnContainer;
    [SerializeField] private Structure parentStructure;
    public int numberOfItems = 1;
    public bool initializeOnStart = false;

    [Tooltip("units per ingame hour")]
    public float timeToGrow = 2;
    public float timeToFinish = 4;
    public bool destroyOnHarvest = true;

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

    public void setRipeState() {
        growthState = GrowthState.RIPE_FRUIT;

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
                instance.transform.SetParent(this.transform);
                harvestables.Add(instance);
            }
        }
    }

    private bool checkGatheredAll() {
        foreach (GameObject fruit in harvestables) {
            if (fruit != null) {
                ItemController collectable = fruit.GetComponent<ItemController>();
                if (collectable != null) {
                    return false;
                }
            }
        }

        return true;
    }

    private void OnHourTick(float timestamp) {

        if (growthState != GrowthState.RIPE_FRUIT) {

            /// grow 1 unit per hour
            age = age + 1;

            if (age >= timeToGrow && growthState == GrowthState.SEED) {
                setMatureState();
            }

            if (age >= timeToFinish && growthState == GrowthState.MATURE) {
                setRipeState();
            }
        } else {

            // check for harbested state
            bool gatheredAll = checkGatheredAll();

            if (gatheredAll) {

                if (destroyOnHarvest) {

                    // destroy parent, so crops lifecycle is finished
                    if (parentStructure != null) {
                        UUIDComponent uuidComponent = parentStructure.GetComponent<UUIDComponent>();
                        if (uuidComponent != null) {
                            WorldConstants.Instance.getLoadingManager().removeUserBuildTile(uuidComponent.UUID);
                        }

                        Object.Destroy(parentStructure.gameObject);
                    }
                } else {

                    // reset crop state
                    age = 0;
                    harvestables.Clear();
                    setSeedState();
                }
            }
        }
    }
}
