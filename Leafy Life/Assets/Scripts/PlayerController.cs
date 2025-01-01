using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float movementSpeed = 1f;

    private Vector3 moveDirection;
    private List<Vector2> waypointList = new List<Vector2>();
    private List<GameAction> actionList = new List<GameAction>();
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
                GameAction gameAction = new GameAction(GameAction.ActionType.WALKTO, new Vector2(targetPos.x, targetPos.y));
                addAction(gameAction);
            }
        }

        processAllActions();


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

    public void addAction(GameAction action) {
        actionList.Add(action);
    }

    public void addActionFirst(GameAction action) {
        actionList.Insert(0, action);
    }

    public void removeAction(GameAction action) {
        actionList.Remove(action);
    }

    public List<GameAction> getActionList() {
        return actionList;
    }

    public void clearWaypoints() {
        waypointList.Clear();
    }

    public void clearActions() {
        actionList.Clear();
    }

    private void processAllActions() {
        MapController mapController = WorldConstants.Instance.getMapController();

        List<GameAction> allActions = getActionList();
        if (allActions.Count > 0) {

            switch(allActions[0].actionType) {
                case GameAction.ActionType.WALKTO:
                    // if all waypoint are done, pick next target
                    // and calculate path
                    if (waypointList.Count == 0) {
                        if (mapController.checkPath(allActions[0].targetPosition, getPosition(), out List<Vector2> resultPath)) {
                            setWaypoints(resultPath);
                            removeAction(allActions[0]);
                        }
                    }

                    break;

                default:
                    break;
            }
        }
    }

    private void navigateAllWaypoints() {
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
