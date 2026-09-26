using UnityEngine;
using UnityEngine.InputSystem;

namespace JuiceGalaxy
{
    /// <summary>
    /// Lets a tracked hand grab and hold a nearby Grabbable while its trigger is held, and throws
    /// it with the hand's current velocity on release.
    /// </summary>
    public class HandGrabber : MonoBehaviour
    {
        public Transform hand;
        public InputAction triggerAction;
        public GameObject owner;
        public float grabRadius = 0.28f;
        public float releaseThreshold = 0.35f;

        Grabbable _held;
        Vector3 _lastPos;
        Vector3 _velocity;

        public void Init(Transform handTransform, InputAction trigger, GameObject holder)
        {
            hand = handTransform;
            triggerAction = trigger;
            owner = holder;
            _lastPos = hand.position;
        }

        void Update()
        {
            if (hand == null) return;

            _velocity = Time.deltaTime > 0f ? (hand.position - _lastPos) / Time.deltaTime : Vector3.zero;
            _lastPos = hand.position;

            bool triggerHeld = triggerAction != null && triggerAction.ReadValue<float>() > releaseThreshold;

            if (triggerHeld)
            {
                if (_held == null) TryGrabNearby();
            }
            else if (_held != null)
            {
                _held.Release(_velocity);
                _held = null;
            }
        }

        void TryGrabNearby()
        {
            var hits = Physics.OverlapSphere(hand.position, grabRadius);
            foreach (var hit in hits)
            {
                var g = hit.GetComponentInParent<Grabbable>();
                if (g != null && !g.isHeld)
                {
                    g.Grab(hand, owner);
                    _held = g;
                    return;
                }
            }
        }

        void OnDisable()
        {
            if (_held != null)
            {
                _held.Release(Vector3.zero);
                _held = null;
            }
        }
    }
}
