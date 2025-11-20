using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {

    [System.Flags]
    public enum StructureType {
        PLATFORM = 1 << 0,
        LADDER = 1 << 1,
        ROOM = 1 << 2,

        GRASS = 1 << 5,
        SOIL = 1 << 6,
        WATER = 1 << 7,

        CROP = 1 << 10,

        WORKBENCH = 1 << 15,
        TRUNK = 1 << 16,
        SLEEPABLE = 1 << 17,
        FURNITURE = 1 << 18,
        DECORATION = 1 << 19,

        BLOCK = 1 << 25,
        MAPLINK = 1 << 26,

        TREEHOUSE_BUILDBLOCK = 1 << 30,
    }

    public enum BuildType {
        REPLACE_TILE,
        ATTACH_TO_SLOT
    }

    public SpriteRenderer spriteRenderer;
    public StructureType structureType;
    public bool attachesToPlatform;
    public StructureType buildOnStructure;
    public BuildType buildType;

    public bool isInteractable;
    public PlayerController.InteractionType interactionType;
    public MapController.MapType linkToMapType;

    [SerializeField]
    public List<GridFootprint> gridFootprint = new List<GridFootprint>(1);

    void OnEnable() {
        UnifiedInputModule.Instance.OnTap += OnTap;
    }

    void OnDisable() {
        UnifiedInputModule.Instance.OnTap -= OnTap;
    }

    void Start() {

        // init crop on start, if needed
        Crop crop = this.gameObject.GetComponentInChildren<Crop>();
        if (crop != null) {
            if (crop.initializeOnStart == false) {
                crop.init();
            }
        }
    }

    void Update() {

    }

    private void OnTap(Vector2 tapPos) {
        // interact on tap
    }

    [Serializable]
    public class GridFootprint {
        public int gridX;
        public int gridY;
        public string canConnectAt = "0123";
        public bool isWalkable;
        public bool hasSlot;

        public GridFootprint() { }
    }
}
