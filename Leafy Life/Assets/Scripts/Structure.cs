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
    public StructureType structureType;
    public bool attachesToPlatform;
    public bool attachesToSlot;

    public bool isInteractable;
    public PlayerController.InteractionType interactionType;
    public MapController.MapType linkToMapType;

    [SerializeField]
    public List<GridFootprint> gridFootprint = new List<GridFootprint>(1);

    void Start() {

    }

    void Update() {

    }

    public Structure getInteractableStructure() {
        if (this.isInteractable) {
            return this;
        }

        foreach (GridFootprint fp in gridFootprint) {
            if (fp.slot != null && fp.slot.isInteractable) {
                return fp.slot;
            }
        }

        return null;
    }

    [Serializable]
    public class GridFootprint {
        public int gridX;
        public int gridY;
        public string canConnectAt = "0123";
        public bool isWalkable;
        public bool hasSlot;
        

        //[HideInInspector]
        public Structure slot;

        public GridFootprint() { }
    }
}
