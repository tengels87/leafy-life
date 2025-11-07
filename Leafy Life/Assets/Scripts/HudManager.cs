using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudManager : MonoBehaviour {

    void OnEnable() {
        SceneManager.MapChangedEvent += OnMapChanged;
    }

    void OnDisable() {
        SceneManager.MapChangedEvent -= OnMapChanged;
    }

    void Start() {
        Camera cam = Camera.main;

        updateCameraFOV(8, 8);

        float visibleWorldWidth = 2 * cam.orthographicSize * cam.aspect;
    }

    void Update() {

    }

    private void updateCameraFOV(int gridWidth, int gridHeight) {
        Camera cam = Camera.main;
        if (cam != null) {
            float aspect = (float)cam.pixelWidth / (float)cam.pixelHeight;

            int maxGridHeight = gridHeight;

            float zoom = (float)maxGridHeight / cam.orthographicSize;

            cam.orthographicSize = (float)maxGridHeight * 0.5f;

            cam.transform.position = new Vector3((float)(maxGridHeight) * aspect / zoom - 0.5f, (float)(maxGridHeight) / zoom - 0.5f, cam.transform.position.z);
        }
    }

    private void OnMapChanged(MapController.MapType mapType) {
        
    }
}
