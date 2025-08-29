using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

public class MapEditorTool : MonoBehaviour {
    [Header("Tile Prefabs")]
    public GameObject[] tilePrefabs;

    [Header("Placement Settings")]
    public Transform parentContainer;
    public int activePrefabIndex = 0; // The selected prefab in the palette

    public void PlaceTile(int x, int y) {
        if (tilePrefabs == null || tilePrefabs.Length == 0)
            return;

        // Avoid duplicates
        foreach (Transform child in parentContainer) {
            if (Mathf.RoundToInt(child.position.x) == x &&
                Mathf.RoundToInt(child.position.y) == y) {
                return;
            }
        }

        GameObject prefab = tilePrefabs[activePrefabIndex];

        MapController mapController = WorldConstants.Instance.getMapController();
        if (mapController != null) {
            GameObject buildable = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parentContainer);
            buildable.transform.position = new Vector3(x, y, 0);
        }

        // re-select tool
        Selection.activeGameObject = this.gameObject;
    }

    public void RemoveTile(int x, int y) {
        foreach (Transform child in parentContainer) {
            if (Mathf.RoundToInt(child.position.x) == x &&
                Mathf.RoundToInt(child.position.y) == y) {
                Object.DestroyImmediate(child.gameObject);
                break;
            }
        }
    }
}

#endif
