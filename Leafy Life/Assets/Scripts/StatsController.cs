using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsController : MonoBehaviour
{
    public float nutrition = 50;
    public float sleep = 50;

    public GameObject nutritionVisual;
    public GameObject sleepVisual;

    void Start()
    {
        
    }

    void Update()
    {
        if (nutrition > 0) {
            nutrition -= Time.deltaTime * 1f;
        }

        if (sleep > 0) {
            sleep -= Time.deltaTime * 1f;
        }

        nutritionVisual.transform.localScale = new Vector3(nutrition * 0.01f, 1, 1);
        sleepVisual.transform.localScale = new Vector3(sleep * 0.01f, 1, 1);
    }

    public void changeNutritionValue(float chg) {
        nutrition = Mathf.Clamp(nutrition + chg, 0, 100);
    }

    public void changeSleepValue(float chg) {
        sleep = Mathf.Clamp(sleep + chg, 0, 100);
    }
}
