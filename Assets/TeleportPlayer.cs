using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour {

    GameObject from, to;
	// Use this for initialization
	void Start () {
        from = transform.Find("From").gameObject;
        to = transform.Find("To").gameObject;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Collision");
        GameObject.Find("RigidBodyFPSController").transform.position = to.transform.position;
    }
}
