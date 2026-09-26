using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// VR locomotion for the player: thumbstick move relative to head yaw, snap-turn comfort
    /// turning, gravity/ground handling, and a CharacterController height that follows the
    /// headset so crouching in real life crouches in-game. Hands off vertical motion to
    /// PlayerFlight while flying.
    ///
    /// Also carries a decaying knockback impulse (see ApplyImpulse) so hits, crushes and your own
    /// swings can physically shove the body around - the closest a VR CharacterController can
    /// safely get to an "active ragdoll" feel without ever touching head tracking, which has to
    /// stay purely HMD-driven for comfort.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FloppyPlayerController : MonoBehaviour
    {
        public XRInputRig rig;
        public float moveSpeed = 3.8f;
        public float gravity = -9.81f;
        public float snapTurnDegrees = 45f;
        public float snapTurnCooldown = 0.35f;
        public float minHeight = 0.5f;
        public float maxHeight = 2.2f;

        // How fast a knockback impulse fades - high enough that a shove reads as a brief jolt
        // rather than a sustained, comfort-wrecking fling.
        public float externalVelocityDrag = 7f;

        [HideInInspector] public bool isFlying;
        [HideInInspector] public float verticalVelocity;
        [HideInInspector] public Vector3 externalVelocity;

        CharacterController _controller;
        float _lastSnapTurnTime;

        /// <summary>Shoves the body with a one-off velocity that decays over the next moment or two.</summary>
        public void ApplyImpulse(Vector3 impulse)
        {
            externalVelocity += impulse;
        }

        public void Init(XRInputRig inputRig)
        {
            rig = inputRig;
        }

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (rig == null) return;

            SyncHeightToHead();
            HandleSnapTurn();
            HandleLocomotion();
        }

        void SyncHeightToHead()
        {
            float headLocalY = Mathf.Clamp(rig.head.localPosition.y, minHeight, maxHeight);
            _controller.height = headLocalY;
            _controller.center = new Vector3(rig.head.localPosition.x, headLocalY / 2f, rig.head.localPosition.z);
        }

        void HandleSnapTurn()
        {
            Vector2 turn = rig.turnAction.ReadValue<Vector2>();
            if (Time.time - _lastSnapTurnTime < snapTurnCooldown) return;

            if (turn.x > 0.6f)
            {
                transform.RotateAround(rig.head.position, Vector3.up, snapTurnDegrees);
                _lastSnapTurnTime = Time.time;
            }
            else if (turn.x < -0.6f)
            {
                transform.RotateAround(rig.head.position, Vector3.up, -snapTurnDegrees);
                _lastSnapTurnTime = Time.time;
            }
        }

        void HandleLocomotion()
        {
            Vector2 move = rig.moveAction.ReadValue<Vector2>();
            Vector3 headForward = Vector3.ProjectOnPlane(rig.head.forward, Vector3.up).normalized;
            Vector3 headRight = Vector3.ProjectOnPlane(rig.head.right, Vector3.up).normalized;
            Vector3 horizontal = (headForward * move.y + headRight * move.x) * moveSpeed;

            if (!isFlying)
            {
                if (_controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 motion = horizontal;
            motion.y = isFlying ? 0f : verticalVelocity;
            motion += externalVelocity;
            _controller.Move(motion * Time.deltaTime);

            externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, externalVelocityDrag * Time.deltaTime);
            if (externalVelocity.sqrMagnitude < 0.0025f) externalVelocity = Vector3.zero;
        }

        public void ApplyFlightMotion(Vector3 worldMotion)
        {
            _controller.Move(worldMotion);
        }
    }
}
