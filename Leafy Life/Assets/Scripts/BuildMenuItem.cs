using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildMenuItem : MonoBehaviour {
    public GameObject prefab;

    private Structure currentDragged;
    private GameObject dragVisualizer;
    private Vector2 dragVisualizerOffset;
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

            // position visual at mouse position
            dragVisualizer.transform.position = new Vector3(mouseWorldPos.x + dragVisualizerOffset.x, mouseWorldPos.y + dragVisualizerOffset.y, -2);

            // snap visual to possible build location
            bool canBuild = false;
            Vector2Int currentBuildLocation = Vector2Int.zero;
            foreach (Vector2Int loc in currentBuildLocations) {
                if (mouseWorldPos.x > loc.x && mouseWorldPos.x < loc.x + 1 && mouseWorldPos.y > loc.y && mouseWorldPos.y < loc.y + 1) {
                    dragVisualizer.transform.position = new Vector3(loc.x, loc.y, 0);

                    currentBuildLocation = loc;
                    canBuild = true;

                    break;
                }
            }


            // drop structure to build it
            if (Input.GetMouseButtonUp(0)) {

                // destroy location visualizers
                foreach (GameObject locationVis in buildLocationVisualizers) {
                    Destroy(locationVis);
                }
                buildLocationVisualizers.Clear();

                currentDragged = null;
                if (dragVisualizer != null) {
                    Destroy(dragVisualizer);
                }


                if (canBuild) {
                    MapController mapController = WorldConstants.Instance.getMapController();

                    // position to walk to before building
                    MapController.Tile walkableTile = mapController.getNearestWalkableTile(currentBuildLocation);
                    if (walkableTile != null) {

                        // add walk and build actions
                        Vector2 walktoPosition = new Vector2(walkableTile.gridX, walkableTile.gridY);

                        PlayerController playerController = WorldConstants.Instance.getPlayerController();

                        GameAction gameAction = new GameAction(GameAction.ActionType.WALKTO, walktoPosition);
                        playerController.addAction(gameAction);

                        gameAction = new GameAction(GameAction.ActionType.BUILD, currentBuildLocation);
                        gameAction.customData = prefab;
                        playerController.addAction(gameAction);
                    }
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
            dragVisualizerOffset = new Vector2(this.transform.position.x, this.transform.position.y) - MapController.pixelPos2WorldPos(Input.mousePosition);

            // determine location where structure can be placed
            currentBuildLocations = mapController.getBuildLocations(structure.structureType);

            // clean and instantiate location visualizers
            foreach (GameObject locationVis in buildLocationVisualizers) {
                Destroy(locationVis);
            }
            buildLocationVisualizers.Clear();
            
            foreach (Vector2Int location in currentBuildLocations) {
                GameObject vis = mapController.createSpriteInstance(mapController.gridBackground, location.x, location.y);
                vis.transform.position += new Vector3(0, 0, -1);    // set z position, so it is shifted to background
                buildLocationVisualizers.Add(vis);
            }
        }
    }
}
