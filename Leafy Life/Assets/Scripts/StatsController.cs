using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsController : MonoBehaviour
{
    public float nutrition = 50;
    public float sleep = 50;

    public GameObject nutritionVisual;
    public GameObject sleepVisual;

    private bool doAddSleep = false;

    void OnEnable() {
        DaytimeManager.hourTickEvent += OnHourTick;
    }

    void OnDisable() {
        DaytimeManager.hourTickEvent -= OnHourTick;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void changeNutritionValue(float chg) {
        nutrition = Mathf.Clamp(nutrition + chg, 0, 100);
    }

    public void changeSleepValue(float chg) {
        sleep = Mathf.Clamp(sleep + chg, 0, 100);
    }

    public void addQueueIncreaseSleep() {
        doAddSleep = true;
    }

    private void OnHourTick(float timestamp) {
        changeNutritionValue(-3);

        if (doAddSleep) {
            changeSleepValue(10);
            doAddSleep = false;
        } else {
            changeSleepValue(-5);
        }

        nutritionVisual.transform.localScale = new Vector3(nutrition * 0.01f, 1, 1);
        sleepVisual.transform.localScale = new Vector3(sleep * 0.01f, 1, 1);
    }
}
