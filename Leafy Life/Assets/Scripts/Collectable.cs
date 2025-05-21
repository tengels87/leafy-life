using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectable : MonoBehaviour
{
    public static UnityAction<Inventory.InventoryItem> OnCollectedEvent;

    public Inventory.InventoryItem itemData;

    private bool isCollected = false;

    void Start()
    {
        
    }

    void Update()
    {
        if (isCollected) {
            return;
        }

        if (Input.GetMouseButtonDown(0)) {
            if (isTapped()) {
                OnCollectedEvent?.Invoke(itemData);

                isCollected = true;
                Object.Destroy(this.gameObject);
            }
        }
    }

    private bool isTapped() {

        Vector2 mouseWorldPos = MapController.pixelPos2WorldPos(Input.mousePosition);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, new Vector3(0, 0, 1));

        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.gameObject.Equals(this.gameObject)) {
                return true;
            }
        }

        return false;
    }
}
