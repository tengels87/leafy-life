using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DaytimeManager : MonoBehaviour
{
    public static UnityAction<float> minuteTickEvent;
    public static UnityAction<float> hourTickEvent;

    public float dayLengthInSeconds = 60 * 3;

    private float currentTime;
    private float lastTime;

    void Start()
    {
        
    }

    void Update()
    {
        // progress
        currentTime += (Time.deltaTime / dayLengthInSeconds) * 24f;

        // reset when day is finished (24h)
        if (currentTime > 24) {
            currentTime = 0;
        }

        // fire tick events
        if ((int)(lastTime*60f)  < (int)(currentTime*60f)) {
            minuteTickEvent?.Invoke(currentTime);
        }
        
        if ((int)lastTime < (int)currentTime) {
            hourTickEvent?.Invoke(currentTime);
        }

        lastTime = currentTime;
    }

    public void startClock(float startTimeHour = 0) {
        currentTime = startTimeHour;

        
    }

    public float getCurrentTime() {
        return currentTime;
    }

    public float getCurrentTimeNormalized() {
        return currentTime / 24f;
    }

    public bool isNightTime() {
        return (currentTime < 6 || currentTime > 22);
    }
}
