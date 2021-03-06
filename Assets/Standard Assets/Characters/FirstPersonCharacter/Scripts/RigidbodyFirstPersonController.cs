using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (Rigidbody))]
    [RequireComponent(typeof (BoxCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour
    {
        [Serializable]
        public class MovementSettings
        {
            //public float ForwardSpeed = 8.0f;   // Speed when walking forward
            //public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            //public float StrafeSpeed = 4.0f;    // Speed when walking sideways
            //public float RunMultiplier = 2.0f;   // Speed when sprinting
            
            //public KeyCode RunKey = KeyCode.LeftShift;
            public float JumpForce = 32f;
            //public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            //[HideInInspector] public float CurrentTargetSpeed = 8f;
/*
#if !MOBILE_INPUT
            private bool m_Running;
#endif*/

            /*public void UpdateDesiredTargetSpeed(Vector2 input)
            {
	            if (input == Vector2.zero) return;
				if (input.x > 0 || input.x < 0)
				{
					//strafe
					CurrentTargetSpeed = StrafeSpeed;
				}
				if (input.y < 0)
				{
					//backwards
					CurrentTargetSpeed = BackwardSpeed;
				}
				if (input.y > 0)
				{
					//forwards
					//handled last as if strafing and moving forward at the same time forwards speed should take precedence
					CurrentTargetSpeed = ForwardSpeed;
				}
#if !MOBILE_INPUT
	            if (Input.GetKey(RunKey))
	            {
		            CurrentTargetSpeed *= RunMultiplier;
		            m_Running = true;
	            }
	            else
	            {
		            m_Running = false;
	            }
#endif
            }*/

/*#if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
#endif*/
        }


        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            //public float stickToGroundHelperDistance = 0.5f; // stops the character
            //public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            //public bool airControl; // can the user control the direction that is being moved in the air
            //[Tooltip("set it to 0.1 or more if you get stuck in wall")]
            //public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)

            //Default values: 8, 50, 2.5, 600, 0.72
            public float friction; //The friction the character experiences when grounded
            public float ground_accelerate; //The acceleration a grounded character can have
            public float max_velocity_ground; //The max velocity a grounded character can achieve by walking
            public float air_accelerate; //The Acceleration a character in the air can have
            public float max_velocity_air; //The max velocity a grounded character can achieve in the air
        }


        public Camera cam;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();


        private Rigidbody m_RigidBody;
        private BoxCollider m_Collider;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping;
        public bool m_IsGrounded;

        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        /*public bool Running
        {
            get
            {
 #if !MOBILE_INPUT
				return movementSettings.Running;
#else
	            return false;
#endif
            }
        }*/


        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Collider = GetComponent<BoxCollider>();
            mouseLook.Init (transform, cam.transform);
        }


        private void Update()
        {
            RotateView();

            if (CrossPlatformInputManager.GetButton("Jump") && !m_Jump)
            {
                m_Jump = true;
            }
        }


        private void FixedUpdate()
        {
            GroundCheck();
            Vector2 input = GetInput();

            
            /*if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                desiredMove = cam.transform.forward*input.y + cam.transform.right*input.x;
                //desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                desiredMove = Vector3.ProjectOnPlane(desiredMove, transform.up).normalized;
                //desiredMove.x = desiredMove.x*movementSettings.CurrentTargetSpeed;
                //desiredMove.z = desiredMove.z*movementSettings.CurrentTargetSpeed;
                //desiredMove.y = desiredMove.y*movementSettings.CurrentTargetSpeed;
                

                if (m_RigidBody.velocity.sqrMagnitude <
                    (movementSettings.CurrentTargetSpeed*movementSettings.CurrentTargetSpeed))
                {
                    m_RigidBody.AddForce(desiredMove*SlopeMultiplier(), ForceMode.Impulse);
                }
            }*/
            Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, transform.up).normalized;
            
            if (m_IsGrounded)
            {
                

                //m_RigidBody.drag = 5f;
                m_RigidBody.velocity = MoveGround(desiredMove, m_RigidBody.velocity);

                if (m_Jump)
                {
                    //m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                {
                    m_RigidBody.Sleep();
                }
            }
            else
            {
                
                m_RigidBody.drag = 0f;
                m_RigidBody.velocity = MoveAir(desiredMove, m_RigidBody.velocity);

                if (m_PreviouslyGrounded && !m_Jumping)
                {
                    //StickToGroundHelper();
                }
            }
            m_Jump = false;
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
                float drop = speed * advancedSettings.friction * Time.fixedDeltaTime;
                prevVelocity *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
            }
            
            // ground_accelerate and max_velocity_ground are server-defined movement variables
            return Accelerate(accelDir, prevVelocity, advancedSettings.ground_accelerate, advancedSettings.max_velocity_ground);
        }

        private Vector3 MoveAir(Vector3 accelDir, Vector3 prevVelocity)
        {
            // air_accelerate and max_velocity_air are server-defined movement variables
            return Accelerate(accelDir, prevVelocity, advancedSettings.air_accelerate, advancedSettings.max_velocity_air);
        }

        /*private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }*/


        /*private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height/2f) - m_Capsule.radius) +
                                   advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }*/


        private Vector2 GetInput()
        {
            
            Vector2 input = new Vector2
                {
                    x = CrossPlatformInputManager.GetAxis("Horizontal"),
                    y = CrossPlatformInputManager.GetAxis("Vertical")
                };
			//movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation (transform, cam.transform);

            if (m_IsGrounded/* || advancedSettings.airControl*/)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation*m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            /* m_PreviouslyGrounded = m_IsGrounded;
             RaycastHit hitInfo;
             if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                    ((m_Capsule.height/2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
             {
                 m_IsGrounded = true;
                 m_GroundContactNormal = hitInfo.normal;
             }
             else
             {
                 m_IsGrounded = false;
                 m_GroundContactNormal = Vector3.up;
             }
             if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
             {
                 m_Jumping = false;
             }*/

            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;

            if (Physics.BoxCast(transform.position, new Vector3(0.1595f, advancedSettings.groundCheckDistance, 0.1595f), Vector3.down, out hitInfo, transform.rotation, 0.36001f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                //m_IsGrounded = true;

                //Vector3 temp = transform.position;
                //temp.y -= 0.36f;
                Vector3 bottom = transform.position;
                bottom.y -= 0.36f;
                Debug.DrawLine(transform.position, bottom, Color.yellow);
                Debug.DrawRay(transform.position, hitInfo.normal, Color.blue);
                // Debug.DrawLine(transform.position, transform.position + hitInfo.normal, Color.blue); //show normal of surface coming out of character
                float angle = Vector3.Angle(transform.up, hitInfo.normal);
                
                if (angle < 45) { 
                    m_IsGrounded = true;
                    m_GroundContactNormal = hitInfo.normal;
                    m_Collider.material.staticFriction = 0.2f;
                    m_Jumping = false;
                }
                else
                {
                    //Debug.Log("Angle of normal: " + angle);
                    m_IsGrounded = false;
                    m_GroundContactNormal = Vector3.up;
                    m_Collider.material.staticFriction = 0.0f;
                }

            }
            else {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
                m_Collider.material.staticFriction = 0.0f;
            }
                

            

            //if (Physics.Raycast(transform.position, -transform.up, 0.36f + 0.1f))
                //m_IsGrounded = true;
            //else
                //m_IsGrounded = false;
        }
    }
}
