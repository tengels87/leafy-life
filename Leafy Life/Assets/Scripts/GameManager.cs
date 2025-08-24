using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void OnEnable() {
        SceneManager.MapChangedEvent += OnMapChanged;
    }

    private void OnDisable() {
        SceneManager.MapChangedEvent -= OnMapChanged;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnMapChanged(MapController.MapType mapType) {

        // start ingame clock when first entering tree house
        if (mapType == MapController.MapType.TREEHOUSE) {
            DaytimeManager daytimeMamager = WorldConstants.Instance.getDaytimeManaer();
            if (daytimeMamager.isClockPaused()) {
                daytimeMamager.resetClock(6);
            }
        }
    }
}
