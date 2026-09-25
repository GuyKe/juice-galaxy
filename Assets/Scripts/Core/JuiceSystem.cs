using System;
using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// Tracks the player's core resource, Juice. Juice is both the health pool and the
    /// progression currency: crates, enemies and pickups feed it, combat and crushing drain it,
    /// and Ingot gates the flight ability behind it.
    /// </summary>
    public class JuiceSystem : MonoBehaviour
    {
        public float maxJuice = 100f;
        public float currentJuice = 60f;
        public bool flightUnlocked = false;

        public event Action<float, float> OnJuiceChanged; // current, max
        public event Action OnFlightUnlocked;
        public event Action OnJuiceDepleted;

        Health _health;

        void Awake()
        {
            _health = GetComponent<Health>();
            if (_health != null)
            {
                _health.OnDamaged += (amount, point) => Drain(amount);
            }
        }

        void Start()
        {
            OnJuiceChanged?.Invoke(currentJuice, maxJuice);
        }

        public void AddJuice(float amount)
        {
            if (amount <= 0f) return;
            currentJuice = Mathf.Min(maxJuice, currentJuice + amount);
            if (_health != null) _health.Heal(amount);
            OnJuiceChanged?.Invoke(currentJuice, maxJuice);
        }

        public void Drain(float amount)
        {
            if (amount <= 0f) return;
            currentJuice = Mathf.Max(0f, currentJuice - amount);
            OnJuiceChanged?.Invoke(currentJuice, maxJuice);
            if (currentJuice <= 0f) OnJuiceDepleted?.Invoke();
        }

        public void UnlockFlight()
        {
            if (flightUnlocked) return;
            flightUnlocked = true;
            OnFlightUnlocked?.Invoke();
        }
    }
}
