using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour {

    Rigidbody rb;
    public float forceX, forceY, forceZ;
    private bool isColliding;
    // Use this for initialization
    void Start () {
        rb = GameObject.Find("RigidBodyFPSController").GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isColliding)
            return;

        isColliding = true;
        /*
        Vector3 tempVel = rb.velocity;
        if (tempVel.y < 0)
            tempVel.y = 0;

        tempVel.x += forceX;
        tempVel.y += forceY;
        tempVel.z += forceZ;

        rb.velocity = tempVel;*/
        //rb.velocity = Vector3.zero;
        //rb.velocity = new Vector3(forceX, forceY, forceZ);
        //rb.AddForce(new Vector3(forceX, forceY, forceZ), ForceMode.Impulse);
    }

    private void OnTriggerExit(Collider other)
    {
        isColliding = false;
    }

    private void FixedUpdate()
    {
        if (!isColliding)
            return;
        else
        {
            Vector3 tempVel = rb.velocity;
            if (tempVel.y < 0)
                tempVel.y = 0;

            tempVel.x += forceX;
            tempVel.y += forceY;
            tempVel.z += forceZ;

            rb.velocity = tempVel;
            //Debug.Log("Velocity added, speed: " + rb.velocity.magnitude);
        }
    }

}
