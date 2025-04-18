using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {

    public enum StructureType {
        PLATFORM,
        LADDER,
        FURNITURE,
        GRASS,
        SOIL,
        CROP,
        SLEEPABLE,
        BLOCK,
        MAPLINK
    }

    public SpriteRenderer spriteRenderer;
    public string canConnectAt = "0123";
    public StructureType structureType;
    public bool attachesToPlatform;
    public bool attachesToSlot;

    [SerializeField]
    public List<GridFootprint> gridFootprint = new List<GridFootprint>(1);

    void Start() {

    }

    void Update() {

    }

    [Serializable]
    public struct GridFootprint {
        public int gridX;
        public int gridY;
        public bool isWalkable;
        public bool isInteractable;
        public PlayerController.InteractionType interactionType;
        public bool hasSlot;
        public MapController.MapType linkToMapType;
    }
}
