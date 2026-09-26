using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// A small glowing orb of Juice that floats and bobs until the player gets close, then flies
    /// toward them and is collected - like a Minecraft XP orb.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class JuicePickup : MonoBehaviour
    {
        public float amount = 12f;
        public float bobSpeed = 2f;
        public float bobHeight = 0.1f;
        public float spinSpeed = 90f;

        public float magnetRange = 4f;
        public float magnetAcceleration = 30f;
        public float maxMagnetSpeed = 16f;
        public float collectDistance = 0.5f;

        Vector3 _basePos;
        bool _magnetized;
        float _magnetSpeed;

        void Start()
        {
            _basePos = transform.position;
            GetComponent<SphereCollider>().isTrigger = true;
        }

        void Update()
        {
            var player = GameManager.Instance != null ? GameManager.Instance.player : null;

            if (player != null)
            {
                Vector3 target = player.position + Vector3.up;
                float dist = Vector3.Distance(transform.position, target);
                if (!_magnetized && dist < magnetRange) _magnetized = true;

                if (_magnetized)
                {
                    _magnetSpeed = Mathf.Min(maxMagnetSpeed, _magnetSpeed + magnetAcceleration * Time.deltaTime);
                    transform.position = Vector3.MoveTowards(transform.position, target, _magnetSpeed * Time.deltaTime);
                    transform.Rotate(Vector3.up, spinSpeed * 3f * Time.deltaTime, Space.World);

                    if (dist <= collectDistance) Collect(player);
                    return;
                }
            }

            transform.position = _basePos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            var juice = other.GetComponentInParent<JuiceSystem>();
            if (juice == null) return;
            juice.AddJuice(amount);
            Destroy(gameObject);
        }

        void Collect(Transform player)
        {
            var juice = player.GetComponentInParent<JuiceSystem>();
            if (juice != null) juice.AddJuice(amount);
            Destroy(gameObject);
        }

        public static GameObject Spawn(Vector3 position, float amount)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "JuicePickup";
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.25f;
            Object.Destroy(go.GetComponent<Collider>());
            var col = go.AddComponent<SphereCollider>();
            col.radius = 0.5f;
            var mat = MaterialUtil.CreateUnlit(new Color(1f, 0.85f, 0.2f));
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var pickup = go.AddComponent<JuicePickup>();
            pickup.amount = amount;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.8f, 0.2f);
            light.range = 2.5f;
            light.intensity = 1.2f;
            return go;
        }
    }
}
