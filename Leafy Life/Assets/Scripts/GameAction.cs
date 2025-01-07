using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAction {
    public enum ActionType {
        WALKTO,
        BUILD,
        INTERACT
    }

    public ActionType actionType;
    public bool isCancelable;
    public float duration;
    public Vector2 targetPosition;
    public System.Object customData;

    public GameAction(ActionType actionType, Vector2 targetPosition) {
        this.actionType = actionType;
        this.isCancelable = false;
        this.duration = 0;
        this.targetPosition = targetPosition;
    }
}
