using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHandler : MonoBehaviour
{
    public static UnityAction<string> AminationFinishedEvent;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnAnimationFinished(string stateName) {
        AminationFinishedEvent?.Invoke(stateName);
    }
}
