using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Generated/ItemData Registry", fileName = "ItemDataRegistry")]
public class ItemDataRegistry : ScriptableObject {
    [System.Serializable]
    public struct Entry {
        public string id;
        public ItemData data;
    }

    [SerializeField] private List<Entry> entries = new();
    public IReadOnlyList<Entry> Entries => entries;

    private Dictionary<string, ItemData> _byId;

    private void OnEnable() {
        BuildIndex();
    }

    public void BuildIndex() {
        _byId = new Dictionary<string, ItemData>(entries.Count);

        foreach (var e in entries) {
            if (!string.IsNullOrEmpty(e.id) && e.data != null && !_byId.ContainsKey(e.id)) {
                _byId.Add(e.id, e.data);
            }
        }
    }

    public bool TryGet(string id, out ItemData itemData) {
        if (_byId == null) BuildIndex();

        return _byId.TryGetValue(id, out itemData);
    }
}