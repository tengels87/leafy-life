using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static UnityAction<InventoryItem, bool> ItemAddedEvent;
    public static UnityAction<InventoryItem, bool> ItemRemovedEvent;

    private Dictionary<InventoryItem, int> items = new Dictionary<InventoryItem, int>();

    private bool isInitialized = false;

    void OnEnable() {
        ItemController.OnCollectedEvent += addItem;
    }

    void OnDisable() {
        ItemController.OnCollectedEvent -= addItem;
    }

    void Start()
    {
        // get loaded save data
        if (!isInitialized) {
            SaveSystem.GameData saveData = WorldConstants.Instance.getSaveSystem().getLoadedData();
            if (saveData != null) {
                foreach (InventoryItem item in saveData.inventoryItemsList) {
                    addItem(item);
                }
            }

            isInitialized = true;
        }
    }

    void Update()
    {
        
    }

    public Dictionary<InventoryItem, int> getInventoryItems() {
        return items;
    }

    public List<InventoryItem> getInventoryItemsAsList() {
        List<InventoryItem> itemsList = new List<InventoryItem>();

        foreach (InventoryItem key in items.Keys) {
            for (int i = 0; i < items[key]; i++) {
                itemsList.Add(new InventoryItem(key.name, key.itemType));
            }
        }

        return itemsList;
    }

    public bool containsItems(InventoryItem item, int count) {
        if (items.ContainsKey(item)) {
            return (items[item] >= count);
        } else {
            return false;
        }
    }

    public void addItem(InventoryItem item) {
        if (items.ContainsKey(item)) {
            items[item]++;
            ItemAddedEvent?.Invoke(item, false);
        } else {
            items[item] = 1;
            ItemAddedEvent?.Invoke(item, true);
        }
    }

    public bool removeItem(InventoryItem item) {
        if (items.ContainsKey(item)) {
            items[item]--;
            if (items[item] <= 0) {
                items.Remove(item);
                ItemRemovedEvent?.Invoke(item, true);
            } else {
                ItemRemovedEvent?.Invoke(item, false);
            }

            return true;
        } else {
            return false;
        }
    }

    [System.Serializable]
    public class InventoryItem {
        public enum ItemType {
            WOOD,
            FOOD,
            DECORATION,
            COIN
        }

        public string name;
        public ItemType itemType;
        public Sprite iconSprite;
        public GameObject structurePrefab;

        public InventoryItem() {}

        public InventoryItem(string name, ItemType itemType) {
            this.name = name;
            this.itemType = itemType;
        }

        public override bool Equals(object obj) {
            return obj is InventoryItem item &&
                   name == item.name &&
                   itemType == item.itemType;
        }

        public override int GetHashCode() {
            return HashCode.Combine(name, itemType);
        }
    }
}
