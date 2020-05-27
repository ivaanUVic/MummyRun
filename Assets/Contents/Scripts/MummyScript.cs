using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MummyScript : MonoBehaviour
{
    public GameObject Player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var pos = Player.transform.position;
        gameObject.transform.position = new Vector3(pos.x, gameObject.transform.position.y, pos.z -20);
    }
    private void OnTriggerEnter(Collider collider)
    {
        collider.isTrigger = true;
    }
    private void OnTriggerExit(Collider other)
    {
        other.isTrigger = false;
    }
}
