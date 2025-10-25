using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryController : MonoBehaviour {
    [SerializeField]
    private GameObject slotsContainer;
    [SerializeField]
    private GameObject itemIconTemplate;

    void OnEnable() {
        Inventory.ItemAddedEvent += updateSlots;
        Inventory.ItemRemovedEvent += updateSlots;
    }

    void OnDisable() {
        Inventory.ItemAddedEvent -= updateSlots;
        Inventory.ItemRemovedEvent -= updateSlots;
    }

    void Start() {

    }

    void Update() {

    }

    private void updateSlots(Inventory.InventoryItem arg0, bool arg1) {

        // destroy all slots
        Transform[] slotTransforms = slotsContainer.GetComponentsInChildren<Transform>();
        foreach (Transform slotTransform in slotTransforms) {
            if (slotTransform.gameObject != slotsContainer) {
                Destroy(slotTransform.gameObject);
            }
        }

        // recreate all items in slots
        Inventory inventory = WorldConstants.Instance.getInventory();
        if (inventory != null) {
            Dictionary<Inventory.InventoryItem, int> itemsDict = inventory.getInventoryItems();
            foreach (Inventory.InventoryItem item in itemsDict.Keys) {
                GameObject go = Instantiate(itemIconTemplate);
                go.transform.SetParent(slotsContainer.transform);
                go.transform.localScale = Vector3.one;

                // add a copy of ItemController
                ItemController blankItemController = go.AddComponent<ItemController>();
                var json = JsonUtility.ToJson(item);
                JsonUtility.FromJsonOverwrite(json, blankItemController.itemData);

                // fill template
                Image iconComponent = go.GetComponent<Image>();
                if (iconComponent != null) {
                    iconComponent.sprite = item.iconSprite;
                }

                TextMeshProUGUI textMesh = go.GetComponentInChildren<TextMeshProUGUI>();
                if (textMesh != null) {
                    textMesh.text = "" + itemsDict[item];
                }
            }
        }
    }
}
