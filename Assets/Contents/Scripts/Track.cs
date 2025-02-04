﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class Track: MonoBehaviour
{
    public string ID;

    public float zSize;

    [HideInInspector]
    public bool positioned;


    public float spawnStartPos;
    public float spawnEndPos;

    public bool CanSpawnAtEveryWhere;
    public bool CanSpawnAtMaximum;

    public bool injectedTrack;

    private void Awake()
    {

        if (ID.Equals(""))
        {
            ID = this.gameObject.name.Replace("(Clone)", "");
        }

    }

    public void init()
    {

        if (CanSpawnAtMaximum == true)
        {
            spawnEndPos = 9000000;
        }
    
    }
    private void Update()
    {
        if (GameObject.Find("Player").transform.position.z > gameObject.transform.position.z + zSize + 150.0f)
        {
            Destroy(gameObject);
        }
       
    }

}
