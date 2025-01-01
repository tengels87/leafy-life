using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float movementSpeed = 1f;

    private Vector3 moveDirection;
    private List<Vector2> waypointList = new List<Vector2>();
    private List<Vector2> targetList = new List<Vector2>();
    private bool canMove = true;

    private SpriteRenderer spriteRenderer;
    private Animator anim;

    void Start() {
        this.transform.position = new Vector3(4, 4, 0);

        spriteRenderer = this.GetComponent<SpriteRenderer>();
        anim = this.GetComponent<Animator>();
    }

    void Update() {

        MapController mapController = WorldConstants.Instance.getMapController();

        // move on tap
        if (Input.GetMouseButtonDown(1)) {
            Vector3 targetPos = MapController.pixelPos2WorldPos(Input.mousePosition);

            if (mapController.getTile(new Vector2(targetPos.x, targetPos.y)) != null) {
                addTarget(new Vector2(targetPos.x, targetPos.y));
            }
        }


        List<Vector2> allTargets = getTargetList();
        if (allTargets.Count > 0) {

            // if all waypoint are done, pick next target
            // and calculate path
            if (waypointList.Count == 0) {
                if (mapController.checkPath(allTargets[0], getPosition(), out List<Vector2> resultPath)) {
                    setWaypoints(resultPath);
                    removeTarget(allTargets[0]);
                }
            }
        }

        // walk along current path
        navigateAllWaypoints();

        // flip sprite according to movementDirection
        Vector3 scale = spriteRenderer.transform.localScale;
        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y)) {

            scale.x = 0.1f * moveDirection.x;
            anim.SetBool("isClimbing", false);
        } else if (Mathf.Abs(moveDirection.x) < Mathf.Abs(moveDirection.y)) {
            scale.x = 0.1f;
            anim.SetBool("isClimbing", true);
        }
            spriteRenderer.transform.localScale = scale;
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

    public void navigateAllWaypoints() {
        if (waypointList.Count > 0) {
            anim.SetFloat("speed", 1f);

            Vector2 targetWaypoint = waypointList[0];
            float dist = Vector2.Distance(targetWaypoint, getPosition());
            if (dist > 0.1f) {
                if (canMove) {
                    moveDirection = (targetWaypoint - getPosition()).normalized;

                    this.transform.position = this.transform.position + moveDirection * movementSpeed * Time.deltaTime;
                }
            } else {
                waypointList.RemoveAt(0);
            }
        } else {
            anim.SetFloat("speed", 0);
        }
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
