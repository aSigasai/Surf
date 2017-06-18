using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
//[RequireComponent(typeof(ParticleSystem))]
public class Thruster : MonoBehaviour {

    
    Rigidbody rb;
    public BoxCollider bc;
    public ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    public Bounds bounds;

    public float forceX, forceY, forceZ;
    
    private bool isColliding;
    // Use this for initialization
    void Start () {
        rb = GameObject.Find("RigidBodyFPSController").GetComponent<Rigidbody>();
        //bc = GetComponent<BoxCollider>();
        //ps = GetComponent<ParticleSystem>();

       
        //ps.transform.rotation = Quaternion.LookRotation(new Vector3(forceX, forceY, forceZ));
        //ps.transform.localPosition -= new Vector3(0, bc.bounds.extents.y, 0);
        bounds.center = transform.position;
        bounds.extents = new Vector3(bc.bounds.extents.x, bc.bounds.extents.y, bc.bounds.extents.z);
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    private void Awake()
    {
        //-26.76, 2.25, -26.25
        //y 0.3f

        //-26.76, 5, -26.25
        //x -0.2f
        var ma = ps.main;
        ma.startSpeed = 0f;
        float temp = Mathf.Max(bc.bounds.extents.x * 2, bc.bounds.extents.y * 2, bc.bounds.extents.z * 2);
        ma.startLifetime = temp / new Vector3(forceX, forceY, forceZ).magnitude; //lifetime = to length of bounding box

        ma.startSize = 0.4f;

        var sh = ps.shape;
        sh.enabled = true;
        sh.shapeType = ParticleSystemShapeType.Box;
        sh.box = new Vector3(bc.bounds.extents.x*2, bc.bounds.extents.y * 2, bc.bounds.extents.z * 2); //bc.bounds.extents.y*2

        var em = ps.emission;

        var vol = ps.velocityOverLifetime;
        vol.enabled = true;
        vol.x = forceX;
        vol.y = forceY;
        vol.z = forceZ;
        //em.rateOverTime = new Vector3(forceX, forceY, forceZ).magnitude;
    }

    private void Update()
    {
        int count = ps.GetParticles(particles);
        for(int i = 0; i < count; i++)
        {
            if(!bounds.Contains(particles[i].position))
            {
                particles[i].remainingLifetime = -1.0f;
            }
        }

        ps.SetParticles(particles, count);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isColliding)
            return;

        isColliding = true;        
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
