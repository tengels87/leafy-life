using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class ItemDataRegistryManager : MonoBehaviour {
    [SerializeField] private ItemDataRegistry registry;

    void Awake() {
        ItemDefs.SetRegistry(registry);
    }
}

public static class ItemDefs {
    private static ItemDataRegistry _registry;

    public static void SetRegistry(ItemDataRegistry reg) => _registry = reg;

    public static bool TryGet(string id, out ItemData itemData) {
        if (_registry == null) {
            itemData = null;
            return false;
        }

        return _registry.TryGet(id, out itemData);
    }

#if UNITY_EDITOR

    [MenuItem("Tools/Content/Rebuild ItemData Registry")]
    public static void RebuildMenu() {
        Rebuild(forceLog: true);
    }

    public static void Rebuild(bool forceLog = false) {
        var guids = AssetDatabase.FindAssets("t:ItemData");
        var defs = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(a => a != null)
            .ToList();

        // Validation
        var missingId = defs.Where(d => string.IsNullOrEmpty(d.Id)).ToList();
        var dupes = defs.GroupBy(d => d.Id).Where(g => !string.IsNullOrEmpty(g.Key) && g.Count() > 1).ToList();

        if (missingId.Count > 0) {
            Debug.LogError($"[ItemDataRegistry] {missingId.Count} ItemData assets have empty IDs:\n" +
                           string.Join("\n", missingId.Select(d => AssetDatabase.GetAssetPath(d))));
        }
        if (dupes.Count > 0) {
            Debug.LogError("[ItemDataRegistry] Duplicate ItemData IDs detected:\n" +
                           string.Join("\n", dupes.Select(g => $"ID {g.Key} → {string.Join(", ", g.Select(d => d.name))}")));
        }

        // Write entries
        if (_registry != null) {
            var so = new SerializedObject(_registry);
            var entriesProp = so.FindProperty("entries");
            entriesProp.ClearArray();

            // Stable, deterministic order (nice for diffs)
            var ordered = defs.OrderBy(d => d.name).ThenBy(d => d.Id).ToList();
            for (int i = 0; i < ordered.Count; i++) {
                entriesProp.InsertArrayElementAtIndex(i);
                var element = entriesProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("id").stringValue = ordered[i].Id;
                element.FindPropertyRelative("data").objectReferenceValue = ordered[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(_registry);
            AssetDatabase.SaveAssets();

            if (forceLog)
                Debug.Log($"[ItemDataRegistry] Rebuilt with {ordered.Count} entries");
        } else {
            Debug.LogError($"[ItemDataRegistry] Rebuilt failed. Static property ItemDefs._registry not set. Enter Play Mode to init it for the first time.");
        }
    }

    // Rebuild when ItemDefs are imported/removed/moved
    private class Watcher : AssetPostprocessor {
        private void OnPostprocessAllAssets(
            string[] imported, string[] deleted, string[] movedTo, string[] movedFrom) {
            bool touched = imported.Concat(deleted).Concat(movedTo).Concat(movedFrom)
                .Any(path => path.EndsWith(".asset"));
            if (!touched) return;

            // If any ItemData changed, rebuild
            bool itemDataChanged = imported.Concat(movedTo).Any(IsItemDataPath)
                                    || deleted.Concat(movedFrom).Any(IsItemDataPath);
            if (itemDataChanged) {
                Rebuild();
            }
        }

        private static bool IsItemDataPath(string path) {
            if (string.IsNullOrEmpty(path)) return false;
            var obj = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            return obj != null;
        }
    }

    // Ensure up-to-date before build
    private class Preprocess : IPreprocessBuildWithReport {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report) {
            Rebuild(forceLog: true);
        }
    }

#endif

}


