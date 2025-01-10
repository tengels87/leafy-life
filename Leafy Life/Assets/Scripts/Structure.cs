using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {

    public enum StructureType {
        PLATFORM,
        LADDER,
        FURNITURE
    }

    public SpriteRenderer spriteRenderer;
    public string canConnectAt = "0123";
    public StructureType structureType;
    public bool attachesToPlatform;
    public bool attachesToSlot;

    [SerializeField]
    public List<GridFootprint> gridFootprint = new List<GridFootprint>();

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
        public bool isSlot;
    }
}
