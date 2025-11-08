using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Data/new Item")]
public class ItemData : ScriptableObject
{
    public enum ItemType {
        WOOD,
        FOOD,
        DECORATION,
        COIN,
        SEED
    }

    [SerializeField] private string id; // stable baked GUID
    public string Id => id;

    [SerializeField] private int version = 1;
    public int Version => version;

    public string itemName;
    public ItemType itemType;
    public int sellValue;
    public Sprite iconSprite;
    public PrefabDef spawnPrefabDef;

#if UNITY_EDITOR

    private void OnValidate() {
        if (string.IsNullOrEmpty(id)) {
            id = System.Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

#endif

    public override bool Equals(object obj) {
        return obj is ItemData item &&
               itemName == item.itemName &&
               itemType == item.itemType;
    }

    public override int GetHashCode() {
        return HashCode.Combine(itemName, itemType);
    }
}
