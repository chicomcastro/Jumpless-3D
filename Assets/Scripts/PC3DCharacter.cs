using UnityEngine;
using System;
using UnityStandardAssets.CrossPlatformInput;

namespace PC3D
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class PC3DCharacter : MonoBehaviour
    {
        [Header("Standard Info")]
        [SerializeField]
        float m_MovingTurnSpeed = 360;
        [SerializeField] float m_StationaryTurnSpeed = 180;
        [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
        [SerializeField] float m_MoveSpeedMultiplier = 1f;
        [SerializeField] float m_AnimSpeedMultiplier = 1f;
        [SerializeField] float m_GroundCheckDistance = 0.1f;

        [Space(5)]
        [Header("Jump information")]
        [SerializeField]
        float m_JumpPower = 12f;
        [SerializeField] int jumpQuant = 2;
        [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 2f;
        [SerializeField] bool canFly = false;

        [Space(5)]
        [Header("Dash information")]
        [SerializeField]
        float dashStep = 0.1f;
        [SerializeField] float dashDistance = 10;
        [SerializeField] float dashCooldown = 0.76f;

        [Space(5)]
        [Header("Slide information")]
        [SerializeField]
        float sildeStep = 0.1f;
        [SerializeField] bool slideJump;
        [SerializeField] bool slideDown;

        int jumpPotential = 2;

        bool isDashing = false;
        bool isSliding = false;
        public const float maxDashTime = 1.0f;
        public const float maxSlideTime = 8.0f;
        float currentDashTime = maxDashTime;
        float currentSlideTime = 0;

        Rigidbody m_Rigidbody;
        Animator m_Animator;
        public static bool m_IsGrounded;
        float m_OrigGroundCheckDistance;
        const float k_Half = 0.5f;
        float m_TurnAmount;
        float m_ForwardAmount;
        Vector3 m_GroundNormal;
        float m_CapsuleHeight;
        Vector3 m_CapsuleCenter;
        CapsuleCollider m_Capsule;
        bool m_Crouching;
        bool spinning = false;      //bool activated with the wall jump
        public int jump_plane, jump_y;      //variables to control the wall jump force
        public static bool wj;

        void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            m_CapsuleHeight = m_Capsule.height;
            m_CapsuleCenter = m_Capsule.center;

            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            m_OrigGroundCheckDistance = m_GroundCheckDistance;
        }

        void spin()         //spin when wall jump
        {
            if (spinning)
            {
                Vector3 targetAngles = transform.eulerAngles + 180f * Vector3.up; // what the new angles should be
                transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, targetAngles, 2 * Time.deltaTime);
            }
        }

        void align(Vector3 par)         //align with wall when grab
        {
            //transform.eulerAngles = Vector3.Lerp(transform.forward, par, 0.2f * Time.deltaTime);
            Quaternion rot = Quaternion.FromToRotation(transform.forward, par);
            transform.rotation *= rot;
            //transform.eulerAngles = par;
        }

        void cancel()
        {
            spinning = false;
        }

        public void Move(Vector3 move, bool crouch, bool jump, bool dash, bool slide, bool preslide, Vector3 normal)
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            spin();
            wj = false;

            #region dash
            // See if we're supposed to dash
            if (dash && !isDashing)
            {
                currentDashTime = 0;
                isDashing = true;
            }

            // If yes, implement our translation
            if (currentDashTime < maxDashTime)
            {
                Vector3 moveDirection = Vector3.zero;
                moveDirection = transform.forward * dashDistance;

                transform.position += (moveDirection * Time.deltaTime * dashDistance);
                currentDashTime += dashStep;
                return;
            }
            // Wait a little bit to reach cooldown
            else if (currentDashTime - maxDashTime < dashCooldown)
            {
                currentDashTime += dashStep;
            }
            // Set we've finished dashing and can dashing again
            else
            {
                isDashing = false;
            }
            #endregion

            #region slide
            if (currentSlideTime >= maxSlideTime)
            {
                PC3DUserControl.preslide = false;
                preslide = false;
                currentSlideTime = 0;
                m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }

            if (preslide && slide && !m_IsGrounded)
            {
                jumpPotential = 0;
                m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                align(normal);
                currentSlideTime += sildeStep;
                m_Rigidbody.velocity = new Vector3(0, -currentSlideTime / 3, 0);
                //Vector3 dir = -jump_plane * transform.forward + new Vector3(0, jump_y, 0);
                Vector3 dir = -jump_plane * normal + new Vector3(0, jump_y, 0);
                if (CrossPlatformInputManager.GetButton("Jump"))
                {
                    jump = false;
                    m_Rigidbody.AddForce(dir);
                    m_IsGrounded = false;
                    spinning = true;
                    Invoke("cancel", 0.5f);
                    wj = true;
                }
            }
            else
            {
                m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                currentSlideTime = 0;
            }
            #endregion

            if (spinning) move = new Vector3(0, 0, 0);

            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired
            // direction.
            if (move.magnitude > 1f) move.Normalize();
            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, m_GroundNormal);
            m_TurnAmount = Mathf.Atan2(move.x, move.z);
            m_ForwardAmount = move.z;

            ApplyExtraTurnRotation();

            // control and velocity handling is different when grounded and airborne:
            if (m_IsGrounded)
            {
                HandleGroundedMovement(crouch, jump);
            }
            else
            {
                if (!slide && !spinning && CrossPlatformInputManager.GetButton("Front"))
                    m_Rigidbody.velocity = new Vector3(0, m_Rigidbody.velocity.y, 0) + 10 * new Vector3(transform.forward.x, 0, transform.forward.z);
                if (!slide && !spinning && CrossPlatformInputManager.GetButton("Back"))
                    m_Rigidbody.velocity = new Vector3(0, m_Rigidbody.velocity.y, 0) + 10 * new Vector3(transform.forward.x, 0, transform.forward.z);
                if (!slide && !spinning && CrossPlatformInputManager.GetButton("Right"))
                    m_Rigidbody.velocity = new Vector3(0, m_Rigidbody.velocity.y, 0) + 10 * new Vector3(transform.forward.x, 0, transform.forward.z);
                if (!slide && !spinning && CrossPlatformInputManager.GetButton("Left"))
                    m_Rigidbody.velocity = new Vector3(0, m_Rigidbody.velocity.y, 0) + 10 * new Vector3(transform.forward.x, 0, transform.forward.z);
                HandleAirborneMovement(jump);
            }

            ScaleCapsuleForCrouching(crouch);
            PreventStandingInLowHeadroom();

            // send input and other state parameters to the animator
            UpdateAnimator(move);
        }


        void ScaleCapsuleForCrouching(bool crouch)
        {
            if (m_IsGrounded && crouch)
            {
                if (m_Crouching) return;
                m_Capsule.height = m_Capsule.height / 2f;
                m_Capsule.center = m_Capsule.center / 2f;
                m_Crouching = true;
            }
            else
            {
                Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
                float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
                if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    m_Crouching = true;
                    return;
                }
                m_Capsule.height = m_CapsuleHeight;
                m_Capsule.center = m_CapsuleCenter;
                m_Crouching = false;
            }
        }

        void PreventStandingInLowHeadroom()
        {
            // prevent standing up in crouch-only zones
            if (!m_Crouching)
            {
                Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
                float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
                if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    m_Crouching = true;
                }
            }
        }

        void UpdateAnimator(Vector3 move)
        {
            // update the animator parameters
            m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
            m_Animator.SetBool("Crouch", m_Crouching);
            m_Animator.SetBool("OnGround", m_IsGrounded);
            if (!m_IsGrounded)
            {
                m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
            }

            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle =
                Mathf.Repeat(
                    m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
            if (m_IsGrounded)
            {
                m_Animator.SetFloat("JumpLeg", jumpLeg);
            }

            // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
            // which affects the movement speed because of the root motion.
            if (m_IsGrounded && move.magnitude > 0)
            {
                m_Animator.speed = m_AnimSpeedMultiplier;
            }
            else
            {
                // don't use that while airborne
                m_Animator.speed = 1;
            }
        }
        void HandleAirborneMovement(bool jump)
        {
            // apply extra gravity from multiplier:
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            m_Rigidbody.AddForce(extraGravityForce);

            m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;

            if (canFly)
                jumpPotential = int.MaxValue;

            // extra jump || flying
            if (jump && jumpPotential > 0)
            {
                // discount our (n-1)-ésimo jump from potential jumps
                jumpPotential--;

                // apply physics
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
                m_Animator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
            }
        }
        void HandleGroundedMovement(bool crouch, bool jump)
        {
            // check whether conditions are right to allow a jump:
            if (jump && !crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
            {
                // discount our first jump from potential jumps
                jumpPotential--;

                // jump!
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
                m_IsGrounded = false;
                m_Animator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
            }
        }

        void ApplyExtraTurnRotation()
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
            transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
        }


        public void OnAnimatorMove()
        {
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (m_IsGrounded && Time.deltaTime > 0)
            {
                Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

                // we preserve the existing y part of the current velocity.
                v.y = m_Rigidbody.velocity.y;
                m_Rigidbody.velocity = v;
            }
        }


        void CheckGroundStatus()
        {
            RaycastHit hitInfo;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
            // 0.1f is a small offset to start the ray from inside the character
            // it is also good to note that the transform position in the sample assets is at the base of the character
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
            {
                m_GroundNormal = hitInfo.normal;
                m_IsGrounded = true;
                m_Animator.applyRootMotion = true;

                // when we're on ground, jumpPotential is in full charge
                jumpPotential = jumpQuant;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundNormal = Vector3.up;
                m_Animator.applyRootMotion = false;
            }
        }
    }
}