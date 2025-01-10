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

        spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        anim = this.GetComponentInChildren<Animator>();
    }

    void Update() {

        MapController mapController = WorldConstants.Instance.getMapController();

        // move on tap
        if (Input.GetMouseButtonDown(1)) {
            Vector3 targetPos = MapController.pixelPos2WorldPos(Input.mousePosition);

            if (mapController.getTile(targetPos) != null) {
                GameAction gameAction = new GameAction(GameAction.ActionType.WALKTO, targetPos);
                addAction(gameAction);
            }
        }

        processAllActions();


        // walk along current path
        navigateAllWaypoints();

        // flip sprite according to movementDirection
        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y)) {
            bool doFlip = (moveDirection.x < 0);
            spriteRenderer.flipX = doFlip;
            anim.SetBool("isClimbing", false);
        } else if (Mathf.Abs(moveDirection.x) < Mathf.Abs(moveDirection.y)) {
            anim.SetBool("isClimbing", true);
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

    public void addAction(GameAction action) {
        switch (action.actionType) {
            case GameAction.ActionType.WALKTO:

                // find start position for path fiding
                // use current position or target position from last action in queue
                Vector2 startPosition = getPosition();
                if (actionList.Count > 0) {
                    startPosition = actionList[actionList.Count - 1].targetPosition;
                }

                MapController mapController = WorldConstants.Instance.getMapController();
                if (mapController.checkPath(action.targetPosition, startPosition, out List<Vector2> resultPath)) {
                    addWaypoints(resultPath);

                    actionList.Add(action);
                }
                break;

            default:
                actionList.Add(action);
                break;
        }
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
                        removeAction(allActions[0]);
                    }

                    break;

                case GameAction.ActionType.BUILD:
                    Vector2Int targetPositionInt = new Vector2Int((int)allActions[0].targetPosition.x, (int)allActions[0].targetPosition.y);
                    mapController.buildTile(targetPositionInt.x, targetPositionInt.y, (UnityEngine.GameObject)allActions[0].customData);
                    removeAction(allActions[0]);
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
