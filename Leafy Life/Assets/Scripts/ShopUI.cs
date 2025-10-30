using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour {
    public GameObject shopItemPrefab;
    public Transform itemListParent;
    public List<ShopItem> shopItems = new List<ShopItem>();
    public Shopkeeper shopkeeperScript;

    private void Start() {
        PopulateShopItems();
    }

    public void PopulateShopItems() {
        // Clear existing items in the shop
        foreach (Transform child in itemListParent) {
            Destroy(child.gameObject);
        }

        // Create item buttons dynamically
        foreach (var item in shopItems) {
            GameObject itemButton = Instantiate(shopItemPrefab, itemListParent);
            ShopItemController shopItemController = itemButton.GetComponent<ShopItemController>();
            shopItemController.imageItemIcon.sprite = item.data.iconSprite;
            shopItemController.textItemName.text = item.data.itemName;
            shopItemController.textItemCosts.text = "" + item.itemPrice;

            // Add listener to the button to show confirm dialog
            shopItemController.setOnSelectedListener(() => shopkeeperScript.showConfirmDialog(item.data, item.itemPrice));
        }
    }
}

[System.Serializable]
public class ShopItem {
    public ItemData data;
    public float itemPrice;
}