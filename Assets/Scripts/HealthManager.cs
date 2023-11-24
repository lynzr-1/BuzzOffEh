using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public int hitPoints = 100;

    public void AdjustHitPoints(int amount)
    {
        hitPoints += amount;
        print("Adjusted hit points by " + amount + " - Health is now " + hitPoints);

        // If player's health is 0, he dies
        if (hitPoints <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        Debug.Log("Player died!");

        // Death animation

        // Disable player
        gameObject.SetActive(false);
    }
}
