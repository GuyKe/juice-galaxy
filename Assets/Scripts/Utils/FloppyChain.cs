using UnityEngine;
using UnityEngine.InputSystem;

namespace JuiceGalaxy
{
    /// <summary>
    /// A physics-driven wobbly chain of spring-jointed segments - the building block for every
    /// "floppy" character in the game (player limbs, Ingot's arms).
    /// A kinematic anchor is dragged along by a driver transform each frame and every segment
    /// behind it springs along for the ride, producing the elastic, jiggly look from the brief.
    /// </summary>
    public class FloppyChain : MonoBehaviour
    {
        public Transform driver;
        public Rigidbody anchor;
        public Rigidbody[] segments;
        public Transform[] visuals;
        public SpringJoint[] joints;

        // A fast hand swing (momentum melee relies on exactly this) can move the driver several
        // tens of centimeters in a single physics step. Teleporting the anchor straight there made
        // the SpringJoints see a huge sudden stretch and fire back a violent corrective force,
        // flinging segments into glitchy tangled poses. Capping how far the anchor can move per
        // step - and clamping segment speed as a second safety net - keeps that bounded.
        public float maxAnchorSpeed = 14f;
        public float maxSegmentSpeed = 12f;

        // Optional: an <XRController>/isTracked action for this chain's driver hand. Quest's
        // inside-out controller tracking can briefly lose a hand that swings out of the headset's
        // camera view, at which point the driver transform just holds its last known pose. Without
        // this the limb reads as rigidly "frozen" onto that stale point; with it, the joints go
        // slack so the limb sags under gravity instead until tracking resumes.
        public InputAction trackedAction;
        float _baseSpring, _baseDamper;
        bool _relaxed;

        public static FloppyChain Build(Transform parent, string name, Transform driver, int segmentCount,
            float segmentLength, float startRadius, float endRadius, Material material,
            float spring = 1100f, float damper = 14f, float segmentMass = 0.4f, bool blocky = false)
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
            chain.joints = new SpringJoint[segmentCount];
            chain._baseSpring = spring;
            chain._baseDamper = damper;

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

                Transform visual = blocky
                    ? BuildBlockyVisual(segGo.transform, radius, material)
                    : BuildRoundVisual(segGo.transform, radius, material);
                chain.visuals[i] = visual;

                var collider = segGo.AddComponent<SphereCollider>();
                collider.radius = radius;

                var rb = segGo.AddComponent<Rigidbody>();
                rb.mass = segmentMass;
                rb.drag = 1.3f;
                rb.angularDrag = 3.5f;
                // The anchor is kinematic and moved via MovePosition, which doesn't reliably wake a
                // sleeping connected body through its SpringJoint - segments that had gone still for
                // a moment could then just sit there "frozen" even as the hand kept moving. Disabling
                // sleep on them entirely (see also the explicit WakeUp() below) keeps them responsive.
                rb.sleepThreshold = 0f;
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
                chain.joints[i] = joint;

                previous = rb;
            }

            return chain;
        }

        static Transform BuildRoundVisual(Transform segment, float radius, Material material)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(segment, false);
            var mf = visual.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMesh.CreateFlatShadedIcosphere(radius, 1);
            var mr = visual.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return visual.transform;
        }

        static Transform BuildBlockyVisual(Transform segment, float radius, Material material)
        {
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(segment, false);
            visual.transform.localScale = Vector3.one * radius * 1.8f;
            // A little random tumble per segment so a chain of cubes reads as chunky and jointed
            // rather than one obviously repeated block.
            visual.transform.localRotation = Random.rotation;
            Object.Destroy(visual.GetComponent<Collider>());
            var mr = visual.GetComponent<MeshRenderer>();
            mr.sharedMaterial = material;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return visual.transform;
        }

        void FixedUpdate()
        {
            if (driver == null || anchor == null) return;

            bool tracked = trackedAction == null || trackedAction.ReadValue<float>() > 0.5f;
            if (tracked == _relaxed && joints != null)
            {
                // First frame tracking flips state: slack the joints so the limb sags under gravity
                // instead of holding a rigid pose on a stale tracked position, then restore them the
                // moment tracking comes back.
                _relaxed = !tracked;
                foreach (var joint in joints)
                {
                    if (joint == null) continue;
                    joint.spring = tracked ? _baseSpring : 0f;
                    joint.damper = tracked ? _baseDamper : 2f;
                }
            }

            if (tracked)
            {
                Vector3 delta = driver.position - anchor.position;
                float maxStep = maxAnchorSpeed * Time.fixedDeltaTime;
                if (delta.magnitude > maxStep) delta = delta.normalized * maxStep;
                anchor.MovePosition(anchor.position + delta);
                anchor.MoveRotation(driver.rotation);
            }

            if (segments == null) return;
            float maxSqr = maxSegmentSpeed * maxSegmentSpeed;
            Vector3 lastGoodPos = anchor.position;
            foreach (var segment in segments)
            {
                if (segment == null) continue;
                segment.WakeUp();

                // A stiff spring joint can occasionally hand back a NaN/huge velocity from a single
                // solver step (e.g. a very fast hand swing overstretching it); left alone that
                // corrupts the Rigidbody's position permanently, and the limb reads as "frozen"
                // forever after since nothing else ever touches its transform again. Snap it back
                // onto the chain instead of leaving it broken.
                if (!IsFinite(segment.velocity) || !IsFinite(segment.transform.position))
                {
                    segment.velocity = Vector3.zero;
                    segment.angularVelocity = Vector3.zero;
                    segment.position = lastGoodPos;
                }
                else if (segment.velocity.sqrMagnitude > maxSqr)
                {
                    segment.velocity = segment.velocity.normalized * maxSegmentSpeed;
                }

                lastGoodPos = segment.position;
            }
        }

        static bool IsFinite(Vector3 v)
        {
            return !float.IsNaN(v.x) && !float.IsInfinity(v.x)
                && !float.IsNaN(v.y) && !float.IsInfinity(v.y)
                && !float.IsNaN(v.z) && !float.IsInfinity(v.z);
        }

        public Vector3 TipVelocity => segments != null && segments.Length > 0 ? segments[segments.Length - 1].velocity : Vector3.zero;
        public Transform TipTransform => visuals != null && visuals.Length > 0 ? visuals[visuals.Length - 1] : null;
    }
}
