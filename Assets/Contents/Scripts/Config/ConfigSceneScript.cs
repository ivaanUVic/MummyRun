using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfigSceneScript : MonoBehaviour
{
    Animator PlayerAnimator;
    GameObject Player;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        PlayerAnimator = Player.GetComponentInChildren<Animator>();
        PlayerAnimator.Play("lookAround", 0, 0);
        ChangeScene(false, "MainScene", 1.5f);
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
        ChangeScene(true, "IntroScene", 1.5f);
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

        yield return new WaitForSeconds(duration);
        if (fadein)
        {
            SceneManager.LoadScene(scene);
        }

    }
}
