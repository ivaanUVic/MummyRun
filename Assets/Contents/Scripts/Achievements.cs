using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Achievements : MonoBehaviour
{

    [HideInInspector]
    public int currentCoins;
    [HideInInspector]
    public int currentScore;

    [HideInInspector]
    public int totalCoins;
    [HideInInspector]
    public int totalScore;

    [HideInInspector]
    public int highScore;

    public int minimumHighScore;

    private AudioSource endingAnimMusic;

    public Text lblScorePanelCollectedCoins, lblScorePanelCollectedScore;
    public Text lblScorePanelTotalCoins, lblScorePanelTotalScore;

    public GameObject ScorePanel;

    public GameObject ScoreFinal;

    public AudioClip Coin;

    public AudioClip PowerUp;

    [HideInInspector]
    public bool scoresLock;

    public float countedScored;
    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        //Reset dels valors, obtenim dels prefabs els valors desats
        ScoreFinal.SetActive(false);
        ScorePanel.SetActive(true);
        currentCoins = 0;
        currentScore = 0;
        endingAnimMusic = GameGlobals.Instance.Player.GetComponent<AudioSource>();
        totalCoins = PlayerPrefs.GetInt("totalCoins", 0);
        totalScore = PlayerPrefs.GetInt("totalScore", 0);


        lblScorePanelTotalCoins.text = totalCoins.ToString();
        lblScorePanelTotalScore.text = totalScore.ToString();


    }


    public void resetScoresAndAchievements()
    {
        PlayerPrefs.SetInt("totalCoins", 0);
        PlayerPrefs.SetInt("totalScore", 0);
    }


    private void Update()
    {

    }

    // SCORE --------------------------------------------------------------------------------------------------

    private int bonusCoins = 2;
    private int pointsCounterDuration = 60;
    private int scoreCounterDuration = 80;
    private int pointsCounter, scoresCounter,scoreWaitCounter;

    private int  storedScore,storedtotalCoins,storedTotalScore;

    public void saveScores()
    {

        currentCoins = GameGlobals.Instance.points;
        bonusCoins = GameGlobals.Instance.plus;
        //pointsCounterDuration = currentCoins;
        scoreCounterDuration = bonusCoins;

        // Saving Scores al player prefs
        PlayerPrefs.SetInt("totalCoins", totalCoins + currentCoins);
        PlayerPrefs.SetInt("totalScore", (totalScore + bonusCoins));
        

        coundDownScore();

    }

    public void coundDownScore()
    {
        lblScorePanelCollectedCoins.text = GameGlobals.Instance.points.ToString();
        lblScorePanelCollectedScore.text = GameGlobals.Instance.plus.ToString();
        //ScoreFinal.SetActive(true);
        //ScorePanel.SetActive(false);

        startCountDownScore();
       
    }

    private void startCountDownScore()
    {
       

        storedScore = currentScore;
        storedtotalCoins = totalCoins;
        storedTotalScore = totalScore;

        pointsCounter = 0;
        scoresCounter = 0;
        scoreWaitCounter = 0;

        SceneManager.LoadScene("ScoreScene");
        //scoring = StartCoroutine(countScore());
    }

    private Coroutine scoring;
    public void cancelCountScore()
    {
        if (scoring != null)
        {
            StopCoroutine(scoring);
        }
    }

    private IEnumerator countScore()
    {

        print("here");
        int pitchTrigger = 0;

        // Coins Counting
        if(currentCoins > 0)
            while (true)
            {

             
                if (pointsCounter <= pointsCounterDuration)
                {
                    float countedPoints = ((float)currentCoins / (float)pointsCounterDuration) * (float)pointsCounter;

                    lblScorePanelCollectedCoins.text = (currentCoins - (int)countedPoints).ToString();
                   
                    totalCoins = storedtotalCoins + (int)countedPoints;
                    lblScorePanelTotalCoins.text = totalCoins.ToString();



                    float pitch = 1.0f + 0.3f / pointsCounterDuration * pointsCounter;
                    if (pitchTrigger == 3)
                    {
                        //Aixi no reproduim un so per cada moneda
                        GameGlobals.Instance.PlaySound(Coin);
                        pitchTrigger = 0;
                    }
                    else
                    {
                        pitchTrigger++;
                    }

                   
                    pointsCounter++;
                    yield return new WaitForSeconds(Time.fixedDeltaTime);

                }
                else
                {
                    break;
                }

            }

        // Wait a while

        while (true)
        {

            if (scoreWaitCounter <= 100)
            {
                scoreWaitCounter++;
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
            else
            {
                break;
            }

        }

        // Scores Counting
        if(bonusCoins > 0)
            while (true)
            {

                if (scoresCounter <= scoreCounterDuration)
                {

                    float countedPoints = ((float)bonusCoins / (float)pointsCounterDuration) * (float)pointsCounter;

                    lblScorePanelCollectedScore.text = (bonusCoins - (int)countedPoints).ToString();

                    totalScore = storedTotalScore + (int)countedPoints;
                    currentScore = storedScore + (int)bonusCoins ;
                    print(currentScore);
                   // print(currentCoins);
                    print(countedPoints);
                    lblScorePanelTotalScore.text = totalScore.ToString();

                    GameGlobals.Instance.PlaySound(PowerUp);

                    scoresCounter++;
                    countedScored = countedPoints;

                    yield return new WaitForSeconds(Time.fixedDeltaTime);
                }
                else
                {
                    break;
                }
         

            }

        // Waiting For Auto Start
        scoreWaitCounter = 0;
        if (PlayerPrefs.GetInt("audio", 1) == 0)
        {
            while (true)
            {

                if (scoreWaitCounter < 500)
                {
                    scoreWaitCounter++;
                    yield return new WaitForSeconds(Time.fixedDeltaTime);
                }
                else
                {

                    GameGlobals.Instance.goToMainMenu();
                    break;
                }

            }
        }
        else if (PlayerPrefs.GetInt("audio", 1) == 1)
        {
            if (endingAnimMusic != null)
            {
                while (true)
                {

                    if (endingAnimMusic.isPlaying == true)
                    {
                        yield return new WaitForSeconds(Time.fixedDeltaTime);
                    }
                    else
                    {
                        GameGlobals.Instance.goToMainMenu();
                        break;
                    }

                }
            }
            else
            {
                GameGlobals.Instance.goToMainMenu();
            }
        }


     
        
 


    }


}

