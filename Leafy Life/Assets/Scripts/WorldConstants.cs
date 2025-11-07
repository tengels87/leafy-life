using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class WorldConstants {
    public static string objName_gameManager = "GameManager";
    public static string objName_loadingManager = "LoadingManager";
    public static string objName_mapController = "MapController";
    public static string objName_hudManager = "HUDmanager";
    public static string objName_prefabDefRegistryManager = "PrefabDefRegistryManager";
    public static string objName_player = "Player";
    public static string objName_inventory = "Inventory";
    public static string objName_daytimeManager = "DaytimeManager";
    public static string objName_audioManager = "AudioManager";
    public static string objName_saveSystem = "SaveSystem";

    public GameManager gameManager;
    public SceneManager sceneManager;
    public MapController mapController;
    public HudManager hudManager;
    public StatsController statsController;
    public PrefabDefRegistryManager prefabDefRegistryManager;
    public Transform player;
    public PlayerController playerController;
    public Inventory inventory;
    public DaytimeManager daytimeManager;
    public AudioPool audioPool;
    public SaveSystem saveSystem;

    private System.Random rnd = new System.Random();

    static readonly WorldConstants _instance = new WorldConstants();
    public static WorldConstants Instance {
        get {
            return _instance;
        }
    }

    private WorldConstants() {

    }

    public int RND(int max) {
        return rnd.Next(max);
    }

    public GameManager getGameManager() {
        if (gameManager == null) {
            gameManager = GameObject.Find(objName_gameManager).GetComponent<GameManager>();
        }

        return gameManager;
    }

    public SceneManager getSceneManager() {
        if (sceneManager == null) {
            sceneManager = GameObject.Find(objName_loadingManager).GetComponent<SceneManager>();
        }

        return sceneManager;
    }

    public MapController getMapController() {
        //if (mapController == null) {
            mapController = GameObject.Find(objName_mapController).GetComponent<MapController>();
        //}

         return mapController;
    }

    public HudManager getHudManager() {
        if (hudManager == null) {
            hudManager = GameObject.Find(objName_hudManager).GetComponent<HudManager>();
        }

        return hudManager;
    }

    public StatsController getStatsController() {
        if (statsController == null) {
            Transform player = getPlayer();
            if (player != null) {
                statsController = player.GetComponent<StatsController>();
            }
        }

        return statsController;
    }

    public PrefabDefRegistryManager getPrefabDefRegistryManager() {
        if (prefabDefRegistryManager == null) {
            prefabDefRegistryManager = GameObject.Find(objName_prefabDefRegistryManager).GetComponent<PrefabDefRegistryManager>();
        }

        return prefabDefRegistryManager;
    }

    public Transform getPlayer() {
        if (player == null) {
            player = GameObject.Find(objName_player).GetComponent<Transform>();
        }

        return player;
    }

    public PlayerController getPlayerController() {
        if (playerController == null) {
            playerController = GameObject.Find(objName_player).GetComponent<PlayerController>();
        }

        return playerController;
    }

    public Inventory getInventory() {
        if (inventory == null) {
            inventory = GameObject.Find(objName_inventory).GetComponent<Inventory>();
        }

        return inventory;
    }

    public DaytimeManager getDaytimeManaer() {
        if (daytimeManager == null) {
            daytimeManager = GameObject.Find(objName_daytimeManager).GetComponent<DaytimeManager>();
        }

        return daytimeManager;
    }

    public AudioPool getAudioPool() {
        if (audioPool == null) {
            audioPool = GameObject.Find(objName_audioManager).GetComponent<AudioPool>();
        }

        return audioPool;
    }

    public SaveSystem getSaveSystem() {
        if (saveSystem == null) {
            saveSystem = GameObject.Find(objName_saveSystem).GetComponent<SaveSystem>();
        }

        return saveSystem;
    }
}
