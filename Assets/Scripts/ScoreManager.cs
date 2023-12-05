using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int scorePoints = 0;

        public void AdjustScorePoints(int amount2)
    {
        scorePoints += amount2;
        
        
        print("Adjusted score points by " + amount2 + "\nSCORE: " + scorePoints);

    }

    
}

