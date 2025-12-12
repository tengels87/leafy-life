using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    private List<MapController.TileData> userBuiltTilesList = new List<MapController.TileData>();

    private bool isInitiated = false;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void init() {
        if (isInitiated) return;

        SaveSystem.GameData saveData = WorldConstants.Instance.getSaveSystem().getLoadedData();
        if (saveData != null) {
            userBuiltTilesList = new List<MapController.TileData>(saveData.builtTilesList);
        }

        isInitiated = true;
    }

    public List<MapController.TileData> getUserBuildTiles() {
        init();

        return userBuiltTilesList;
    }

    public void addUserBuildTile(string prefabDefId, MapController.MapType mapType, int x, int y, string uuid) {
        MapController.TileData tileData = new MapController.TileData(prefabDefId, mapType, x, y, uuid);
        
        userBuiltTilesList.Add(tileData);
    }

    public void removeUserBuildTile(string uuid) {
        MapController.TileData tileData = userBuiltTilesList.Find(t => t.uuid == uuid);

        bool success = userBuiltTilesList.Remove(tileData);
        print(success);
    }
}
