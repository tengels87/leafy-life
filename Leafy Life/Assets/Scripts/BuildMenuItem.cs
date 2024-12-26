using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildMenuItem : MonoBehaviour
{
    public GameObject prefab;

    private Structure currentDragged;
    private GameObject dragVisualizer;
    private Sprite spriteIcon;

    void Start() {
        spriteIcon = this.GetComponent<SpriteRenderer>().sprite;
    }

    void Update() {

        // visualize drag with structure sprite
        if (currentDragged != null) {
            dragVisualizer.transform.position = MapController.pixelPos2WorldPos(Input.mousePosition - Vector3.one * 16);
        }

        // drop structure to build it
        if (Input.GetMouseButtonUp(0)) {
            if (currentDragged != null) {
                currentDragged = null;
                if (dragVisualizer != null) {
                    Destroy(dragVisualizer);
                }

                MapController mapController = WorldConstants.Instance.getMapController();

                // find valid tile under mouse
                Vector2 gridPos = MapController.pixelPos2WorldPos(Input.mousePosition);

                if (mapController.isEmpty(new Vector2Int((int)gridPos.x, (int)gridPos.y))) {
                    mapController.buildTile((int)gridPos.x, (int)gridPos.y, prefab);
                }
            }
        }
    }

    private void OnMouseDown() {
        Structure structure = prefab.GetComponent<Structure>();
        if (structure != null) {
            currentDragged = structure;

            // instantiate drag visuls
            MapController mapController = WorldConstants.Instance.getMapController();
            dragVisualizer = mapController.createSpriteInstance(spriteIcon, 0, 0);
        }
    }
}
