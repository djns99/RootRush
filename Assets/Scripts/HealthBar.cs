using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Image healthBarTop;
    public float currentNutrition = 0.5f;
    public float depletionRate = 0.05f;
    public float damageAmount = 0.2f;
    public float nutritionAdd = 0.1f;

    public void Damage()
    {
        Debug.Log("Damage");
        currentNutrition -= damageAmount;
    }

    public void Fertilize()
    {
        Debug.Log("Fertilize");
        currentNutrition += nutritionAdd;
    }

    // Start is called before the first frame update
    void Start()
    {
        healthBarTop = GetComponent<Image>();
        healthBarTop.fillMethod = Image.FillMethod.Horizontal;
        healthBarTop.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthBarTop.fillAmount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        currentNutrition = Mathf.Clamp01(currentNutrition);
        currentNutrition -= depletionRate * Time.deltaTime;
        
        if (currentNutrition < 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        healthBarTop.fillAmount = Mathf.Clamp01(currentNutrition);
        Debug.Log(currentNutrition);
    }
}
