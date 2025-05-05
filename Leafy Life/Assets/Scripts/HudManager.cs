using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudManager : MonoBehaviour
{
    public GameObject statsPanel;
    public GameObject buildPanel;

    public List<Object> buildMenuItems = new List<Object>();

    private Dictionary<MapController.MapType, List<GameObject>> buildMenuItemsDict = new Dictionary<MapController.MapType, List<GameObject>>();

    void OnEnable() {
        SceneManager.OnMapChanged += OnMapChangedEvent;
    }

    void OnDisable() {
        SceneManager.OnMapChanged -= OnMapChangedEvent;
    }

    void Start()
    {
        Camera cam = Camera.main;

        updateCameraFOV(8, 8);

        float visibleWorldWidth = 2 * cam.orthographicSize * cam.aspect;

        statsPanel.transform.localPosition = new Vector3(-0.5f * visibleWorldWidth, cam.orthographicSize, 2);
        buildPanel.transform.localPosition = new Vector3(-0.5f * visibleWorldWidth, cam.orthographicSize, 2);

        initBuildPanelItems();
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
        for (int i = 0; i < buildMenuItems.Count; i++) {
            GameObject instance = (GameObject)Instantiate(buildMenuItems[i]);
            instance.transform.SetParent(buildPanel.transform);

            BuildMenuItem menuItemData = instance.GetComponent<BuildMenuItem>();

            if (menuItemData != null) {
                if (!buildMenuItemsDict.ContainsKey(menuItemData.availableInMapType)) {
                    buildMenuItemsDict[menuItemData.availableInMapType] = new List<GameObject>();
                }
                buildMenuItemsDict[menuItemData.availableInMapType].Add(instance);
            }
        }
    }

    private void updateBuildPanelItemsVisibility(MapController.MapType mapType) {
        foreach (List<GameObject> list in buildMenuItemsDict.Values) {
            foreach (GameObject go in list) {
                go.SetActive(false);
            }
        }
        for (int i = 0; i < buildMenuItemsDict[mapType].Count; i++) {
            buildMenuItemsDict[mapType][i].transform.localPosition = new Vector3(0.75f, -2.5f - i * 1.2f);
            buildMenuItemsDict[mapType][i].SetActive(true);
        }
    }

    private void OnMapChangedEvent(MapController.MapType mapType) {
        updateBuildPanelItemsVisibility(mapType);
    }
}
