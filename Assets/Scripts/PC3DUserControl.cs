using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace PC3D
{
    [RequireComponent(typeof(PC3DCharacter))]
    public class PC3DUserControl : MonoBehaviour
    {
        private PC3DCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
        private bool m_sliding;                 //true when the player is sliding
        public static bool preslide;        //true before the player grabs the wall
        public static Vector3 alignDir;         //return the vector perpendicular to the collider

        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<PC3DCharacter>();
        }


        private void Update()
        {
            if (CrossPlatformInputManager.GetButtonDown("Front")) preslide = true;
            if (CrossPlatformInputManager.GetButtonUp("Front")) preslide = false;
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
        }

        private void OnCollisionStay(Collision col)
        {
            if (col.gameObject.tag == "wall")
            {
                if (CrossPlatformInputManager.GetButton("Front")) m_sliding = true;
                else m_sliding = false;
            }
            alignDir = -col.contacts[0].normal;
            //alignDir = Vector3.Dot(transform.forward, col.contacts[0].normal)* col.contacts[0].normal;
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            // read inputs
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);
            bool dash = Input.GetButtonDown("Fire2");

            m_Cam = CameraController.camController.cam.transform;

            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v * m_CamForward + h * m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward + h * Vector3.right;
            }
#if !MOBILE_INPUT
            // walk speed multiplier
            if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 2;
#endif

            //if (!PC3DCharacter.m_IsGrounded) m_Move = new Vector3(0, 0, 0);
            //if (!PC3DCharacter.m_IsGrounded) airMov = true;

            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, m_Jump, dash, m_sliding, preslide, alignDir);
            m_Jump = false;
            m_sliding = false;
        }
    }
}