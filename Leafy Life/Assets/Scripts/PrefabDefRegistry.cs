using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Generated/PrefabDef Registry", fileName = "PrefabDefRegistry")]
public class PrefabDefRegistry : ScriptableObject {
    [System.Serializable]
    public struct Entry {
        public string id;
        public PrefabDef def;
    }

    [SerializeField] private List<Entry> entries = new();
    public IReadOnlyList<Entry> Entries => entries;

    private Dictionary<string, PrefabDef> _byId;

    private void OnEnable() {
        BuildIndex();
    }

    public void BuildIndex() {
        _byId = new Dictionary<string, PrefabDef>(entries.Count);

        foreach (var e in entries) {
            if (!string.IsNullOrEmpty(e.id) && e.def != null && !_byId.ContainsKey(e.id)) {
                _byId.Add(e.id, e.def);
            }
        }
    }

    public bool TryGet(string id, out PrefabDef def) {
        if (_byId == null) BuildIndex();

        return _byId.TryGetValue(id, out def);
    }
}