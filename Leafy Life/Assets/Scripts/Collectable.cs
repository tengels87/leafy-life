using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DigitalRuby.Tween;

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
                collect();
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

    public void collect() {
        if (!isCollected) {
            isCollected = true;

            Vector3 playerPos = WorldConstants.Instance.getPlayer().transform.position;
            playerPos = playerPos - (playerPos - this.transform.position).normalized * 0.8f;
            Vector3 midTargetPos = this.gameObject.transform.position + (this.transform.position - playerPos).normalized * (0.2f + Random.value);

            // unlink, because parent will be probably be deleted
            this.transform.SetParent(null);

            this.gameObject.Tween("flyToPlayer" + Random.value, this.gameObject.transform.position, midTargetPos, 0.5f, TweenScaleFunctions.CubicEaseInOut, (t) => {
                this.gameObject.transform.position = t.CurrentValue;
            }).ContinueWith(new Vector3Tween().Setup(midTargetPos, playerPos, 0.3f, TweenScaleFunctions.Linear, (t) => {
                this.gameObject.transform.position = t.CurrentValue;
            }, (t) => {
                OnCollectedEvent?.Invoke(itemData);
                Object.Destroy(this.gameObject);
            }));
        }
    }
}
