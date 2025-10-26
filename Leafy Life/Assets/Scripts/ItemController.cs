using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemController : DragInteractController
{
    public static UnityAction<ItemData> OnCollectedEvent;

    public ItemData itemData;
    public int itemCount;

    private bool isCollected = false;

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
        collect();
    }

    protected override void handleTappedInUI(Object prefabToSpawn) {
        Object dragVisual = itemData.structurePrefab != null ? itemData.structurePrefab : itemData.iconSprite;
        //Object dragBuildPrefab = itemData.structurePrefab != null ? itemData.structurePrefab : null;

        base.handleTappedInUI(dragVisual);
    }

    protected override void handleDragFinished() {
        base.handleDragFinished();
    }

    protected override void handleDragFinishedOnPlayer(PlayerController playerController) {
        base.handleDragFinishedOnPlayer(playerController);
        print("42 drag finished on player");

        // feed player
        Inventory inventory = WorldConstants.Instance.getInventory();
        if (inventory != null) {

            ItemData itemToConsume = itemData;
            if (itemToConsume != null) {
                if (itemToConsume.itemType == ItemData.ItemType.FOOD) {
                    playerController.GetComponent<StatsController>().changeNutritionValue(30);
                    inventory.removeItem(itemToConsume);
                }
            }
        }
    }

    protected override void handleBuildAction(Object customData) {
        Inventory inventory = WorldConstants.Instance.getInventory();

        if (inventory != null) {
            inventory.removeItem(itemData);
        }
    }

    public void collect() {
        if (!isCollected) {
            isCollected = true;

            // unlink, because parent will probably be deleted
            this.transform.SetParent(null);

            // tween
            FindObjectOfType<ResourceCollector>()
                .Collect(itemData.iconSprite, itemCount, this.transform.position);

            OnCollectedEvent?.Invoke(this.itemData);
            Object.Destroy(this.gameObject);
        }
    }
}
