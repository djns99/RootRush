using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public TextMeshProUGUI scoreDisplay;
    public float scoreFeelBetterMultiplier = 100f;
    public static float score = 0.0f; 
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Score update");
        score += Time.deltaTime * scoreFeelBetterMultiplier;
        Debug.Log(score);
        scoreDisplay.SetText(((long)Mathf.Floor(score)).ToString("######"));
    }
}
