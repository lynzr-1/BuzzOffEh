using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    //public static ScoreManager instance;
    int score = 0;
    public Text scoreText;
    

    private void Awake() {
        //instance = this;
    }


    void Start()
    {
        scoreText.text = "SCORE: " + score.ToString();

    }

        public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "SCORE: " + score.ToString();
        
        print("Adjusted score points by " + amount + "\nSCORE: " + score);

    }

    
}

