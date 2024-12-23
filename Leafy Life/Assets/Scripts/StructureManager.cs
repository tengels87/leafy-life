using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureManager : MonoBehaviour
{
    public GameObject prefab_platform;

    void Start()
    {
        
    }

    void Update()
    {
        // Test
        if (Input.GetMouseButtonDown(0)) {
            MapController mapController = WorldConstants.Instance.getMapController();

            Vector2 pp = MapController.pixelPos2WorldPos(Input.mousePosition);

            if (mapController.isEmpty(new Vector2Int((int)pp.x, (int)pp.y))) {

                // instantiate
                GameObject platform = (GameObject)Object.Instantiate(prefab_platform);
                platform.transform.position = new Vector3((int)pp.x, (int)pp.y, 0);
                //goHeart.transform.SetParent(spriteContainer);

                MapController.Tile t = new MapController.Tile((int)pp.x, (int)pp.y, platform);
                
                mapController.spawnTile(t);

            }
        }
    }
}
