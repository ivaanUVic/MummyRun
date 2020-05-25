using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroScene : MonoBehaviour
{
    Animator PlayerAnimator;
    GameObject Player;
    public Image Fade;
    public Text TotalCoins;
    public Text TotalPlus;
    // Start is called before the first frame update
    void Start()
    {
        Fade.gameObject.SetActive(false);
        Player = GameObject.FindWithTag("Player");
        PlayerAnimator = Player.GetComponentInChildren<Animator>();
        PlayerAnimator.Play("lookAround", 0, 0);
        Fade.canvasRenderer.SetAlpha(0.8f);
        ChangeScene(false, "MainScene", 1.5f);
        TotalCoins.text = PlayerPrefs.GetInt("totalCoins", 0).ToString();
        TotalPlus.text = PlayerPrefs.GetInt("totalScore", 0).ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void StartGame()
    {
        ChangeScene(true, "MainScene", 1.5f);
    }
    public void CloseGame()
    { 
        Application.Quit();
    }

    public void ConfigGame()
    {
        ChangeScene(true, "ConfigScene", 1.5f);
    }

    public void IntroGame()
    {
        ChangeScene(true,"IntroScene",1.5f);
    }
    private void ChangeScene(bool fadeIn, String scene, float duration = 3f)
    {
        if (fadeIn)
        {
            StartCoroutine(FadeCoroutine(duration, 0f, 1f, fadeIn, scene));
        }
        else
        {
            StartCoroutine(FadeCoroutine(duration, 0.8f, 0f, fadeIn, scene));
        }

    }

    public IEnumerator FadeCoroutine(float duration, float init, float final, bool fadein, String scene)
    {

        Fade.gameObject.SetActive(true);
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
}
