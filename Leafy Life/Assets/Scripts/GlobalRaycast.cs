using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
        if (GetPointerPosition(out Vector2 pointerPos)) {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(pointerPos);

            // Perform the raycast
            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero); // No direction needed as we're just checking a point

            // Check if this game object was hit
            foreach (RaycastHit2D hit in hits) {
                if (hit.collider != null && hit.collider.gameObject.Equals(target)) {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsTappedInUI(GameObject target) {
        GraphicRaycaster graphicRaycaster = target.GetComponent<GraphicRaycaster>();
        EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();

        if (graphicRaycaster == null || eventSystem == null)
            return false;

        if (GetPointerPosition(out Vector2 pointerPos)) {
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = pointerPos;

            List<RaycastResult> results = new List<RaycastResult>();

            graphicRaycaster.Raycast(pointerEventData, results);

            foreach (RaycastResult result in results) {
                if (result.gameObject.Equals(target)) {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool GetPointerPosition(out Vector2 pointerPosition, Vector2? customPointerPosition = null) {
        if (customPointerPosition.HasValue) {
            pointerPosition = customPointerPosition.Value;
        } else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) {
            pointerPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        } else if (Mouse.current != null) {
            pointerPosition = Mouse.current.position.ReadValue();
        } else {
            pointerPosition = Vector2.zero;
            return false; // no pointer input
        }

        return true;
    }

    public static bool IsPointerDown() {
        if (Touchscreen.current != null) {
            var touch = Touchscreen.current.primaryTouch;
            UnityEngine.InputSystem.TouchPhase phase = touch.phase.ReadValue();

            if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        return false;
    }

    public static bool IsPointerUp() {
        if (Touchscreen.current != null) {
            var touch = Touchscreen.current.primaryTouch;
            UnityEngine.InputSystem.TouchPhase phase = touch.phase.ReadValue();

            if (phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            return true;

        return false;
    }

    public static bool IsPointerOverUI() {
        PointerEventData eventData = new PointerEventData(EventSystem.current);

        if (GetPointerPosition(out Vector2 pointerPos)) {
            eventData.position = pointerPos;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        } else {
            return false;
        }
    }
}