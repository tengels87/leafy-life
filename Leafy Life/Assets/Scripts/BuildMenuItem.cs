using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildMenuItem : MonoBehaviour {
    public GameObject prefab;

    private Structure currentDragged;
    private GameObject dragVisualizer;
    private Sprite spriteIcon;
    private List<Vector2Int> currentBuildLocations;
    private List<GameObject> buildLocationVisualizers = new List<GameObject>();

    void Start() {
        spriteIcon = this.GetComponent<SpriteRenderer>().sprite;
    }

    void Update() {

        Vector2 mouseWorldPos = MapController.pixelPos2WorldPos(Input.mousePosition);// - Vector3.one * 16);

        // visualize drag with structure sprite
        if (currentDragged != null) {

            // position and snap to possible build location
            dragVisualizer.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
            foreach (Vector2Int loc in currentBuildLocations) {
                if (mouseWorldPos.x > loc.x && mouseWorldPos.x < loc.x + 1 && mouseWorldPos.y > loc.y && mouseWorldPos.y < loc.y + 1) {
                    dragVisualizer.transform.position = new Vector3(loc.x, loc.y, 0);
                    break;
                }
            }
        }

        // drop structure to build it
        if (Input.GetMouseButtonUp(0)) {
            if (currentDragged != null) {

                MapController mapController = WorldConstants.Instance.getMapController();

                // destroy location visualizers
                foreach (GameObject locationVis in buildLocationVisualizers) {
                    Destroy(locationVis);
                }
                buildLocationVisualizers.Clear();

                // position to build at
                Vector2Int buildPosition = new Vector2Int((int)mouseWorldPos.x, (int)mouseWorldPos.y);

                bool isEmpty = mapController.isEmpty(buildPosition);
                MapController.Tile tileAtBuildPosition = mapController.getTile(buildPosition);
                bool currentIsPlatform = currentDragged.gridFootprint[0].isWalkable;


                bool canBuildHere = (currentIsPlatform && isEmpty)
                    || (currentDragged.attachesToPlatform && tileAtBuildPosition != null && tileAtBuildPosition.isWalkable)
                    || (!currentDragged.attachesToSlot && tileAtBuildPosition != null && tileAtBuildPosition.isSlot);


                currentDragged = null;
                if (dragVisualizer != null) {
                    Destroy(dragVisualizer);
                }

                PlayerController playerController = WorldConstants.Instance.getPlayerController();

                // find valid tile under mouse

                // position to walk to before building
                MapController.Tile walkableTile = mapController.getNearestWalkableTile(buildPosition);
                if (canBuildHere && walkableTile != null) {
                    Vector2 walktoPosition = new Vector2(walkableTile.gridX, walkableTile.gridY);

                    // add walk and build actions{
                    GameAction gameAction = new GameAction(GameAction.ActionType.WALKTO, walktoPosition);
                    playerController.addAction(gameAction);

                    gameAction = new GameAction(GameAction.ActionType.BUILD, buildPosition);
                    gameAction.customData = prefab;
                    playerController.addAction(gameAction);
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

            // determine location where structure can be placed
            currentBuildLocations = mapController.getBuildLocations(structure.structureType);

            // clean and instantiate location visualizers
            foreach (GameObject locationVis in buildLocationVisualizers) {
                Destroy(locationVis);
            }
            buildLocationVisualizers.Clear();
            
            foreach (Vector2Int location in currentBuildLocations) {
                GameObject vis = mapController.createSpriteInstance(mapController.gridBackground, location.x, location.y);
                vis.transform.position += new Vector3(0, 0, 1);    // set z position, so it is shifted to background
                buildLocationVisualizers.Add(vis);
            }
        }
    }
}
