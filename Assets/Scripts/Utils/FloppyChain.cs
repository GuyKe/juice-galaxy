using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// A physics-driven wobbly chain of spring-jointed segments - the building block for every
    /// "floppy" character in the game (player limbs, Mrs. Slithers' body, Ingot's arms).
    /// A kinematic anchor is dragged along by a driver transform each frame and every segment
    /// behind it springs along for the ride, producing the elastic, jiggly look from the brief.
    /// </summary>
    public class FloppyChain : MonoBehaviour
    {
        public Transform driver;
        public Rigidbody anchor;
        public Rigidbody[] segments;
        public Transform[] visuals;

        public static FloppyChain Build(Transform parent, string name, Transform driver, int segmentCount,
            float segmentLength, float startRadius, float endRadius, Material material,
            float spring = 900f, float damper = 12f, float segmentMass = 0.4f)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var chain = root.AddComponent<FloppyChain>();
            chain.driver = driver;

            var anchorGo = new GameObject(name + "_Anchor");
            anchorGo.transform.SetParent(root.transform, false);
            anchorGo.transform.position = driver.position;
            var anchorRb = anchorGo.AddComponent<Rigidbody>();
            anchorRb.isKinematic = true;
            chain.anchor = anchorRb;

            chain.segments = new Rigidbody[segmentCount];
            chain.visuals = new Transform[segmentCount];

            Rigidbody previous = anchorRb;
            Vector3 spawnPos = driver.position;
            Vector3 down = Vector3.down; // segments hang/trail initially, physics takes over after

            for (int i = 0; i < segmentCount; i++)
            {
                float t = segmentCount <= 1 ? 0f : (float)i / (segmentCount - 1);
                float radius = Mathf.Lerp(startRadius, endRadius, t);

                spawnPos += down * segmentLength;
                var segGo = new GameObject($"{name}_Segment{i}");
                segGo.transform.SetParent(root.transform, false);
                segGo.transform.position = spawnPos;

                var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "Visual";
                visual.transform.SetParent(segGo.transform, false);
                visual.transform.localScale = Vector3.one * radius * 2f;
                Object.Destroy(visual.GetComponent<Collider>());
                var mr = visual.GetComponent<MeshRenderer>();
                mr.sharedMaterial = material;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                chain.visuals[i] = visual.transform;

                var collider = segGo.AddComponent<SphereCollider>();
                collider.radius = radius;

                var rb = segGo.AddComponent<Rigidbody>();
                rb.mass = segmentMass;
                rb.drag = 1.5f;
                rb.angularDrag = 4f;
                chain.segments[i] = rb;

                var joint = segGo.AddComponent<SpringJoint>();
                joint.connectedBody = previous;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector3.zero;
                joint.connectedAnchor = Vector3.zero;
                joint.spring = spring;
                joint.damper = damper;
                joint.minDistance = 0f;
                joint.maxDistance = segmentLength * 0.85f;
                joint.tolerance = 0.02f;

                previous = rb;
            }

            return chain;
        }

        void FixedUpdate()
        {
            if (driver == null || anchor == null) return;
            anchor.MovePosition(driver.position);
            anchor.MoveRotation(driver.rotation);
        }

        public Vector3 TipVelocity => segments != null && segments.Length > 0 ? segments[segments.Length - 1].velocity : Vector3.zero;
        public Transform TipTransform => visuals != null && visuals.Length > 0 ? visuals[visuals.Length - 1] : null;
    }
}
