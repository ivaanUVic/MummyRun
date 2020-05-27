using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class GameGlobals : MonoBehaviour
{
    public static bool isInCriticalState = false;

    public float criticalStateSeconds = 5f;
    public Image DamageImage;
    public Image Fade;
    public List<AudioClip> Ambience;
    private AudioSource ASrc;
    public GameObject AudioEffects;
    public GameObject Player;
    private PlayerControllerScript PlayerController;
    private static GameGlobals instance = null;
    public bool gameOver = false;
    public Text Coins;
    public Text PlusText;
    [HideInInspector]
    public int points = 0;
    [HideInInspector]
    public int plus = 0;
    public AudioClip PowerUp;
    public static TouchController touchController;
    [HideInInspector]
    private Achievements hudScore;
    private bool Faded;
    private Coroutine mainMenuCoroutine;
    public GameObject Camera;
    //Instancia per accedir desde altres classes
    public static GameGlobals Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        //Player = GameObject.FindWithTag("Player");
        //print(Player.name);
        hudScore = this.gameObject.GetComponent<Achievements>();
        PlayerController = Player.GetComponent<PlayerControllerScript>();
        touchController = this.gameObject.GetComponent<TouchController>();
        DamageImage.gameObject.SetActive(false);
        DamageImage.canvasRenderer.SetAlpha(0.0f);
        ASrc = gameObject.GetComponent<AudioSource>();
        isInCriticalState = false;
        points = 0;
        plus = 0;
        instance = this;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeScene(false, "IntroScene");
    }


    public void PlayPowerUp()
    {
        AudioSource audioSrc = Player.GetComponentInChildren<AudioSource>();
        audioSrc.clip = PowerUp;
        audioSrc.Play();
    }


    public void goToMainMenu(bool force = false)
    {
        if (force)
        {
            StopAllCoroutines();
            ChangeScene(true, "IntroScene",1f);
        }
        else
        {
            //Si no apretem cap boto, en 10s retorna sol al menu principal
            mainMenuCoroutine = StartCoroutine(CoroutineGoToMainMenu(10f));

        }
        print("IntroScene");

    }

    public IEnumerator CoroutineGoToMainMenu(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Fade.gameObject.SetActive(true);
        ChangeScene(true, "IntroScene");

    }

    public void Close()
    {
        Application.Quit();
    }

    public void Restart()
    {
        StopAllCoroutines();
        //print("restart");
        ChangeScene(true, "MainScene",1f);
    }

    //Fa que quan canviem de scena, apareixi un fade in/ fade out negre
    private void ChangeScene(bool fadeIn,String scene,float duration = 3f)
    {
        if (fadeIn)
        {
            StartCoroutine(FadeCoroutine(duration, 0f,1f,fadeIn, scene));
        }
        else
        {
            StartCoroutine(FadeCoroutine(1f, 2f, 0f, fadeIn, scene));
        }

    }

    public IEnumerator FadeCoroutine(float duration,float init, float final,bool fadein, String scene )
    {
        //Fade.gameObject.SetActive(true); //TODO:ELIMINAR
        Fade.canvasRenderer.SetAlpha(init);
        Fade.CrossFadeAlpha(final, duration, false);
        yield return new WaitForSeconds(duration);
        if (fadein)
        {
            SceneManager.LoadScene(scene);
        }
        else
        {
            Fade.gameObject.SetActive(false);
        }

    }

   
    public void IncresePoints(int val,bool isPlus = false)
    {
        if (!isPlus)
        {
            points += val;
            Coins.text = points.ToString();
        }else
        {
            plus += val;
            PlusText.text = plus.ToString();

        }

    }
   

    public IEnumerator CriticalState(float waitTime)
    {
        DamageImage.gameObject.SetActive(true);
        isInCriticalState = true;
        //Color tempColor = DamageImage.color;
        DamageImage.canvasRenderer.SetAlpha(0.0f);
        DamageImage.CrossFadeAlpha(0.5f, 0.5f, false);
        SwipeCriticalAudio();

        //tempColor.a = 0.5f;
        //DamageImage.color = tempColor;
        yield return new WaitForSeconds(waitTime);
        if (!gameOver)
        {
            isInCriticalState = false;
            SwipeNormalAudio();
            DamageImage.CrossFadeAlpha(0, 1, false);
            //DamageImage.gameObject.SetActive(false);

        }


    }

    private void SwipeCriticalAudio()
    {
        if (!gameOver)
        { 
            ASrc.clip = Ambience[1];
            ASrc.Play();
        }
       
    }

    private void SwipeNormalAudio()
    {
        if (!gameOver)
        {
            ASrc.clip = Ambience[0];
            ASrc.Play();
        }
    }

    public void GameOver()
    {
        //Restart();//TODO:DELETE
        //StartCoroutine(DoGameOver(2f));

        gameOver = true;
        hudScore.saveScores();

    }

    private IEnumerator DoGameOver(float waitTime)
    {
        gameOver = true;
        ASrc.clip = Ambience[2];
        ASrc.Play();

        yield return new WaitForSeconds(waitTime);

        hudScore.saveScores();

    }

    public void PlaySound(AudioClip ac)
    {
        AudioSource AS = AudioEffects.GetComponent<AudioSource>();
        AS.clip = ac;
        AS.Play();
    }

    public void PlayerPlaySound(string name)
    {
        PlayerController.PlaySound(name);
    }


}
