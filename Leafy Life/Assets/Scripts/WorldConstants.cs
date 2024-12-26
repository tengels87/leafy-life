﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class WorldConstants {
    public static string objName_gameManager = "GameManager";
    public static string objName_mapController = "MapController";
    public static string objName_hudManager = "HUDmanager";
    public static string objName_structureManager = "StructureManager";
    public static string objName_player = "Player";

    public GameManager gameManager;
    public MapController mapController;
    public HudManager hudManager;
    public StructureManager structureManager;
    public Transform player;

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

    public MapController getMapController() {
        if (mapController == null) {
            mapController = GameObject.Find(objName_mapController).GetComponent<MapController>();
        }

        return mapController;
    }

    public HudManager getHudManager() {
        if (hudManager == null) {
            hudManager = GameObject.Find(objName_hudManager).GetComponent<HudManager>();
        }

        return hudManager;
    }

    public StructureManager getStructureManager() {
        if (structureManager == null) {
            structureManager = GameObject.Find(objName_structureManager).GetComponent<StructureManager>();
        }

        return structureManager;
    }

    public Transform getPlayer() {
        if (player == null) {
            player = GameObject.Find(objName_player).GetComponent<Transform>();
        }

        return player;
    }
}
