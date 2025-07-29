using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureManager : MonoBehaviour
{
    public List<PrefabDatabaseItem> prefabs = new List<PrefabDatabaseItem>();
    // tiles
    public GameObject prefab_platform;
    public GameObject prefab_ladder;
    public GameObject prefab_ground_invisible;

    public GameObject prefab_grass;
    public GameObject prefab_pinetree;

    // collectables
    public GameObject prefab_carrot;
    public GameObject prefab_log;

    // decoration
    public GameObject prefab_decal_grass1;
    public GameObject prefab_decal_grass2;

    public GameObject prefab_maplink_treehouse;
    public GameObject prefab_maplink_garden;

    void Start()
    {
        
    }

    void Update()
    {
        // Test
        /*
        if (Input.GetMouseButtonDown(0)) {
            MapController mapController = WorldConstants.Instance.getMapController();

            Vector2 pp = MapController.pixelPos2WorldPos(Input.mousePosition);

            if (mapController.isEmpty(new Vector2Int((int)pp.x, (int)pp.y))) {

                // instantiate
                mapController.buildTile((int)pp.x, (int)pp.y, prefab_platform);
            }
        }
        */
    }

    public GameObject getPrefabByName(string prefabName) {
        PrefabDatabaseItem item = prefabs.Find(item => item.prefab.name == prefabName);

        if (item != null) {
            return item.prefab;
        } else {
            return null;
        }
    }

    public GameObject getPrefabByUID(int uid) {
        PrefabDatabaseItem item = prefabs.Find(item => item.uid == uid);

        if (item != null) {
            return item.prefab;
        } else {
            return null;
        }
    }

    public int getPrefabUID(string prefabName) {
        PrefabDatabaseItem item = prefabs.Find(item => item.prefab.name == prefabName);

        if (item != null) {
            return item.uid;
        }

        return -1;
    }

    [System.Serializable]
    public class PrefabDatabaseItem {
        public GameObject prefab;
        public int uid;
    }
}
