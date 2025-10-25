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

    public GameObject nutritionStatsBar;
    public GameObject sleepStatsBar;

    private bool doAddSleep = false;

    void OnEnable() {
        DaytimeManager.minuteTickEvent += OnHourTick;
    }

    void OnDisable() {
        DaytimeManager.minuteTickEvent -= OnHourTick;
    }

    void Start()
    {
        SaveSystem.GameData saveData = WorldConstants.Instance.getSaveSystem().getLoadedData();
        if (saveData != null) {
            nutrition = saveData.nutritionValue;
            sleep = saveData.sleepValue;
        }
    }

    void Update()
    {
        
    }

    public void changeNutritionValue(float chg) {
        float lastValue = nutrition;

        nutrition = Mathf.Clamp(nutrition + chg, 0, 100);

        if (chg > 0) {
            NutritionIncreasedEvent?.Invoke();
        }
        if ((nutrition < 40 && lastValue > 40)
            || (nutrition < 30 && lastValue > 30)
            || (nutrition < 20 && lastValue > 20)
            || (nutrition < 10 && lastValue > 10)
            || (nutrition < 5 && lastValue > 5)) {
            NutritionLowEvent?.Invoke();
        }

        updateVisuals();
    }

    public void changeSleepValue(float chg) {
        float lastValue = sleep;

        sleep = Mathf.Clamp(sleep + chg, 0, 100);

        if ((sleep < 40 && lastValue > 40)
            || (sleep < 30 && lastValue > 30)
            || (sleep < 20 && lastValue > 20)
            || (sleep < 10 && lastValue > 10)
            || (sleep < 5 && lastValue > 5)) {
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
            changeSleepValue(0.5f);
            doAddSleep = false;
        } else {
            changeSleepValue(-0.06f);
        }
    }

    private void updateVisuals() {
        nutritionStatsBar.transform.localScale = new Vector3(nutrition * 0.01f, 1, 1);
        sleepStatsBar.transform.localScale = new Vector3(sleep * 0.01f, 1, 1);
    }
}
