using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// Put on a heavy rigidbody (a crate) so it hurts things it lands on hard enough - the
    /// "stacks of crates that give juice but can crush you" hazard from the design brief.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CrushHazard : MonoBehaviour
    {
        public float minImpactSpeed = 3.5f;
        public float damagePerSpeed = 8f;
        public float maxDamage = 45f;

        Rigidbody _rb;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        void OnCollisionEnter(Collision collision)
        {
            float speed = collision.relativeVelocity.magnitude;
            if (speed < minImpactSpeed) return;

            var health = collision.collider.GetComponentInParent<Health>();
            if (health == null || health.isDead) return;

            // Only counts as a crush if the crate is meaningfully above/on top of the victim.
            Vector3 toVictim = (collision.collider.transform.position - transform.position);
            if (Vector3.Dot(toVictim.normalized, Vector3.up) > 0.2f) return;

            float damage = Mathf.Min(maxDamage, speed * damagePerSpeed);
            health.TakeDamage(damage, collision.GetContact(0).point);
        }
    }
}
