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
        mapController.updateCameraFOV(6, 8);

        float visibleWorldWidth = 2 * cam.orthographicSize * cam.aspect;

        resourcesPanel.transform.localPosition = new Vector3(-0.5f * visibleWorldWidth, cam.orthographicSize, 2);
        buildPanel.transform.localPosition = new Vector3(-0.5f * visibleWorldWidth, cam.orthographicSize, 2);
    }

    void Update()
    {
        
    }
}
