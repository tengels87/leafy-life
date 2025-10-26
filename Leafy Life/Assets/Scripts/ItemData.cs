using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Data/new Item")]
public class ItemData : ScriptableObject
{
    public enum ItemType {
        WOOD,
        FOOD,
        DECORATION,
        COIN
    }

    public string itemName;
    public ItemType itemType;
    public Sprite iconSprite;
    public GameObject structurePrefab;

    public static ItemData Create(string itemName, ItemType itemType) {
        ItemData item = CreateInstance<ItemData>();
        item.itemName = itemName;
        item.itemType = itemType;

        return item;
    }

    public override bool Equals(object obj) {
        return obj is ItemData item &&
               itemName == item.itemName &&
               itemType == item.itemType;
    }

    public override int GetHashCode() {
        return HashCode.Combine(itemName, itemType);
    }
}
