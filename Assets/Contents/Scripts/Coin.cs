using System;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{


    [HideInInspector]
    public float audioPitch;
    public AudioClip PickUpAudio;
    public int Value = 1;
    public bool Plus = false;


    public void pickUp()
    {
        //When we pick up a coin we reproduce a soun ad well as we increase ui value
        GameGlobals.Instance.IncresePoints(Value,Plus);
        GameGlobals.Instance.PlaySound(PickUpAudio);
        gameObject.SetActive(false);
    }

}
