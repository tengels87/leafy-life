using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public string canConnectAt = "0123";
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
