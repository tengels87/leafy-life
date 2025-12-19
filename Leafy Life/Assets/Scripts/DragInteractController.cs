using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;

public class DragInteractController : MonoBehaviour
{
    private GameObject currentDragged;
    protected GameObject dragVisualsInstance;
    protected PrefabDef currentPrefabDef;
    protected ItemData currentItemDataDef;
    private List<Vector2Int> currentBuildLocations = new List<Vector2Int>();
    private List<Structure.GridFootprint> currentFootprint = new List<Structure.GridFootprint>();
    private List<GameObject> buildLocationVisualizers = new List<GameObject>();

    protected virtual void OnEnable() {
        UnifiedInputModule.Instance.OnPress += OnPointerPressed;
    }

    protected virtual void OnDisable() {
        UnifiedInputModule.Instance.OnPress -= OnPointerPressed;
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update() {
        // visualize drag with structure sprite
        if (currentDragged != null) {

            // position visual at mouse position
            Vector2 pointerPos = UnifiedInputModule.Instance.PointerPosition;
            Vector2 mouseWorldPos = MapController.pixelPos2WorldPos(pointerPos);
            dragVisualsInstance.transform.position = new Vector3(mouseWorldPos.x - 0.5f, mouseWorldPos.y - 0.5f, 0);

            handleDragging();
        }
    }

    private void OnPointerPressed(Vector2 tapPos) {
        if (SceneManager.Instance.isSceneLoading()) {
            return;
        }

        if (GlobalRaycast.IsTappedInWorld(this.gameObject)) {
            handleTappedInWorld();
        }

        if (GlobalRaycast.IsTappedInUI(this.gameObject)) {
            handleTappedInUI(null);
        }
    }

    protected virtual void handleTappedInWorld() {

    }

    protected virtual void handleTappedInUI(Object dragVisual) {
        currentDragged = this.gameObject;

        // instantiate drag visuals
        this.dragVisualsInstance = dragVisual.GetType() == typeof(Sprite)
            ? instantiateSpriteInWorld((Sprite)dragVisual)
            : Instantiate((GameObject)dragVisual);

        dragVisualsInstance.transform.position = new Vector3(0, 0, -100);

        if (currentPrefabDef != null) {
            Structure structure = currentPrefabDef.Prefab.GetComponent<Structure>();

            if (structure != null) {

                // get all locations where structure can be placed
                MapController mapController = WorldConstants.Instance.getMapController();
                currentBuildLocations = mapController.getBuildLocations(structure);
                currentFootprint = structure.gridFootprint;

                // clean and instantiate location visualizers
                foreach (GameObject locationVis in buildLocationVisualizers) {
                    Destroy(locationVis);
                }
                buildLocationVisualizers.Clear();

                foreach (Vector2Int location in currentBuildLocations) {
                    GameObject vis = mapController.createSpriteInstance(mapController.gridBackground, location.x + 0.5f, location.y + 0.5f);
                    vis.GetComponent<SpriteRenderer>().sortingOrder = 9;    // shift to background
                    buildLocationVisualizers.Add(vis);
                }
            }
        }
    }

    private void handleDragging() {
        PlayerController playerController = WorldConstants.Instance.getPlayerController();
        Shopkeeper[] shopKeepers = GameObject.FindObjectsOfType<Shopkeeper>();

        // check if being dropped on player or shop etc.
        bool draggedOntoPlayer = false;
        bool draggedOntoShop = false;
        bool canBuild = false;
        Vector2Int currentBuildLocation = Vector2Int.zero;

        if (currentItemDataDef != null && currentItemDataDef.itemType == ItemData.ItemType.FOOD) {
            if (Vector3.Distance(dragVisualsInstance.transform.position, playerController.transform.position + new Vector3(0.5f, 0.75f, 0)) < 0.8f) {
                draggedOntoPlayer = true;
            }
        }

        if (draggedOntoPlayer) {
            dragVisualsInstance.transform.DOKill();
            dragVisualsInstance.transform.DOScale(2.0f, 0.1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                dragVisualsInstance.transform.DOScale(1f, 0.1f)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => { });
                });
        } else {

            // snap visual to possible build location
            Vector2 pointerPos = UnifiedInputModule.Instance.PointerPosition;
            Vector2 mouseWorldPos = MapController.pixelPos2WorldPos(pointerPos);
            bool canFullyBuild = true;

            foreach (Vector2Int loc in currentBuildLocations) {
                if (mouseWorldPos.x > loc.x && mouseWorldPos.x < loc.x + 1 && mouseWorldPos.y > loc.y && mouseWorldPos.y < loc.y + 1) {

                    // check for whole structure footprint
                    foreach (Structure.GridFootprint footprint in currentFootprint) {
                        if (footprint.isWalkable && !currentBuildLocations.Contains(new Vector2Int(loc.x + footprint.gridX, loc.y + footprint.gridY))) {
                            canFullyBuild = false;
                        }
                    }

                    if (canFullyBuild) {
                        dragVisualsInstance.transform.position = new Vector3(loc.x, loc.y, 0);

                        currentBuildLocation = loc;
                        canBuild = true;

                        break;
                    }
                }
            }
        }


