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

    void OnEnable() {
        Collectable.OnCollectedEvent += addItem;
    }

    void OnDisable() {
        Collectable.OnCollectedEvent -= addItem;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
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

    [Serializable]
    public class InventoryItem {
        public enum ItemType {
            WOOD,
            FOOD
        }

        public string name;
        public ItemType itemType;

        public InventoryItem(string name, ItemType itemType) {
            this.name = name;
            this.itemType = itemType;
        }

        public override bool Equals(object obj) {
            return obj is InventoryItem item &&
                   //name == item.name &&
                   itemType == item.itemType;
        }

        public override int GetHashCode() {
            //return HashCode.Combine(name, itemType);
            return HashCode.Combine("", itemType);
        }
    }
}
