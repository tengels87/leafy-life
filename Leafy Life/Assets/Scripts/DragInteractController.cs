using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class DragInteractController : MonoBehaviour
{
    private GameObject currentDragged;
    private GameObject dragVisualsInstance;
    private List<Vector2Int> currentBuildLocations = new List<Vector2Int>();
    private List<GameObject> buildLocationVisualizers = new List<GameObject>();

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        Vector2 mouseWorldPos = MapController.pixelPos2WorldPos(Input.mousePosition);
        
        if (Input.GetMouseButtonDown(0)) {
            if (isTappedInWorld()) {
                handleTappedInWorld();
            }
        }

        if (Input.GetMouseButtonDown(0)) {
            if (isTappedInUI()) {
                handleTappedInUI(null);
            }
        }

        // visualize drag with structure sprite
        if (currentDragged != null) {

            // position visual at mouse position
            dragVisualsInstance.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);

            handleDragging();
        }
    }

    private bool isTappedInWorld() {

        Vector2 mouseWorldPos = MapController.pixelPos2WorldPos(Input.mousePosition);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, new Vector3(0, 0, 1));

        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.gameObject.Equals(this.gameObject)) {
                return true;
            }
        }

        return false;
    }

    private bool isTappedInUI() {
        m_Raycaster = this.gameObject.GetComponent<GraphicRaycaster>();
        m_EventSystem = FindObjectOfType<EventSystem>();

        if (m_Raycaster == null || m_EventSystem == null)
            return false;

        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();

        m_Raycaster.Raycast(m_PointerEventData, results);

        foreach (RaycastResult result in results) {
            if (result.gameObject.Equals(this.gameObject)) {
                return true;
            }
        }

        return false;
    }

    protected virtual void handleTappedInWorld() {

    }

    protected virtual void handleTappedInUI(Object prefabToSpawn) {
        if (prefabToSpawn == null) return;

        currentDragged = this.gameObject;

        // instantiate drag visuals
        dragVisualsInstance = prefabToSpawn.GetType() == typeof(Sprite)
            ? instantiateSpriteInWorld((Sprite)prefabToSpawn)
            : Instantiate((GameObject)prefabToSpawn);

        dragVisualsInstance.name = prefabToSpawn.name;
        dragVisualsInstance.transform.position = new Vector3(0, 0, 0);

        if (prefabToSpawn != null) {
            Structure structure = dragVisualsInstance.GetComponent<Structure>();

            if (structure != null) {

                // get all locations where structure can be placed
                MapController mapController = WorldConstants.Instance.getMapController();
                currentBuildLocations = mapController.getBuildLocations(structure);

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
        Vector2 mouseWorldPos = MapController.pixelPos2WorldPos(Input.mousePosition);

        PlayerController playerController = WorldConstants.Instance.getPlayerController();

        // check if being dropped on player
        bool draggedOntoPlayer = false;
        bool canBuild = false;
        Vector2Int currentBuildLocation = Vector2Int.zero;
        
        if (Vector3.Distance(dragVisualsInstance.transform.position, playerController.transform.position + new Vector3(0.5f, 0.75f, 0)) < 0.8f) {
            draggedOntoPlayer = true;
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
            foreach (Vector2Int loc in currentBuildLocations) {
                if (mouseWorldPos.x > loc.x && mouseWorldPos.x < loc.x + 1 && mouseWorldPos.y > loc.y && mouseWorldPos.y < loc.y + 1) {
                    dragVisualsInstance.transform.position = new Vector3(loc.x, loc.y, 0);

                    currentBuildLocation = loc;
                    canBuild = true;

                    break;
                }
            }
        }


        // drop food on player to eat it
        // or dropstructure on buildLocation to build it
        if (Input.GetMouseButtonUp(0)) {
            clearDragState();

            if (draggedOntoPlayer) {
                canBuild = false;   // ignore building the structure when dragged at player

                handleDragFinishedOnPlayer(playerController);
            }

            if (canBuild) {
                MapController mapController = WorldConstants.Instance.getMapController();

                // get position to walk to before building
                Vector2 playerPos2d = playerController.getPosition2D();
                Vector2Int playerPosInt = new Vector2Int(Mathf.RoundToInt(playerPos2d.x), Mathf.RoundToInt(playerPos2d.y));
                MapController.Tile walkableTile = mapController.getNearestWalkableTile(currentBuildLocation, playerPosInt);
                if (walkableTile != null) {

                    // add walk and build actions
                    Vector2 walktoPosition = new Vector2(walkableTile.gridX, walkableTile.gridY);

                    // walk to action
                    GameAction gameAction = new GameAction(GameAction.ActionType.WALKTO, walktoPosition);
                    playerController.addAction(gameAction);

                    // build action
                    GameObject prefabToBuild = WorldConstants.Instance.getStructureManager().getPrefabByName(dragVisualsInstance.name);
                    if (prefabToBuild != null) {
                        gameAction = new GameAction(GameAction.ActionType.BUILD, currentBuildLocation, () => {
                            handleBuildAction(prefabToBuild);
                        });

                        gameAction.customData = prefabToBuild;
                        playerController.addAction(gameAction);
                    }
                }
            }
        }
    }

    protected virtual void handleDragFinished() {
        
    }

    protected virtual void handleDragFinishedOnPlayer(PlayerController playerController) {

    }

    private void clearDragState() {
        if (dragVisualsInstance != null) {
            Destroy(dragVisualsInstance);
        }

        currentDragged = null;

        foreach (var locationVis in buildLocationVisualizers) {
            Destroy(locationVis);
        }
        buildLocationVisualizers.Clear();
    }

    protected virtual void handleBuildAction(Object customData) {

    }

    private GameObject instantiateSpriteInWorld(Sprite sprite) {
        GameObject go = new GameObject();
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        return go;
    }
}
