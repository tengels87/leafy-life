using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuildController : DragInteractController
{
    public static UnityAction<ItemData> OnCollectedEvent;

    public Object buildPrefab;
    public ItemData buildIngredients;
    public bool ignoreIngredients;

    private bool isEnabled = false;

    private void Awake() {
        setEnabled(canBuild());
    }

    private void OnEnable() {
        Inventory.ItemAddedEvent += OnItemAdded;
        Inventory.ItemRemovedEvent += OnItemRemoved;
    }

    private void OnDisable() {
        Inventory.ItemAddedEvent += OnItemAdded;
        Inventory.ItemRemovedEvent += OnItemRemoved;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void handleTappedInWorld() {
        base.handleTappedInWorld();
    }

    protected override void handleTappedInUI(Object prefabToSpawn) {
        if (canBuild()) {
            Object dragVisual = buildPrefab;

            base.handleTappedInUI(dragVisual);
        }
    }

    protected override void handleBuildAction(Object customData) {
        Inventory inventory = WorldConstants.Instance.getInventory();
        
        if (inventory != null) {
            if (ignoreIngredients == false) {
                if (buildIngredients != null) {
                    inventory.removeItem(buildIngredients);
                }
            }
        }
    }

    private void OnItemAdded(ItemData item, bool isFirstOfThisKind) {
        setEnabled(canBuild());
    }

    private void OnItemRemoved(ItemData item, bool isFirstOfThisKind) {
        setEnabled(canBuild());
    }

    private void setEnabled(bool val) {
        isEnabled = val;

        // set disabled state via material shader
        Image icon = this.gameObject.GetComponent<Image>();
        if (icon != null) {
            icon.material.SetFloat("_EffectAmount", val ? 0f : 1f);
        }
    }

    private bool canBuild() {
        if (ignoreIngredients) return true;

        Inventory inventory = WorldConstants.Instance.getInventory();
        if (inventory != null) {
            if (inventory.containsItems(buildIngredients, 1)) {
                return true;
            }
        }

        return false;
    }
}
