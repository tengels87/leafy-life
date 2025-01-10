using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudManager : MonoBehaviour
{
    public GameObject resourcesPanel;
    public GameObject buildPanel;

    void Start()
    {
        Camera cam = Camera.main;

        MapController mapController = WorldConstants.Instance.getMapController();
        mapController.updateCameraFOV(8, 11);

        float visibleWorldWidth = 2 * cam.orthographicSize * cam.aspect;

        resourcesPanel.transform.localPosition = new Vector3(-0.5f * visibleWorldWidth, cam.orthographicSize, 10);
        buildPanel.transform.localPosition = new Vector3(-0.5f * visibleWorldWidth, cam.orthographicSize, 10);
    }

    void Update()
    {
        
    }
}
