using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private List<Vector2> waypointList = new List<Vector2>();
    private List<Vector2> targetList = new List<Vector2>();
    private bool canMove = true;

    void Start() {
        this.transform.position = new Vector3(4, 4, 0);
    }

    void Update() {

        // walk along path
        if (waypointList.Count > 0) {
            Vector2 targetWaypoint = waypointList[0];
            float dist = Vector2.Distance(targetWaypoint, getPosition());
            if (dist > 0.1f) {
                if (canMove) {
                    Vector3 moveDIr = (targetWaypoint - getPosition()).normalized;

                    this.transform.position = transform.position + moveDIr * 4 * Time.deltaTime;
                }
            } else {
                waypointList.RemoveAt(0);
            }
        }
    }

    public Vector2 getPosition() {
        return new Vector2(this.transform.position.x, this.transform.position.y);
    }

    public void setWaypoints(List<Vector2> waypoints) {
        waypointList = new List<Vector2>(waypoints);
    }

    public void addWaypoints(List<Vector2> waypoints) {
        waypointList.AddRange(waypoints);
    }

    public void addTarget(Vector2 target) {
        targetList.Add(target);
    }

    public void addTargetFirst(Vector2 target) {
        targetList.Insert(0, target);
    }

    public void removeTarget(Vector2 target) {
        targetList.Remove(target);
    }

    public List<Vector2> getTargetList() {
        return targetList;
    }

    public void clearWaypoints() {
        waypointList.Clear();
    }

    public void clearTargets() {
        targetList.Clear();
    }

    IEnumerator CoroutineFreeze(Action callback = null) {
        if (canMove) {
            canMove = false;

            yield return new WaitForSeconds(0.3f);

            canMove = true;

            callback?.Invoke();
        } else {
            yield return null;
        }
    }
}
