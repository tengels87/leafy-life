using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using DG.Tweening;

public class ItemController : DragInteractController
{
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
        tryCollect();
    }

    protected override void handleTappedInUI(Object obj) {
        Object dragVisual = itemData.spawnPrefabDef != null ? itemData.spawnPrefabDef.Prefab : itemData.iconSprite;

        currentPrefabDef = itemData.spawnPrefabDef;

        base.handleTappedInUI(dragVisual);
    }

    protected override void handleDragFinished() {
        base.handleDragFinished();
    }

    protected override void handleDragFinishedOnPlayer(PlayerController playerController) {
        base.handleDragFinishedOnPlayer(playerController);

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

    protected override void handleDragFinishedOnShop() {
        Inventory inventory = WorldConstants.Instance.getInventory();

        if (inventory != null) {
            inventory.removeItem(itemData);
            inventory.depositGold(itemData.sellValue);

            Vector2 pointerPos = UnifiedInputModule.Instance.PointerPosition;
            Vector2 effectSpawnPos = MapController.pixelPos2WorldPos(pointerPos);
            FindObjectOfType<ResourceCollector>()
                .Collect(ResourceCollector.Preset.COIN, null, itemData.sellValue, effectSpawnPos);
        }
    }

    protected override void handleBuildAction(PrefabDef prefabDef) {
        Inventory inventory = WorldConstants.Instance.getInventory();

        if (inventory != null) {
            inventory.removeItem(itemData);
        }
    }

    public bool tryCollect() {
        if (!isCollected) {
            isCollected = true;

            Inventory inventory = WorldConstants.Instance.getInventory();

            if (inventory != null) {
                if (inventory.tryAddItem(itemData)) {

                    // unlink, because parent will probably be deleted
                    this.transform.SetParent(null);

                    // tween
                    FindObjectOfType<ResourceCollector>()
                        .Collect(ResourceCollector.Preset.CUSTOM, itemData.iconSprite, itemCount, this.transform.position);

                    Object.Destroy(this.gameObject);

                    return true;
                } else {

                    // do not collect, inentory full
                    Vector3 targetPos = this.gameObject.transform.position;
                    this.gameObject.transform.DOJump(targetPos, 1, 1, 1.0f).OnComplete(() => {
                        isCollected = false;
                    });

                    return false;
                }
            }
        }

        return false;
    }
}
