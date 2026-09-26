using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// A spiked morningstar resting on the school roof. Hold a controller trigger near it to grab
    /// it (see HandGrabber/Grabbable) and swing it as a bigger, deadlier melee weapon than a bare fist.
    /// </summary>
    public static class MorningstarProp
    {
        public static GameObject Spawn(Transform parent, Vector3 position)
        {
            var root = new GameObject("Morningstar");
            root.transform.SetParent(parent, true);
            root.transform.position = position;

            var handleMat = MaterialUtil.CreateLit(new Color(0.35f, 0.22f, 0.12f),
                MaterialUtil.CreateMottleTexture(new Color(0.35f, 0.22f, 0.12f), new Color(0.2f, 0.12f, 0.06f), 24, 6));
            var darkMat = MaterialUtil.CreateLit(new Color(0.08f, 0.06f, 0.07f));
            var spikeMat = MaterialUtil.CreateLit(new Color(0.85f, 0.18f, 0.12f));

            const float handleLength = 0.9f;
            const float handleRadius = 0.05f;
            const float ballRadius = 0.32f;

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(root.transform, false);
            handle.transform.localPosition = new Vector3(0, handleLength * 0.15f, 0);
            handle.transform.localScale = new Vector3(handleRadius * 2f, handleLength * 0.5f, handleRadius * 2f);
            Object.Destroy(handle.GetComponent<Collider>());
            handle.GetComponent<MeshRenderer>().sharedMaterial = handleMat;

            var ball = new GameObject("Ball");
            ball.transform.SetParent(root.transform, false);
            ball.transform.localPosition = new Vector3(0, handleLength * 0.15f + handleLength, 0);
            var ballMf = ball.AddComponent<MeshFilter>();
            ballMf.sharedMesh = ProceduralMesh.CreateFlatShadedIcosphere(ballRadius, 1);
            ball.AddComponent<MeshRenderer>().sharedMaterial = darkMat;

            const int spikeCount = 12;
            for (int i = 0; i < spikeCount; i++)
            {
                Vector3 dir = Random.onUnitSphere;
                var spike = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spike.name = "Spike";
                spike.transform.SetParent(ball.transform, false);
                Object.Destroy(spike.GetComponent<Collider>());
                spike.transform.localPosition = dir * (ballRadius + 0.1f);
                spike.transform.localRotation = Quaternion.LookRotation(dir);
                spike.transform.localScale = new Vector3(0.12f, 0.12f, 0.45f);
                spike.GetComponent<MeshRenderer>().sharedMaterial = spikeMat;
            }

            // A trigger hitbox on the ball turns a swing into real momentum-based melee damage
            // once wielded - separate from the solid collider below so grabbing/resting still work.
            var ballTrigger = ball.AddComponent<SphereCollider>();
            ballTrigger.radius = ballRadius + 0.12f;
            ballTrigger.isTrigger = true;
            var hitbox = ball.AddComponent<MomentumMeleeHitbox>();
            hitbox.minSpeedToHurt = 1.5f;
            hitbox.damagePerSpeed = 10f;
            hitbox.maxDamage = 45f;
            hitbox.hitCooldown = 0.4f;

            // Solid capsule spanning handle+ball so it can be found by a hand's grab-range overlap
            // check and collides properly once thrown.
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, handleLength * 0.15f + handleLength * 0.4f, 0);
            col.height = handleLength * 1.5f;
            col.radius = ballRadius;
            col.direction = 1; // Y axis

            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 6f;
            // Starts kinematic so it stands stably upright on the roof instead of toppling over on
            // its rounded capsule end; Grabbable switches it dynamic once it's picked up and thrown.
            rb.isKinematic = true;

            root.AddComponent<Grabbable>();

            return root;
        }
    }
}
