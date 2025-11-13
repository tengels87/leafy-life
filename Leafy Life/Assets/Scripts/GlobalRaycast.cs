using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GlobalRaycast {
    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    public static bool IsTappedInWorld(GameObject target) {
        if (IsPointerOverUI()) {
            return false;
        }

        // Get mouse position in world space
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Perform the raycast
        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero); // No direction needed as we're just checking a point

        // Check if this game object was hit
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider != null && hit.collider.gameObject.Equals(target)) {
                return true;
            }
        }

        return false;
    }

    public static bool IsTappedInUI(GameObject target) {
        GraphicRaycaster graphicRaycaster = target.GetComponent<GraphicRaycaster>();
        EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();

        if (graphicRaycaster == null || eventSystem == null)
            return false;

        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();

        graphicRaycaster.Raycast(pointerEventData, results);

        foreach (RaycastResult result in results) {
            if (result.gameObject.Equals(target)) {
                return true;
            }
        }

        return false;
    }

    public static bool IsPointerOverUI() {
#if UNITY_EDITOR || UNITY_STANDALONE
        return EventSystem.current.IsPointerOverGameObject();
#elif UNITY_ANDROID || UNITY_IOS
    if (Input.touchCount > 0)
        return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
    return false;
#else
    return false;
#endif
    }
}