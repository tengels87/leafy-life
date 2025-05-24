using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGameObject : MonoBehaviour
{
    public GameObject targetGameObject;
    public GameObject toggleGameObject;

    private bool isTargetEnabled= false;

    void Awake() {
        //targetGameObject.SetActive(isTargetEnabled);
    }

    void Start()
    {
        
    }

    void Update() {
        // tap to toggle
        if (Input.GetMouseButtonDown(0)) {
            Vector2 targetPos = MapController.pixelPos2WorldPos(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(targetPos, new Vector3(0, 0, 1));
            bool hasHit = false;
            foreach (RaycastHit2D hit in hits) {
                if (hit.collider.gameObject.Equals(toggleGameObject)) {
                    hasHit = true;
                    break;
                }
            }
            if (hasHit) {
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
