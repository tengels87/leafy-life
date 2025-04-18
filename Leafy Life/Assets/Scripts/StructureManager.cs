using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureManager : MonoBehaviour
{
    public GameObject prefab_platform;
    public GameObject prefab_ladder;

    public GameObject prefab_grass;

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
}
