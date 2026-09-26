using System.Collections.Generic;
using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// Attach to a fist/head/tail collider. Tracks its own world-space velocity and turns fast
    /// swings into damage on whatever Health component it touches - the core "momentum melee" feel.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MomentumMeleeHitbox : MonoBehaviour
    {
        public GameObject owner;
        public float damagePerSpeed = 6f;
        public float minSpeedToHurt = 1.2f;
        public float maxDamage = 60f;
        public float hitCooldown = 0.35f;
        public float knockbackForce = 6f;
        // Newton's third law, VR-safe version: a solid swing should shove your own floppy body back
        // a little too, not just the thing you hit - part of what sells the reference game's
        // "active ragdoll" feel. Kept small since this only nudges the CharacterController, never
        // the camera.
        public float ownerRecoil = 1.2f;
        FloppyPlayerController _ownerController;

        Vector3 _lastPos;
        Vector3 _velocity;
        readonly Dictionary<Health, float> _lastHitTime = new Dictionary<Health, float>();

        void Start()
        {
            _lastPos = transform.position;
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        void FixedUpdate()
        {
            Vector3 pos = transform.position;
            _velocity = (pos - _lastPos) / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
            _lastPos = pos;
        }

        public Vector3 CurrentVelocity => _velocity;

        void OnTriggerEnter(Collider other)
        {
            TryHit(other);
        }

        void OnTriggerStay(Collider other)
        {
            TryHit(other);
        }

        void TryHit(Collider other)
        {
            if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform))) return;

            var health = other.GetComponentInParent<Health>();
            if (health == null || health.isDead) return;
            if (owner != null)
            {
                var ownerHealth = owner.GetComponentInParent<Health>();
                if (ownerHealth == health) return;
            }

            float speed = _velocity.magnitude;
            if (speed < minSpeedToHurt) return;

            if (_lastHitTime.TryGetValue(health, out float last) && Time.time - last < hitCooldown) return;
            _lastHitTime[health] = Time.time;

            float damage = Mathf.Min(maxDamage, speed * damagePerSpeed);
            health.TakeDamage(damage, transform.position);

            var rb = other.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForceAtPosition(_velocity.normalized * knockbackForce * Mathf.Clamp01(speed / 6f), transform.position, ForceMode.Impulse);
            }

            if (_ownerController == null && owner != null) _ownerController = owner.GetComponent<FloppyPlayerController>();
            if (_ownerController != null)
            {
                _ownerController.ApplyImpulse(-_velocity.normalized * ownerRecoil * Mathf.Clamp01(speed / 6f));
            }
        }
    }
}
