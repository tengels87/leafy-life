using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildMenuItem : MonoBehaviour {
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

                MapController mapController = WorldConstants.Instance.getMapController();

                // position to build at
                Vector2 gridPos = MapController.pixelPos2WorldPos(Input.mousePosition);
                Vector2 buildPosition = new Vector2(gridPos.x, gridPos.y);

                bool isEmpty = mapController.isEmpty(new Vector2Int((int)buildPosition.x, (int)buildPosition.y));
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
                MapController.Tile walkableTile = mapController.getNearestWalkableTile(new Vector2Int((int)buildPosition.x, (int)buildPosition.y));
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
        }
    }
}
