using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalCredits : MonoBehaviour
{
    public Animator credits;
    public float creditsMultiplier;

    public Animator reportCard;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpeedUp()
    {
        credits.speed = creditsMultiplier;
    }
    public void SpeedReturn()
    {
        credits.speed = 1;
    }
    public void FadeCredits()
    {
        credits.SetTrigger("Fade");
    }

    public void ShowReportCard()
    {
        reportCard.SetTrigger("Start");
    }
    public void ToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
