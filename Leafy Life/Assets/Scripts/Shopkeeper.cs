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
            Vector2 mouseWorldPos = MapController.pixelPos2WorldPos(Input.mousePosition);

            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, new Vector3(0, 0, 1));
            bool hasHit = false;
            foreach (RaycastHit2D hit in hits) {
                if (hit.collider.gameObject.Equals(this.gameObject)) {
                    hasHit = true;
                    break;
                }
            }
            if (hasHit) {
                setUIenabled(true);
            }
        }
    }

    public void setUIenabled(bool val) {
        shopCanvasUI.SetActive(val);
    }

    public void showConfirmDialog(ItemData item, float itemPrice) {
        confirmDialogUI.SetActive(true);

        lastOffer = new ShopItem();
        lastOffer.data = ItemData.Create(item.itemName, item.itemType);
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

    public void purchaseLastOffer() {
        print(lastOffer.data.itemName + " : " + lastOffer.itemPrice);

        ItemController.OnCollectedEvent?.Invoke(lastOffer.data);
    }
}