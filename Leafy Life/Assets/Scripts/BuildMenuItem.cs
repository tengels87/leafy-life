using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuItem : MonoBehaviour {
    public GameObject prefab;
    public GameObject prefab_optionalDragVisuals;
    public MapController.MapType availableInMapType;
    public GameObject prefab_comsumesItem;
    public bool infiniteUse = false;
    public GameObject canvasGameobject;

    private Structure buildableStructure;
    private Structure currentDragged;
    private GameObject dragVisualsInstance;
    private List<Vector2Int> currentBuildLocations;
    private List<GameObject> buildLocationVisualizers = new List<GameObject>();

    private bool isEnabled;
    private bool isVisible;
    private int amount = 0;

    void Awake() {
        Inventory.ItemAddedEvent += OnItemAdded;
        Inventory.ItemRemovedEvent += OnItemRemoved;

        if (prefab != null) {
            buildableStructure = prefab.GetComponent<Structure>();
        }

        if (amount > 0 || infiniteUse == true) {
            setEnabled(true);
        } else {
            setEnabled(false);
        }

        setVisible(isVisible);
        updateLabel();
    }

    void OnDestroy() {
        Inventory.ItemAddedEvent -= OnItemAdded;
        Inventory.ItemRemovedEvent -= OnItemRemoved;
    }

    void Start() {
        
    }

    void Update() {

        Vector2 mouseWorldPos = MapController.pixelPos2WorldPos(Input.mousePosition);

        if (Input.GetMouseButtonDown(0)) {
            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, new Vector3(0, 0, 1));
            bool hasHit = false;
            foreach (RaycastHit2D hit in hits) {
                if (isVisible && hit.collider.gameObject.Equals(this.gameObject)) {
                    hasHit = true;
                    break;
                }
            }
            if (hasHit) {
                if (prefab != null && (amount > 0 || infiniteUse == true)) {
                    Structure structure = prefab.GetComponent<Structure>();

                    if (structure != null) {
                        currentDragged = structure;

                        // instantiate drag visuals
                        MapController mapController = WorldConstants.Instance.getMapController();

                        dragVisualsInstance = (GameObject)Object.Instantiate(prefab_optionalDragVisuals != null ? prefab_optionalDragVisuals : prefab);
                        dragVisualsInstance.transform.position = new Vector3(0, 0, 0);

                        // get all locations where structure can be placed
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
        }

        // visualize drag with structure sprite
        if (currentDragged != null) {

            // position visual at mouse position
            dragVisualsInstance.transform.position = new Vector3(mouseWorldPos.x - 0.5f, mouseWorldPos.y - 0.5f, 0);

            // check if being dropped on player
            bool draggedOntoPlayer = false;
            bool canBuild = false;
            Vector2Int currentBuildLocation = Vector2Int.zero;

            PlayerController playerController = WorldConstants.Instance.getPlayerController();
            if (Vector3.Distance(dragVisualsInstance.transform.position, playerController.transform.position) < 0.5f) {
                draggedOntoPlayer = true;
            }

            if (draggedOntoPlayer) {

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

                // destroy location visualizers
                foreach (GameObject locationVis in buildLocationVisualizers) {
                    Destroy(locationVis);
                }
                buildLocationVisualizers.Clear();

                currentDragged = null;
                if (dragVisualsInstance != null) {
                    Destroy(dragVisualsInstance);
                }


                if (draggedOntoPlayer) {
                    canBuild = false;   // ignore building the structure when dragged at player

                    // feed player
                    Inventory inventory = WorldConstants.Instance.getInventory();
                    if (inventory != null) {
                        if (infiniteUse == false) {
                            if (prefab_comsumesItem != null) {
                                Inventory.InventoryItem itemToConsume = prefab_comsumesItem.GetComponent<Collectable>()?.itemData;
                                if (itemToConsume != null) {
                                    if (itemToConsume.itemType == Inventory.InventoryItem.ItemType.FOOD) {
                                        playerController.GetComponent<StatsController>().changeNutritionValue(30);
                                        inventory.removeItem(itemToConsume);
                                    }
                                }
                            }
                        }
                    }
                }

                if (canBuild) {
                    MapController mapController = WorldConstants.Instance.getMapController();

                    // position to walk to before building
                    MapController.Tile walkableTile = mapController.getNearestWalkableTile(currentBuildLocation);
                    if (walkableTile != null) {

                        // add walk and build actions
                        Vector2 walktoPosition = new Vector2(walkableTile.gridX, walkableTile.gridY);

                        GameAction gameAction = new GameAction(GameAction.ActionType.WALKTO, walktoPosition);
                        playerController.addAction(gameAction);

                        gameAction = new GameAction(GameAction.ActionType.BUILD, currentBuildLocation, () => {
                            Inventory inventory = WorldConstants.Instance.getInventory();

                            if (inventory != null) {
                                if (infiniteUse == false) {
                                    if (prefab_comsumesItem != null) {
                                        Inventory.InventoryItem itemToConsume = prefab_comsumesItem.GetComponent<Collectable>()?.itemData;
                                        if (itemToConsume != null) {
                                            inventory.removeItem(itemToConsume);
                                        }
                                    }
                                }
                            }
                        });
                        gameAction.customData = prefab;
                        playerController.addAction(gameAction);
                    }

                    // collapse sub menues
                    ToggleGameObject[] subMenues = WorldConstants.Instance.getHudManager().GetComponentsInChildren<ToggleGameObject>();
                    foreach (ToggleGameObject menu in subMenues) {
                        menu.hide();
                    }
                }
            }
        }
    }

    public void setVisible(bool val) {
        isVisible = val;

        if (canvasGameobject != null) {
            canvasGameobject.SetActive(val);
        }

        updateLabel();
    }

    public void setEnabled(bool val) {
        isEnabled = val;

        Image buildIcon = canvasGameobject.GetComponentInChildren<Image>();
        if (buildIcon != null) {
            buildIcon.material.SetFloat("_EffectAmount", val ? 0f : 1f);
        }

        updateLabel();
    }

    public void updateLabel() {
        if (canvasGameobject != null) {
            Image backgroundLabel = canvasGameobject.GetComponentsInChildren<Image>()[1];
            Text textLabel = canvasGameobject.GetComponentInChildren<Text>();

            if (backgroundLabel != null) {
                backgroundLabel.enabled = (amount > 0);
            }

            if (textLabel != null) {
                textLabel.enabled = (amount > 0);
                textLabel.text = "" + amount;
            }
            
        }
    }

    private void OnItemAdded(Inventory.InventoryItem item, bool isFirstOfThisKind) {
        if (buildableStructure != null && prefab_comsumesItem != null) {
            if (item.itemType == prefab_comsumesItem.GetComponent<Collectable>().itemData.itemType) {
                amount++;

                updateLabel();

                if (isFirstOfThisKind) {
                    setEnabled(true);
                }
            }
        }
    }

    private void OnItemRemoved(Inventory.InventoryItem item, bool isLastOfThisKind) {
        if (buildableStructure != null && prefab_comsumesItem != null) {
            if (item.itemType == prefab_comsumesItem.GetComponent<Collectable>().itemData.itemType) {
                if (amount > 0) {
                    amount--;

                    updateLabel();
                }

                if (isLastOfThisKind && infiniteUse == false) {
                    setEnabled(false);
                }
            }
        }
    }
}
