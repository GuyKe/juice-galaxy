using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Something a hand can pick up and hold via HandGrabber while its trigger is held.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        public bool isHeld { get; private set; }

        Rigidbody _rb;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Grab(Transform hand, GameObject holder)
        {
            isHeld = true;
            _rb.isKinematic = true;
            _rb.useGravity = false;
            transform.SetParent(hand, true);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            var hitbox = GetComponentInChildren<MomentumMeleeHitbox>();
            if (hitbox != null) hitbox.owner = holder;
        }

        public void Release(Vector3 velocity)
        {
            isHeld = false;
            transform.SetParent(null, true);
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.velocity = velocity;
        }
    }
}
