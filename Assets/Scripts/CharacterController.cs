using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    public bool isGrounded;
    public float friction, ground_accelerate, max_velocity_ground, air_accelerate, max_velocity_air;
    private float distToGround;
    Rigidbody rb;
	// Use this for initialization
	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;
    }
	
    void IsGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, distToGround + 0.1f);     
    }

    bool IsOnSlope()
    {
        RaycastHit rayHit;
        Ray ray = new Ray(transform.position, Vector3.down);
        if(Physics.Raycast(ray, out rayHit))
        {
            Vector3 slope = rayHit.normal;
            float angle = Vector3.Angle(transform.up, slope);
            if (angle < 5.0f)
                return false;
        }
        return true;
    }
	// Update is called once per frame
	void Update () {
        
    }

    private void FixedUpdate()
    {
        
        IsGrounded();
        float translation = Input.GetAxis("Vertical");
        float straffe = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown("escape"))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        //get normal of surface
        Vector3 castPos = new Vector3(transform.position.x, transform.position.y - distToGround, transform.position.z);
        RaycastHit hit;
        Ray ray = new Ray(castPos, -transform.up);
        if (Physics.Raycast(ray, out hit))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.green);
            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue);
        }

        Vector3 desiredMove = transform.forward * translation + transform.right * straffe;

        desiredMove = Vector3.ProjectOnPlane(desiredMove, transform.up).normalized;
        //desiredMove = Vector3.ProjectOnPlane(desiredMove, hit.normal).normalized;
        Debug.DrawLine(transform.position, transform.position + hit.normal, Color.yellow); //show normal of surface coming out of character


        Debug.DrawLine(transform.position, transform.position + desiredMove, Color.red); //show desired move cominng out of character


        if (isGrounded)
        {
            rb.velocity = MoveGround(desiredMove, rb.velocity);

            if (Input.GetKey(KeyCode.Space))
            {
                Vector3 jump = new Vector3(0, 6, 0);
                rb.velocity += jump;
                Debug.Log("Jump!");
            }
        }
        else
        {
            rb.velocity = MoveAir(desiredMove, rb.velocity);
        }
    }

    // accelDir: normalized direction that the player has requested to move (taking into account the movement keys and look direction)
    // prevVelocity: The current velocity of the player, before any additional calculations
    // accelerate: The server-defined player acceleration value
    // max_velocity: The server-defined maximum player velocity (this is not strictly adhered to due to strafejumping)
    private Vector3 Accelerate(Vector3 accelDir, Vector3 prevVelocity, float accelerate, float max_velocity)
    {
        //projVel + accelVel = wishspeed
        float projVel = Vector3.Dot(prevVelocity, accelDir); // Vector projection of Current velocity onto accelDir.
        float accelVel = accelerate * Time.fixedDeltaTime; // Accelerated velocity in direction of movment

        // If necessary, truncate the accelerated velocity so the vector projection does not exceed max_velocity
        if (projVel + accelVel > max_velocity)
            accelVel = max_velocity - projVel;

        if ((projVel + accelVel) < projVel)
            return prevVelocity;

        return prevVelocity + accelDir * accelVel;
    }

    

    private Vector3 MoveGround(Vector3 accelDir, Vector3 prevVelocity)
    {
        // Apply Friction
        float speed = prevVelocity.magnitude;
        if (speed != 0) // To avoid divide by zero errors
        {
            float drop = speed * friction * Time.fixedDeltaTime;
            prevVelocity *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
        }

        // ground_accelerate and max_velocity_ground are server-defined movement variables
        return Accelerate(accelDir, prevVelocity, ground_accelerate, max_velocity_ground);
    }

    private Vector3 MoveAir(Vector3 accelDir, Vector3 prevVelocity)
    {
        // air_accelerate and max_velocity_air are server-defined movement variables
        return Accelerate(accelDir, prevVelocity, air_accelerate, max_velocity_air);
    }
}