        // drop food on player to eat it
        // or dropstructure on buildLocation to build it
        if (UnifiedInputModule.UpThisFrame) {
            foreach (Shopkeeper shopKeeper in shopKeepers) {
                if (GlobalRaycast.IsTappedInUI(shopKeeper.sellArea)) {
                    draggedOntoShop = true;
                }
            }

            if (draggedOntoShop) {
                handleDragFinishedOnShop();
            } else if (draggedOntoPlayer) {
                handleDragFinishedOnPlayer(playerController);
            } else if (canBuild) {

                if (currentPrefabDef != null) {
                    MapController mapController = WorldConstants.Instance.getMapController();

                    // get position to walk to before building
                    Vector2 playerPos2d = playerController.getPosition2D();
                    Vector2Int playerPosInt = new Vector2Int(Mathf.RoundToInt(playerPos2d.x), Mathf.RoundToInt(playerPos2d.y));

                    // check it for every structure footprint location
                    Structure currentStructureToBuild = currentPrefabDef.Prefab.GetComponent<Structure>();

                    foreach (Structure.GridFootprint structureFootPrint in currentStructureToBuild.gridFootprint) {
                        Vector2Int testWalkablePosInt = currentBuildLocation + new Vector2Int(structureFootPrint.gridX, structureFootPrint.gridY);
                        MapController.Tile walkableTile = mapController.getNearestWalkableTile(testWalkablePosInt, playerPosInt);

                        if (walkableTile != null) {

                            // add walk and build actions
                            Vector2 walktoPosition = new Vector2(walkableTile.gridX, walkableTile.gridY);

                            // walk to action
                            GameAction gameAction = new GameAction(GameAction.ActionType.WALKTO, walktoPosition);
                            playerController.addAction(gameAction);

                            // build action
                            gameAction = new GameAction(GameAction.ActionType.BUILD, currentBuildLocation, () => {
                                handleBuildAction(currentPrefabDef);
                            });

                            gameAction.customData = currentPrefabDef;
                            playerController.addAction(gameAction);

                            break;
                        }
                    }
                }
            }

            clearDragState();
        }
    }

    protected virtual void handleDragFinished() {
        
    }

    protected virtual void handleDragFinishedOnPlayer(PlayerController playerController) {

    }

    protected virtual void handleDragFinishedOnShop() {

    }

    private void clearDragState() {
        if (dragVisualsInstance != null) {
            Destroy(dragVisualsInstance);
        }

        currentDragged = null;
        currentPrefabDef = null;
        currentItemDataDef = null;

        foreach (var locationVis in buildLocationVisualizers) {
            Destroy(locationVis);
        }
        buildLocationVisualizers.Clear();
    }

    protected virtual void handleBuildAction(PrefabDef prefabDef) {

    }

    protected GameObject instantiateSpriteInWorld(Sprite sprite) {
        GameObject go = new GameObject();
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        return go;
    }
}
