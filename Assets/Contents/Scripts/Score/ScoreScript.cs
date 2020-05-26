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
        var arrInt = new int[3];
        var totalScore = PlayerPrefs.GetInt("totalCoins", 0) * PlayerPrefs.GetInt("totalScore", 0);

        //Inicialitzem per primer cop el array
        if (arrInt[0] < 1)
        {
            arrInt[0] = totalScore;
            arrInt[1] = 0;
            arrInt[2] = 0;
        }
        if (PlayerPrefs.GetInt("bestScore1") == 0)
        {
            PlayerPrefs.SetInt("bestScore1", totalScore);
        }
        else if(PlayerPrefs.GetInt("bestScore2") == 0)
        {
            PlayerPrefs.SetInt("bestScore2", 0);
        }
        else if (PlayerPrefs.GetInt("bestScore3") == 0)
        {
            PlayerPrefs.SetInt("bestScore3", 0);
        }
        /*La logica seria, inicialitzem els playerpref dels bestscore a 0
         * Un cop inicialitzades comparem el score1 i si el score actual es millor al anterior movem totes les posicions una posició
         * enrere i actualitzem el score1
         * Si no superem la primera posició comprovem la segona, si superas la segona llavors tornem a fer el mateix sense tocar la primera posició
         * 
         */
        if (totalScore > PlayerPrefs.GetInt("bestScore1"))
        {
            PlayerPrefs.SetInt("bestScore1", totalScore);
            arrInt[2] = arrInt[1];
            arrInt[1] = arrInt[0];
            arrInt[0] = totalScore;
            PlayerPrefs.SetInt("bestScore2", arrInt[1]);
            PlayerPrefs.SetInt("bestScore3", arrInt[2]);
        }
        if (totalScore < PlayerPrefs.GetInt("bestScore1") && totalScore > PlayerPrefs.GetInt("bestScore2"))
        {
            PlayerPrefs.SetInt("bestScore2", totalScore);
            arrInt[2] = arrInt[1];
            arrInt[1] = PlayerPrefs.GetInt("bestScore2");
            PlayerPrefs.SetInt("bestScore3", arrInt[2]);

        }
        if (totalScore < PlayerPrefs.GetInt("bestScore1") && totalScore < PlayerPrefs.GetInt("bestScore2") && totalScore > PlayerPrefs.GetInt("bestScore3"))
        {
            PlayerPrefs.SetInt("bestScore3", totalScore);
            arrInt[2] = totalScore;
        }
        BestScore.text = string.Format(" 1 - {0} \n 2 - {1} \n 3 - {2}", arrInt[0],arrInt[1],arrInt[2]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
