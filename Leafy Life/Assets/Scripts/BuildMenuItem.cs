using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuItem : MonoBehaviour {
    public GameObject prefab;
    public MapController.MapType availableInMapType;
    public bool infiniteUse = false;
    public Canvas canvas_amountIndicator;

    private Structure buildableStructure;
    private Structure currentDragged;
    private GameObject dragVisualizer;
    private List<Vector2Int> currentBuildLocations;
    private List<GameObject> buildLocationVisualizers = new List<GameObject>();

    private bool isEnabled;
    private int amount = 0;

    void OnEnable() {
        Inventory.ItemAddedEvent += OnItemAdded;
        Inventory.ItemRemovedEvent += OnItemRemoved;
    }

    void OnDisable() {
        Inventory.ItemAddedEvent -= OnItemAdded;
        Inventory.ItemRemovedEvent -= OnItemRemoved;
    }

    void Start() {
        if (prefab != null) {
            buildableStructure = prefab.GetComponent<Structure>();
        }

        if (amount > 0 || infiniteUse == true) {
            setEnabled(true);
        } else {
            setEnabled(false);
        }

        updateLabel();
    }

    void Update() {

        Vector2 mouseWorldPos = MapController.pixelPos2WorldPos(Input.mousePosition);

        if (Input.GetMouseButtonDown(0)) {
            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, new Vector3(0, 0, 1));
            bool hasHit = false;
            foreach (RaycastHit2D hit in hits) {
                if (hit.collider.gameObject.Equals(this.gameObject)) {
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

                        dragVisualizer = (GameObject)Object.Instantiate(prefab);
                        dragVisualizer.transform.position = new Vector3(0, 0, 0);

                        // determine lofbuildtilcation where structure can be placed
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
            dragVisualizer.transform.position = new Vector3(mouseWorldPos.x - 0.5f, mouseWorldPos.y - 0.5f, 0);

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

                        gameAction = new GameAction(GameAction.ActionType.BUILD, currentBuildLocation, () => {
                            Inventory inventory = WorldConstants.Instance.getInventory();
                            Crop cropScript = prefab.GetComponentInChildren<Crop>();

                            if (inventory != null && cropScript != null) {
                                Collectable collectableScript = cropScript.prefab_itemToSpawn.GetComponent<Collectable>();
                                if (collectableScript != null) {
                                    inventory.removeItem(collectableScript.itemData);
                                }
                            }
                        });
                        gameAction.customData = prefab;
                        playerController.addAction(gameAction);
                    }

                    // collapse sub menues
                    MenuCollapse[] subMenues = WorldConstants.Instance.getHudManager().GetComponentsInChildren<MenuCollapse>();
                    foreach (MenuCollapse menu in subMenues) {
                        menu.hide();
                    }
                }
            }
        }
    }

    public void setEnabled(bool val) {
        isEnabled = val;

        SpriteRenderer renderer = this.GetComponent<SpriteRenderer>();
        if (renderer != null) {
            renderer.material.SetFloat("_EffectAmount", val ? 0f : 1f);
        }

        if (canvas_amountIndicator != null) {
            canvas_amountIndicator.enabled = val;

            Text textLabel = canvas_amountIndicator.GetComponentInChildren<Text>();
            if (textLabel != null && amount > 0) {
                textLabel.text = "" + amount;
            }
        }
    }

    public void updateLabel() {
        if (canvas_amountIndicator != null) {
            Text textLabel = canvas_amountIndicator.GetComponentInChildren<Text>();
            if (textLabel != null && amount > 0) {
                textLabel.text = "" + amount;
            }
        }
    }

    private void OnItemAdded(Inventory.InventoryItem item, bool isFirstOfThisKind) {
        if (buildableStructure != null) {
            if (item.itemType == Inventory.InventoryItem.ItemType.FOOD
                && buildableStructure.structureType == Structure.StructureType.CROP) {

                amount++;

                updateLabel();

                if (isFirstOfThisKind) {
                    setEnabled(true);
                }
            }
        }
    }

    private void OnItemRemoved(Inventory.InventoryItem item, bool isLastOfThisKind) {
        if (buildableStructure != null) {
            if (buildableStructure.structureType == Structure.StructureType.CROP) {

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
