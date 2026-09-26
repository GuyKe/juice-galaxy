using System.Collections.Generic;
using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>A floating spiked mine that hurts anything that touches it, regardless of speed.</summary>
    public class SpikeHazard : MonoBehaviour
    {
        public float damage = 18f;
        public float damageCooldown = 0.6f;
        public float bobSpeed = 1.4f;
        public float bobHeight = 0.15f;
        public float spinSpeed = 25f;

        Vector3 _basePos;
        readonly Dictionary<Health, float> _lastHit = new Dictionary<Health, float>();

        void Start()
        {
            _basePos = transform.position;
        }

        void Update()
        {
            transform.position = _basePos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        }

        void OnTriggerEnter(Collider other) => TryHurt(other);
        void OnTriggerStay(Collider other) => TryHurt(other);

        void TryHurt(Collider other)
        {
            var health = other.GetComponentInParent<Health>();
            if (health == null || health.isDead) return;
            if (_lastHit.TryGetValue(health, out float last) && Time.time - last < damageCooldown) return;
            _lastHit[health] = Time.time;
            health.TakeDamage(damage, transform.position);
        }

        public static GameObject Spawn(Transform parent, Vector3 position, float coreRadius = 0.35f)
        {
            var root = new GameObject("SpikeHazard");
            root.transform.SetParent(parent, true);
            root.transform.position = position;

            var darkMat = MaterialUtil.CreateLit(new Color(0.08f, 0.06f, 0.07f));
            var spikeMat = MaterialUtil.CreateLit(new Color(0.85f, 0.18f, 0.12f));

            var core = new GameObject("Core");
            core.transform.SetParent(root.transform, false);
            var mf = core.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMesh.CreateFlatShadedIcosphere(coreRadius, 1);
            core.AddComponent<MeshRenderer>().sharedMaterial = darkMat;

            const int spikeCount = 10;
            for (int i = 0; i < spikeCount; i++)
            {
                Vector3 dir = Random.onUnitSphere;
                var spike = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spike.name = "Spike";
                spike.transform.SetParent(root.transform, false);
                Object.Destroy(spike.GetComponent<Collider>());
                spike.transform.localPosition = dir * (coreRadius + 0.12f);
                spike.transform.localRotation = Quaternion.LookRotation(dir);
                spike.transform.localScale = new Vector3(0.14f, 0.14f, 0.5f);
                spike.GetComponent<MeshRenderer>().sharedMaterial = spikeMat;
            }

            var trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = coreRadius * 1.6f;

            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            root.AddComponent<SpikeHazard>();
            return root;
        }
    }
}
