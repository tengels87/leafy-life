using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StatsController : MonoBehaviour
{
    public static UnityAction NutritionIncreasedEvent;
    public static UnityAction NutritionLowEvent;
    public static UnityAction SleepLowEvent;

    public float nutrition = 50;
    public float sleep = 50;

    public GameObject nutritionVisual;
    public GameObject sleepVisual;

    private bool doAddSleep = false;

    void OnEnable() {
        DaytimeManager.minuteTickEvent += OnHourTick;
    }

    void OnDisable() {
        DaytimeManager.minuteTickEvent -= OnHourTick;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void changeNutritionValue(float chg) {
        nutrition = Mathf.Clamp(nutrition + chg, 0, 100);

        if (chg > 0) {
            NutritionIncreasedEvent?.Invoke();
        }
        if (nutrition < 40) {
            NutritionLowEvent?.Invoke();
        }

        updateVisuals();
    }

    public void changeSleepValue(float chg) {
        sleep = Mathf.Clamp(sleep + chg, 0, 100);

        if (sleep < 40) {
            SleepLowEvent?.Invoke();
        }

        updateVisuals();
    }

    public void addQueueIncreaseSleep() {
        doAddSleep = true;
    }

    private void OnHourTick(float timestamp) {
        changeNutritionValue(-0.1f);

        if (doAddSleep) {
            changeSleepValue(1);
            doAddSleep = false;
        } else {
            changeSleepValue(-0.03f);
        }
    }

    private void updateVisuals() {
        nutritionVisual.transform.localScale = new Vector3(nutrition * 0.01f, 1, 1);
        sleepVisual.transform.localScale = new Vector3(sleep * 0.01f, 1, 1);
    }
}
