using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCollapse : MonoBehaviour
{
    public GameObject menuGameObject;
    public GameObject collapseButton;

    private bool isExpanded = false;

    void Awake() {
        menuGameObject.SetActive(isExpanded);
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
                if (hit.collider.gameObject.Equals(collapseButton)) {
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
        isExpanded = true;

        menuGameObject.SetActive(true);
    }

    public void hide() {
        isExpanded = false;

        menuGameObject.SetActive(false);
    }

    public void toggle() {
        isExpanded = !isExpanded;

        menuGameObject.SetActive(isExpanded);
    }
}
