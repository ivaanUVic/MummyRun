using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerScript : MonoBehaviour
{

    public AudioClip MoveRight;
    public AudioClip MoveLeft;
    public AudioClip HitGround;
    public AudioClip Jump;
    public AudioClip Death;
    public AudioClip run;
    public AudioClip roll;
    public AudioClip PlayerStumple;
    public List<AudioClip> ouch;

    public void PlaySound(string name)
    {
        AudioSource AC = GetComponent<AudioSource>();

        AC.loop = false;
        switch (name)
        {
            case "jump":
                AC.clip = Jump;
                break;
            case "left":
                AC.clip = MoveLeft;
                break;
            case "right":
                AC.clip = MoveRight;
                break;
            case "ground":
                AC.clip = HitGround;
                break;
            case "death":
                AC.clip = Death;
                break;
            case "run":
                AC.clip = run;
                AC.loop = true;
                break;
            case "ouch":
                AC.clip = ouch[Random.Range(0, ouch.Count)];
                break;
            case "roll":
                AC.clip = roll;
                break;
            case "PlayerStumple":
                AC.clip = PlayerStumple;
                break;
        }
        AC.Play();
    }
    public void StopPlaySound(){
        AudioSource AC = GetComponent<AudioSource>();
        AC.Stop();
    }
}
