using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveSystem : MonoBehaviour {
    private GameData currentGameData;
    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "save.json");
    private bool isInitialized = false;
    private bool isLoaded = false;


    void Update() {
        if (Input.GetMouseButtonDown(1)) {

            // populate save data object
            if (isInitialized) {
                if (currentGameData == null) {
                    currentGameData = new GameData();
                }

                // seed
                currentGameData.seed = WorldConstants.Instance.getMapController().getSeed();

                // inentory
                currentGameData.goldAmount = WorldConstants.Instance.getInventory().getGoldAmount();
                
                List<ItemData> inventoryItemsList = WorldConstants.Instance.getInventory().getInventoryItemsAsList();
                currentGameData.inventoryItemsUidList.Clear();
                foreach (ItemData itemData in inventoryItemsList) {
                    currentGameData.inventoryItemsUidList.Add(itemData.Id);
                }

                // character stats
                currentGameData.nutritionValue = WorldConstants.Instance.getStatsController().nutrition;
                currentGameData.sleepValue = WorldConstants.Instance.getStatsController().sleep;

                // World
                WorldConstants.Instance.getDaytimeManaer().getCurrentTime();

                currentGameData.builtTilesList = WorldConstants.Instance.getLoadingManager().getUserBuildTiles();

                saveGame();
            }
        }
    }

    public void saveGame() {
        if (currentGameData != null) {
            string json = JsonConvert.SerializeObject(currentGameData);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log("Game saved to " + SaveFilePath);
        }
    }
    
    public GameData getLoadedData() {
        if (isInitialized && isLoaded) {
            return currentGameData;
        } else {
            return loadGame();
        }
    }

    private GameData loadGame() {
        if (isInitialized)
            return currentGameData;

        if (!isInitialized) {
            if (File.Exists(SaveFilePath)) {
                string json = File.ReadAllText(SaveFilePath);
                currentGameData = JsonConvert.DeserializeObject<GameData>(json);
                isLoaded = true;
            } else {
                Debug.LogWarning("No game save file found!");
                currentGameData = null;
            } 
        }

        isInitialized = true;

        return currentGameData;
    }

    [System.Serializable]
    public class GameData {
        public int version = 1;
        public int seed = -1;

        public int goldAmount;
        public List<string> inventoryItemsUidList = new List<string>();

        public float nutritionValue;
        public float sleepValue;

        public List<MapController.TileData> builtTilesList = new List<MapController.TileData>();
    }
}
