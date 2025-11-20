using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static UnityAction<ItemData, bool> ItemAddedEvent;
    public static UnityAction<ItemData, bool> ItemRemovedEvent;
    public static UnityAction<int> GoldChangedEvent;

    public int maxCapacity = 9;

    private Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
    private int goldCount = 0;

    private bool isInitialized = false;

    void Start()
    {
        // get loaded save data
        if (!isInitialized) {
            SaveSystem.GameData saveData = WorldConstants.Instance.getSaveSystem().getLoadedData();
            if (saveData != null) {

                // restore saved inventory items
                foreach (string itemId in saveData.inventoryItemsUidList) {
                    if (ItemDefs.TryGet(itemId, out ItemData _itemData)) {
                        addItem(_itemData);
                    }
                }

                // restore saved gold
                depositGold(saveData.goldAmount);
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
                itemsList.Add(key);
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

    public bool tryAddItem(ItemData item) {
        if (items.Count < maxCapacity || items.ContainsKey(item)) {
            addItem(item);

            return true;
        } else {
            return false;
        }
    }

    private void addItem(ItemData item) {
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

    public void depositGold(int amount) {
        goldCount = goldCount + amount;

        GoldChangedEvent?.Invoke(goldCount);
    }

    public void withdrawGold(int amount) {
        goldCount = goldCount - amount;

        goldCount = Math.Clamp(goldCount, 0, 999);

        GoldChangedEvent?.Invoke(goldCount);
    }

    public int getGoldAmount() {
        return goldCount;
    }
}
