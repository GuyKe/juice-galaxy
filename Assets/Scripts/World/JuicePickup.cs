using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>A small glowing orb of Juice that floats, bobs, and refills the player's resource on touch.</summary>
    [RequireComponent(typeof(SphereCollider))]
    public class JuicePickup : MonoBehaviour
    {
        public float amount = 12f;
        public float bobSpeed = 2f;
        public float bobHeight = 0.1f;
        public float spinSpeed = 90f;

        Vector3 _basePos;

        void Start()
        {
            _basePos = transform.position;
            GetComponent<SphereCollider>().isTrigger = true;
        }

        void Update()
        {
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
