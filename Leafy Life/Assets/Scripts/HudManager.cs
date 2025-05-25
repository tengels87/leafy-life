using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudManager : MonoBehaviour
{
    public GameObject statsPanel;
    public GameObject buildPanel;
    public GameObject furniturePanel;

    public List<GameObject> buildMenuItems = new List<GameObject>();
    public List<GameObject> furnitureMenuItems = new List<GameObject>();

    private Dictionary<MapController.MapType, List<GameObject>> buildMenuItemsDict = new Dictionary<MapController.MapType, List<GameObject>>();
    private Dictionary<MapController.MapType, List<GameObject>> furnitureMenuItemsDict = new Dictionary<MapController.MapType, List<GameObject>>();

    void OnEnable() {
        SceneManager.MapChangedEvent += OnMapChanged;
    }

    void OnDisable() {
        SceneManager.MapChangedEvent -= OnMapChanged;
    }

    void Start()
    {
        Camera cam = Camera.main;

        updateCameraFOV(8, 8);

        float visibleWorldWidth = 2 * cam.orthographicSize * cam.aspect;

        statsPanel.transform.localPosition = new Vector3(-0.5f * visibleWorldWidth, cam.orthographicSize, 2);
        buildPanel.transform.localPosition = new Vector3(-0.5f * visibleWorldWidth, cam.orthographicSize, 2);
        furniturePanel.transform.localPosition = new Vector3(-0.5f * visibleWorldWidth, cam.orthographicSize, 2);

        initBuildPanelItems();
        initFurniturePanelItems();
    }

    void Update()
    {
        
    }

    private void updateCameraFOV(int gridWidth, int gridHeight) {
        Camera cam = Camera.main;
        if (cam != null) {
            float aspect = (float)cam.pixelWidth / (float)cam.pixelHeight;

            int maxGridHeight = gridHeight;

            float zoom = (float)maxGridHeight / cam.orthographicSize;

            cam.orthographicSize = (float)maxGridHeight * 0.5f;

            cam.transform.position = new Vector3((float)(maxGridHeight) * aspect / zoom - 0.5f, (float)(maxGridHeight) / zoom - 0.5f, cam.transform.position.z);
        }
    }

    private void initBuildPanelItems() {
        foreach (GameObject item in buildMenuItems) {
            item.transform.SetParent(buildPanel.transform);

            // some items actually build something, others are just toggles for sub menues
            BuildMenuItem menuItemData = item.GetComponent<BuildMenuItem>();
            if (menuItemData != null) {
                if (!buildMenuItemsDict.ContainsKey(menuItemData.availableInMapType)) {
                    buildMenuItemsDict[menuItemData.availableInMapType] = new List<GameObject>();
                }
                buildMenuItemsDict[menuItemData.availableInMapType].Add(item);
            }
        }

        foreach (GameObject item in furnitureMenuItems) {
            item.transform.SetParent(furniturePanel.transform);

            // some items actually build something, others are just toggles for sub menues
            BuildMenuItem menuItemData = item.GetComponent<BuildMenuItem>();
            if (menuItemData != null) {
                if (!furnitureMenuItemsDict.ContainsKey(menuItemData.availableInMapType)) {
                    furnitureMenuItemsDict[menuItemData.availableInMapType] = new List<GameObject>();
                }
                furnitureMenuItemsDict[menuItemData.availableInMapType].Add(item);
            }
        }
    }

    private void initFurniturePanelItems() {
        for (int i = 0; i < furnitureMenuItems.Count; i++) {
            furnitureMenuItems[i].transform.SetParent(furniturePanel.transform);

            BuildMenuItem menuItemData = furnitureMenuItems[i].GetComponent<BuildMenuItem>();

            if (menuItemData != null) {
                furnitureMenuItems[i].transform.localPosition = new Vector3(2.25f, -2.5f - i * 1.2f);
                furnitureMenuItems[i].SetActive(true);
            }
        }
    }

    private void updateBuildPanelItemsVisibility(MapController.MapType mapType) {
        foreach (List<GameObject> list in buildMenuItemsDict.Values) {
            foreach (GameObject go in list) {
                BuildMenuItem buildMenuScript = go.GetComponent<BuildMenuItem>();
                if (buildMenuScript != null) {
                    buildMenuScript.setVisible(false);
                }
            }
        }

        foreach (List<GameObject> list in furnitureMenuItemsDict.Values) {
            foreach (GameObject go in list) {
                BuildMenuItem buildMenuScript = go.GetComponent<BuildMenuItem>();
                if (buildMenuScript != null) {
                    buildMenuScript.setVisible(true);
                }
            }
        }

        // show all build icons which do belong to mapType
        if (buildMenuItemsDict.ContainsKey(mapType)) {
            for (int i = 0; i < buildMenuItemsDict[mapType].Count; i++) {
                buildMenuItemsDict[mapType][i].transform.localPosition = new Vector3(0.75f, -2.5f - i * 1.2f);

                BuildMenuItem buildMenuScript = buildMenuItemsDict[mapType][i].GetComponent<BuildMenuItem>();
                if (buildMenuScript != null) {
                    buildMenuScript.setVisible(true);
                }
            }
        }
    }

    private void OnMapChanged(MapController.MapType mapType) {
        updateBuildPanelItemsVisibility(mapType);
    }
}
