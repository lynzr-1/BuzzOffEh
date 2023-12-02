using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public int hitPoints = 100;

    public Image healthMeter; //healthbar image
    public Sprite fiveHearts;
    public Sprite fourHearts;
    public Sprite threeHearts;
    public Sprite twoHearts;
    public Sprite oneHeart;
    public Sprite noHearts;

    public void AdjustHitPoints(int amount)
    {
        hitPoints += amount;
        print("Adjusted hit points by " + amount + " - Health is now " + hitPoints);

        if (hitPoints == 0)
        {
            healthMeter.sprite = noHearts;
            Death();
        }
        else if (hitPoints >= 1 && hitPoints <= 20)
        {
            healthMeter.sprite = oneHeart;
        }
        else if (hitPoints >= 21 && hitPoints <= 40)
        {
            healthMeter.sprite = twoHearts;
        }
        else if (hitPoints >= 41 && hitPoints <= 60)
        {
            healthMeter.sprite = threeHearts;
        }
        else if (hitPoints >= 61 && hitPoints <= 80)
        {
            healthMeter.sprite = fourHearts;
        }
        else 
        {
            hitPoints = 100; //player can't have greater than 100 health
            healthMeter.sprite = fiveHearts;
        }

        // If player's health is 0, he dies
        //if (hitPoints <= 0)
        //{
        //    Death();
        //}
    }

    public void Death()
    {
        Debug.Log("Player died!");

        // Death animation

        // Load Defeat scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Defeat");

        // Disable player
        //gameObject.SetActive(false);
    }
}
