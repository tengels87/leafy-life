using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGameObject : MonoBehaviour
{
    public GameObject targetGameObject;
    public GameObject toggleGameObject;
    public bool hideOnInit = true;

    private bool isTargetEnabled = false;

    void Awake() {
        if (hideOnInit) {
            targetGameObject.SetActive(isTargetEnabled);
        }
    }

    void Start()
    {
        
    }

    void Update() {

        // tap to toggle
        if (Input.GetMouseButtonDown(0)) {
            if (GlobalRaycast.IsTappedInUI(toggleGameObject)) {
                toggle();
            }
        }
    }

    public void show() {
        isTargetEnabled = true;

        targetGameObject.SetActive(true);
    }

    public void hide() {
        isTargetEnabled = false;

        targetGameObject.SetActive(false);
    }

    public void toggle() {
        isTargetEnabled = !isTargetEnabled;

        targetGameObject.SetActive(isTargetEnabled);
    }
}
