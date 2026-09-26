using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// VR locomotion for the player: thumbstick move relative to head yaw, snap-turn comfort
    /// turning, gravity/ground handling, and a CharacterController height that follows the
    /// headset so crouching in real life crouches in-game. Hands off vertical motion to
    /// PlayerFlight while flying.
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

        [HideInInspector] public bool isFlying;
        [HideInInspector] public float verticalVelocity;

        CharacterController _controller;
        float _lastSnapTurnTime;

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
            _controller.Move(motion * Time.deltaTime);
        }

        public void ApplyFlightMotion(Vector3 worldMotion)
        {
            _controller.Move(worldMotion);
        }
    }
}
