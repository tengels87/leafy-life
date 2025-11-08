using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shopkeeper : MonoBehaviour {
    public GameObject shopCanvasUI;
    public GameObject confirmDialogUI;

    private ShopItem lastOffer;

    void Awake() {
        setUIenabled(false);
        hideConfirmDialog();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            if (GlobalRaycast.IsTappedInWorld(this.gameObject)) {
                setUIenabled(true);
            }
        }
    }

    public void setUIenabled(bool val) {
        shopCanvasUI.SetActive(val);

        // open player inventory when opening shop
        // close inventory when leaving shop
        InventoryController inventoryController = WorldConstants.Instance.getInventory()?.GetComponent<InventoryController>();
        if (inventoryController != null) {
            inventoryController.setOpen(val);
        }
    }

    public void showConfirmDialog(ItemData item, int itemPrice) {
        confirmDialogUI.SetActive(true);

        lastOffer = new ShopItem();
        lastOffer.data = item;
        lastOffer.data = item;
        lastOffer.itemPrice = itemPrice;

        ShopConfirmDialog confirmDialog = confirmDialogUI.GetComponentInChildren<ShopConfirmDialog>();
        if (confirmDialog != null) {
            confirmDialog.ImageIcon.sprite = item.iconSprite;
            confirmDialog.TextLabel.text = "" + item.itemName;
        }
    }

    public void hideConfirmDialog() {
        confirmDialogUI.SetActive(false);
    }

    public void tryPurchaseLastOffer() {

        // check for sufficient gold and buy, add item to inventory
        Inventory inventory_player = WorldConstants.Instance.getInventory();
        if (inventory_player != null) {
            if (inventory_player.getGoldAmount() >= lastOffer.itemPrice) {
                inventory_player.withdrawGold(lastOffer.itemPrice);
                ItemController.OnCollectedEvent?.Invoke(lastOffer.data);
            }
        }
    }
}