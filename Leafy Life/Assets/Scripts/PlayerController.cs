using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public enum InteractionType {
        NONE,
        SLEEP,
        CRAFT
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
        interactionData.Add(new InteractionData("player_craft", "isCrafting", 2f));
    }

    void Start() {
        //setPosition(new Vector3(0, 0, 0));

        spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        anim = this.GetComponentInChildren<Animator>();

        statsController = this.GetComponentInChildren<StatsController>();
    }

    void Update() {

        // camera follow player
        Camera cam = Camera.main;
        if (cam != null) {
            cam.transform.position = this.gameObject.transform.position + new Vector3(0, 1, -10);
        }

        // move on tap
        if (Input.GetMouseButtonDown(0)) {
            Vector2 targetPos = MapController.pixelPos2WorldPos(Input.mousePosition);

            RaycastHit2D[] hits = Physics2D.RaycastAll(targetPos, new Vector3(0, 0, 1));
            foreach (RaycastHit2D hit in hits) {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("UI")) {
                    return;
                }
            }

            MapController mapController = WorldConstants.Instance.getMapController();

            Vector2Int targetPosInt = new Vector2Int((int)targetPos.x, (int)targetPos.y);

            MapController.Tile t = mapController.getTile(targetPosInt);

            if (t != null) {

                // set walk target there
                GameAction gameAction = new GameAction(GameAction.ActionType.WALKTO, targetPosInt);
                gameAction.customData = t.attachedGameObject;
                addAction(gameAction);

                // if there is a structure to interact with, do that
                if (t.attachedGameObject != null) {
                    Structure structureOnTile = t.attachedGameObject.GetComponent<Structure>();
                    if (structureOnTile != null) {
                        Structure interactablePart = structureOnTile.getInteractableStructure();
                        if (interactablePart != null) {
                            gameAction = new GameAction(GameAction.ActionType.INTERACT, targetPosInt);
                            gameAction.customData = interactablePart.gameObject;
                            addAction(gameAction);
                        }
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
        } else if (moveDirection == Vector3.zero) {
            anim.SetBool("isClimbing", false);
        }
    }

    public Vector2 getPosition2D() {
        return new Vector2(this.transform.position.x, this.transform.position.y);
    }

    public void setPosition(Vector3 pos) {
        this.transform.position = pos;
    }
    public void setPosition2D(Vector2Int pos) {
        this.transform.position = new Vector3(pos.x, pos.y, this.transform.position.z);
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
                Vector2 startPosition = getPosition2D();
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
            Structure structure = customDataObj?.GetComponent<Structure>();

            switch (allActions[0].actionType) {
                case GameAction.ActionType.WALKTO:
                    // if all waypoint are done, pick next target
                    // and calculate path
                    if (waypointList.Count == 0) {
                        removeAction(allActions[0]);

                        // teleport, if there is a map link
                        if (structure != null && structure.structureType == Structure.StructureType.MAPLINK) {
                            SceneManager sceneManager = WorldConstants.Instance.getSceneManager();
                            if (sceneManager != null) {
                                sceneManager.activateMap(structure.linkToMapType);
                            }
                        }
                    }

                    break;

                case GameAction.ActionType.BUILD:
                    if (canMove()) {
                        stopInteraction();
                        removeAction(allActions[0]);

                        startInteraction(interactionData[(int)(InteractionType.CRAFT) - 1], () => {
                            stopInteraction();
                            mapController.buildTile(targetPositionInt.x, targetPositionInt.y, customDataObj);
                        });
                    }

                    break;

                case GameAction.ActionType.INTERACT:
                    removeAction(allActions[0]);

                    startInteraction(interactionData[(int)(structure.interactionType) - 1]);

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
            
            float dist = Vector2.Distance(targetWaypoint, getPosition2D());
            if (dist > 0.1f) {
                moveDirection = (targetWaypoint - getPosition2D()).normalized;
                setPosition(this.transform.position + moveDirection * movementSpeed * Time.deltaTime);
            } else {
                waypointList.RemoveAt(0);
                moveDirection = Vector3.zero;
            }
        } else {
            anim.SetFloat("speed", 0);
        }
    }

    private IEnumerator CoroutineInteraction(InteractionData interaction, Action callback) {
        bool firedCallback = false;

        anim.SetBool(interaction.getStateName(), true);
        interaction.start();

        while (true) {

            // affect stats after animation has been finished (i.e. start sleeping after laying dwn was finished)
            if (interaction.isFinished()) {
                statsController.addQueueIncreaseSleep();

                if (!firedCallback) {
                    if (callback != null) {
                        callback();
                        firedCallback = true;
                    };
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void startInteraction(InteractionData interaction, Action callback = null) {
        currentInteraction = interaction;

        coroutineInteraction = StartCoroutine(CoroutineInteraction(interaction, () => {
            callback?.Invoke();
        }));
    }

    private void stopInteraction() {
        if (coroutineInteraction != null) {
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
        bool isCrafting = !interactionData[(int)(InteractionType.CRAFT) - 1].isFinished();

        return isSleeping == false && isCrafting == false;
    }

    public class InteractionData {
        private string name;
        private string stateName;
        private float duration;
        private float startTime;

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

        public void start() {
            startTime = Time.time;
        }

        public bool isFinished() {
            if (startTime + duration < Time.time) {
                return true;
            } else {
                return false;
            }
        }
    }
}
