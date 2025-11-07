using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Prefab Definition", fileName = "New PrefabDef")]
public class PrefabDef : ScriptableObject {
    [SerializeField] private string id; // stable baked GUID
    public string Id => id;

    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;

    [SerializeField] private int version = 1;
    public int Version => version;

#if UNITY_EDITOR

    private void OnValidate() {
        if (string.IsNullOrEmpty(id)) {
            id = System.Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

#endif

    public override bool Equals(object obj) {
        return obj is PrefabDef st &&
               id == st.id;
    }

    public override int GetHashCode() {
        return HashCode.Combine("", id);
    }
}