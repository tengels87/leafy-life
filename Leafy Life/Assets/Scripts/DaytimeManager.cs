using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class DaytimeManager : MonoBehaviour
{
    public static UnityAction<float> minuteTickEvent;
    public static UnityAction<float> hourTickEvent;
    public static UnityAction<float> dayTickEvent;

    public static UnityAction NightStartedEvent;
    public static UnityAction NightFinishedEvent;

    public float dayLengthInSeconds = 60 * 3;

    public Light2D globalLight;
    public Light2D playerLight;
    public Color colorDaytime;
    public Color colorNighttime;

    [SerializeField]
    private float currentTime;
    private float lastTime;

    private static float nightStartTime = 22f;
    private static float nightEndTime = 6f;

    private bool isPaused = false;

    void Start()
    {
        resetClock();
        pauseClock();
    }

    void Update()
    {
        if (!isPaused) {

            // progress
            currentTime += (Time.deltaTime / dayLengthInSeconds) * 24f;

            // reset when day is finished (24h)
            if (currentTime > 24) {
                currentTime = 0;

                dayTickEvent?.Invoke(currentTime);
            }

            // fire tick events
            if ((int)(lastTime * 60f) < (int)(currentTime * 60f)) {
                minuteTickEvent?.Invoke(currentTime);
            }

            if ((int)lastTime < (int)currentTime) {
                hourTickEvent?.Invoke(currentTime);

                // day/night switch
                if ((int)currentTime == (int)nightStartTime) {
                    NightStartedEvent?.Invoke();

                    DOTween.To(() => globalLight.intensity, x => globalLight.intensity = x, 0f, 10);
                    DOTween.To(() => playerLight.intensity, x => playerLight.intensity = x, 1f, 10);
                }

                if ((int)currentTime == (int)nightEndTime) {
                    NightFinishedEvent?.Invoke();

                    DOTween.To(() => globalLight.intensity, x => globalLight.intensity = x, 1f, 10);
                    DOTween.To(() => playerLight.intensity, x => playerLight.intensity = x, 0f, 10);
                }
            }


            lastTime = currentTime;
        }
    }

    public void resumeClock() {
        isPaused = false;
    }

    public void pauseClock() {
        isPaused = true;
    }

    public void resetClock(float startTimeHour = 12) {
        currentTime = startTimeHour;

        resumeClock();
    }

    public bool isClockPaused() {
        return isPaused;
    }

    public float getCurrentTime() {
        return currentTime;
    }

    public float getCurrentTimeNormalized() {
        return currentTime / 24f;
    }

    public bool isNightTime() {
        return (currentTime < nightEndTime || currentTime > nightStartTime);
    }
}
