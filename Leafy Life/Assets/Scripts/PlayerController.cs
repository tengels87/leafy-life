using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public enum InteractionType {
        SLEEP
    }

    public float movementSpeed = 1f;

    public StatsController statsController;

    private Vector3 moveDirection;
    private List<Vector2> waypointList = new List<Vector2>();
    private List<GameAction> actionList = new List<GameAction>();
    private List<InteractionData> interactionData = new List<InteractionData>();

    private SpriteRenderer spriteRenderer;
    private Animator anim;

    private InteractionData currentInteraction;
    private Coroutine coroutineInteraction;

    void Awake() {
        interactionData.Clear();
        interactionData.Add(new InteractionData("player_sleep", "isSleeping", 2f));
    }

    void Start() {
        this.transform.position = new Vector3(4, 4, 0);

        spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        anim = this.GetComponentInChildren<Animator>();

        statsController = this.GetComponentInChildren<StatsController>();
    }

    void Update() {
        
        // move on tap
        if (Input.GetMouseButtonDown(1)) {
            MapController mapController = WorldConstants.Instance.getMapController();

            Vector2 targetPos = MapController.pixelPos2WorldPos(Input.mousePosition);
            Vector2Int targetPosInt = new Vector2Int((int)targetPos.x, (int)targetPos.y);

            MapController.Tile t = mapController.getTile(targetPosInt);

            if (t != null) {

                // set walk target there
                GameAction gameAction = new GameAction(GameAction.ActionType.WALKTO, targetPosInt);
                addAction(gameAction);


                // if there is a structure to interact with, do that
                Structure structureOnTile = null;
                
                if (t.attachedGameObject != null) {
                    structureOnTile = t.attachedGameObject.GetComponent<Structure>();
                    if (structureOnTile != null && structureOnTile.gridFootprint[0].isInteractable) {
                        gameAction = new GameAction(GameAction.ActionType.INTERACT, targetPosInt);
                        gameAction.customData = t.attachedGameObject;
                        addAction(gameAction);
                    }
                }
            }
        }

        processAllActions();


        // walk along current path
        if (canMove()) {
            navigateAllWaypoints();
        }

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

                // find start position for path finding algorithm
                // start psition: use current position or target position from last action in queue
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
        List<GameAction> allActions = getActionList();
        if (allActions.Count > 0) {
            Vector2Int targetPositionInt = new Vector2Int((int)allActions[0].targetPosition.x, (int)allActions[0].targetPosition.y);
            MapController mapController = WorldConstants.Instance.getMapController();
            GameObject customDataObj = (GameObject)allActions[0].customData;

            switch (allActions[0].actionType) {
                case GameAction.ActionType.WALKTO:
                    // if all waypoint are done, pick next target
                    // and calculate path
                    if (waypointList.Count == 0) {
                        removeAction(allActions[0]);
                    }

                    break;

                case GameAction.ActionType.BUILD:
                    mapController.buildTile(targetPositionInt.x, targetPositionInt.y, customDataObj);
                    removeAction(allActions[0]);
                    break;

                case GameAction.ActionType.INTERACT:
                    removeAction(allActions[0]);

                    Structure structure = customDataObj.GetComponent<Structure>();
                    startInteraction(interactionData[(int)(structure.gridFootprint[0].interactionType)]);
                    
                    break;

                default:
                    break;
            }
        }
    }

    private void navigateAllWaypoints() {
        if (waypointList.Count > 0) {

            // stop any behaviour
            stopInteraction();

            // walk towards next way point
            anim.SetFloat("speed", 1f);

            Vector2 targetWaypoint = waypointList[0];
            
            float dist = Vector2.Distance(targetWaypoint, getPosition());
            if (dist > 0.1f) {
                moveDirection = (targetWaypoint - getPosition()).normalized;
                this.transform.position = this.transform.position + moveDirection * movementSpeed * Time.deltaTime;
            } else {
                waypointList.RemoveAt(0);
            }
        } else {
            anim.SetFloat("speed", 0);
        }
    }

    private IEnumerator CoroutineInteraction(InteractionData interaction) {
        float animDuration = interaction.getDuration();
        float animStartTime = Time.time;

        anim.SetBool(interaction.getStateName(), true);

        while (true) {
            if (animStartTime + animDuration < Time.time) {
                statsController.changeSleepValue(0.01f);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void startInteraction(InteractionData interaction) {
        currentInteraction = interaction;

        coroutineInteraction = StartCoroutine(CoroutineInteraction(interaction));
    }

    private void stopInteraction() {
        if (coroutineInteraction != null && canMove()) {
            anim.SetBool(currentInteraction.getStateName(), false);

            StopCoroutine(coroutineInteraction);
            coroutineInteraction = null;
        }
    }

    private bool isAnimationStatePlaying(string stateName) {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            return true;
        else
            return false;
    }

    private bool canMove() {
        bool isSleeping = isAnimationStatePlaying("player_sleep");
        
        return isSleeping == false;
    }

    public class InteractionData {
        private string name;
        private string stateName;
        private float duration;

        public InteractionData(string name, string stateName, float duration) {
            this.name = name;
            this.stateName = stateName;
            this.duration = duration;
        }

        public string getName() {
            return name;
        }

        public string getStateName() {
            return stateName;
        }

        public float getDuration() {
            return duration;
        }
    }
}
