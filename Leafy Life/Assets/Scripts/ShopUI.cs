using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShopUI : MonoBehaviour {
    public GameObject shopItemPrefab;
    public Transform itemListParent;
    public List<ShopItem> shopItems = new List<ShopItem>();
    public Shopkeeper shopkeeperScript;

    private List<ShopItemController> shopItemControllerList = new List<ShopItemController>();

    private void OnEnable() {
        Inventory.GoldChangedEvent += OnGoldChanged;
    }

    private void OnDisable() {
        Inventory.GoldChangedEvent -= OnGoldChanged;
    }

    private void Start() {
        PopulateShopItems();
    }

    public void PopulateShopItems() {

        // clear existing items in the shop
        foreach (Transform child in itemListParent) {
            Destroy(child.gameObject);
        }

        shopItemControllerList.Clear();

        // create item buttons dynamically
        foreach (var item in shopItems) {
            GameObject itemButton = Instantiate(shopItemPrefab, itemListParent);
            ShopItemController shopItemController = itemButton.GetComponent<ShopItemController>();
            shopItemController.imageItemIcon.sprite = item.data.iconSprite;
            shopItemController.textItemName.text = item.data.itemName;
            shopItemController.textItemCosts.text = "" + item.itemPrice;

            // add listener to the button to show confirm dialog
            shopItemController.setOnClickedListener(() => shopkeeperScript.showConfirmDialog(item.data, item.itemPrice));

            shopItemControllerList.Add(shopItemController);
        }
    }

    private void OnGoldChanged(int goldAmount) {
        foreach (ShopItemController controller in shopItemControllerList) {
            if (int.TryParse(controller.textItemCosts.text, out int result)) {
                if (result <= goldAmount) {
                    controller.setEnabled(true);
                } else {
                    controller.setEnabled(false);
                }
            }
        }
    }
}

[System.Serializable]
public class ShopItem {
    public ItemData data;
    public int itemPrice;
}