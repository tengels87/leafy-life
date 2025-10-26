using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static UnityAction<ItemData, bool> ItemAddedEvent;
    public static UnityAction<ItemData, bool> ItemRemovedEvent;

    private Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();

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
                foreach (ItemData item in saveData.inventoryItemsList) {
                    addItem(item);
                }
            }

            isInitialized = true;
        }
    }

    void Update()
    {
        
    }

    public Dictionary<ItemData, int> getInventoryItems() {
        return items;
    }

    public List<ItemData> getInventoryItemsAsList() {
        List<ItemData> itemsList = new List<ItemData>();

        foreach (ItemData key in items.Keys) {
            for (int i = 0; i < items[key]; i++) {
                itemsList.Add(ItemData.Create(key.name, key.itemType));
            }
        }

        return itemsList;
    }

    public bool containsItems(ItemData item, int count) {
        if (items.ContainsKey(item)) {
            return (items[item] >= count);
        } else {
            return false;
        }
    }

    public void addItem(ItemData item) {
        if (items.ContainsKey(item)) {
            items[item]++;
            ItemAddedEvent?.Invoke(item, false);
        } else {
            items[item] = 1;
            ItemAddedEvent?.Invoke(item, true);
        }
    }

    public bool removeItem(ItemData item) {
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
}
