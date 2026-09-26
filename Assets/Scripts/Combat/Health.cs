using System;
using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Generic damageable component used by the player, NPCs and crates.</summary>
    public class Health : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float current { get; private set; }
        public bool isDead { get; private set; }

        /// <summary>How much this thing shakes/knocks back when hit, purely cosmetic feedback.</summary>
        public float hitShakeStrength = 0.15f;

        public event Action<float, Vector3> OnDamaged; // amount, hitPoint
        public event Action OnDeath;

        void Awake()
        {
            current = maxHealth;
        }

        public void TakeDamage(float amount, Vector3 hitPoint = default)
        {
            if (isDead || amount <= 0f) return;
            current = Mathf.Max(0f, current - amount);
            OnDamaged?.Invoke(amount, hitPoint);
            if (current <= 0f) Die();
        }

        public void Heal(float amount)
        {
            if (isDead) return;
            current = Mathf.Min(maxHealth, current + amount);
        }

        public void Die()
        {
            if (isDead) return;
            isDead = true;
            OnDeath?.Invoke();
        }

        public float Percent01 => maxHealth <= 0f ? 0f : current / maxHealth;
    }
}
