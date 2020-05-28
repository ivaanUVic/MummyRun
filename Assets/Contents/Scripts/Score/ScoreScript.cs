using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public Text BestScore;
    // Start is called before the first frame update
    void Start()
    {
        var totalScore = PlayerPrefs.GetInt("currentCoins") + (PlayerPrefs.GetInt("currentBonus") *20);

        

        if (totalScore > PlayerPrefs.GetInt("bestScore1"))
        {
            PlayerPrefs.SetInt("bestScore3", PlayerPrefs.GetInt("bestScore2"));
            PlayerPrefs.SetInt("bestScore2", PlayerPrefs.GetInt("bestScore1"));
            PlayerPrefs.SetInt("bestScore1", totalScore);
            
        }
        if (totalScore < PlayerPrefs.GetInt("bestScore1") && totalScore > PlayerPrefs.GetInt("bestScore2"))
        {
            PlayerPrefs.SetInt("bestScore3", PlayerPrefs.GetInt("bestScore2"));
            PlayerPrefs.SetInt("bestScore2", totalScore);
            

        }
        if (totalScore < PlayerPrefs.GetInt("bestScore1") && totalScore < PlayerPrefs.GetInt("bestScore2") && totalScore > PlayerPrefs.GetInt("bestScore3"))
        {
            PlayerPrefs.SetInt("bestScore3", totalScore);
        }
        if (PlayerPrefs.GetInt("bestScore2") < 1)
        {
            BestScore.text = string.Format(" 1 - {0} ", PlayerPrefs.GetInt("bestScore1"));
        }
        if (PlayerPrefs.GetInt("bestScore2") > 0 && PlayerPrefs.GetInt("bestScore3") < 1)
        {
            BestScore.text = string.Format(" 1 - {0} \n 2 - {1}", PlayerPrefs.GetInt("bestScore1"), PlayerPrefs.GetInt("bestScore2"));
        }
        else if (PlayerPrefs.GetInt("bestScore2") > 0 && PlayerPrefs.GetInt("bestScore3") > 0)
        {
            BestScore.text = string.Format(" 1 - {0} \n 2 - {1} \n 3 - {2}", PlayerPrefs.GetInt("bestScore1"), PlayerPrefs.GetInt("bestScore2"), PlayerPrefs.GetInt("bestScore3"));
        }
        //BestScore.text = string.Format(" 1 - {0} \n 2 - {1} \n 3 - {2}", PlayerPrefs.GetInt("bestScore1"), PlayerPrefs.GetInt("bestScore2"), PlayerPrefs.GetInt("bestScore3"));
        PlayerPrefs.SetInt("currentCoins", 0);
        PlayerPrefs.SetInt("currentBonus", 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
