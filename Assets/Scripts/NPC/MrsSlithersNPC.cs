using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Builds Mrs. Slithers: a big floppy, weird snake teacher who lurks by her blackboard.</summary>
    public static class MrsSlithersNPC
    {
        public static Transform Spawn(Transform parent, Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("MrsSlithers").transform;
            root.SetParent(parent, true);
            root.position = position;
            root.rotation = rotation;

            var skinMat = MaterialUtil.CreateLit(new Color(0.78f, 0.1f, 0.1f),
                MaterialUtil.CreateMottleTexture(new Color(0.78f, 0.1f, 0.1f), new Color(0.95f, 0.4f, 0.2f), 32, 5));

            // The head is a script-driven "driver" that the floppy body chain trails behind.
            var head = new GameObject("Head").transform;
            head.SetParent(root, true);
            head.localPosition = new Vector3(0, 1.4f, 0);

            var headVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headVisual.name = "HeadVisual";
            headVisual.transform.SetParent(head, false);
            headVisual.transform.localScale = new Vector3(0.55f, 0.45f, 0.7f);
            Object.Destroy(headVisual.GetComponent<Collider>());
            headVisual.GetComponent<MeshRenderer>().sharedMaterial = skinMat;

            BuildFace(head, skinMat);

            var headCollider = head.gameObject.AddComponent<SphereCollider>();
            headCollider.radius = 0.4f;
            headCollider.isTrigger = true;

            var health = head.gameObject.AddComponent<Health>();
            health.maxHealth = 140f;

            var hitbox = head.gameObject.AddComponent<MomentumMeleeHitbox>();
            hitbox.owner = root.gameObject;
            hitbox.minSpeedToHurt = 2.5f;
            hitbox.damagePerSpeed = 4f;
            hitbox.maxDamage = 18f;

            FloppyChain.Build(root, "SlithersBody", head, 9, 0.32f, 0.28f, 0.05f, skinMat, spring: 650f, damper: 8f, segmentMass: 0.8f);

            var label = WorldSpaceLabel.Create(head, new Vector3(0, 0.9f, 0), "Mrs. Slithers", new Color(1f, 0.8f, 0.8f));

            var ai = root.gameObject.AddComponent<MrsSlithersAI>();
            ai.head = head;
            ai.homePosition = head.position;
            ai.health = health;

            return root;
        }

        static void BuildFace(Transform head, Material skinMat)
        {
            var whiteMat = MaterialUtil.CreateLit(Color.white);
            var blackMat = MaterialUtil.CreateLit(new Color(0.05f, 0.05f, 0.05f));
            var mouthMat = MaterialUtil.CreateLit(new Color(0.5f, 0.05f, 0.08f));

            foreach (float side in new[] { -1f, 1f })
            {
                var eyeWhite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eyeWhite.transform.SetParent(head, false);
                eyeWhite.transform.localPosition = new Vector3(side * 0.22f, 0.12f, 0.28f);
                eyeWhite.transform.localScale = Vector3.one * 0.16f;
                Object.Destroy(eyeWhite.GetComponent<Collider>());
                eyeWhite.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

                var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pupil.transform.SetParent(eyeWhite.transform, false);
                pupil.transform.localPosition = new Vector3(0, 0, 0.6f);
                pupil.transform.localScale = Vector3.one * 0.5f;
                Object.Destroy(pupil.GetComponent<Collider>());
                pupil.GetComponent<MeshRenderer>().sharedMaterial = blackMat;
            }

            var mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mouth.transform.SetParent(head, false);
            mouth.transform.localPosition = new Vector3(0, -0.12f, 0.32f);
            mouth.transform.localScale = new Vector3(0.32f, 0.12f, 0.1f);
            Object.Destroy(mouth.GetComponent<Collider>());
            mouth.GetComponent<MeshRenderer>().sharedMaterial = mouthMat;

            for (int i = 0; i < 4; i++)
            {
                float x = Mathf.Lerp(-0.13f, 0.13f, i / 3f);
                var tooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tooth.transform.SetParent(mouth.transform, false);
                tooth.transform.localPosition = new Vector3(x / 0.32f, 0.6f, 0.4f);
                tooth.transform.localScale = new Vector3(0.5f, 1.2f, 0.5f);
                Object.Destroy(tooth.GetComponent<Collider>());
                tooth.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;
            }

            // A little forked red tongue for extra weirdness.
            var tongue = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tongue.transform.SetParent(head, false);
            tongue.transform.localPosition = new Vector3(0, -0.14f, 0.45f);
            tongue.transform.localScale = new Vector3(0.05f, 0.03f, 0.3f);
            Object.Destroy(tongue.GetComponent<Collider>());
            tongue.GetComponent<MeshRenderer>().sharedMaterial = MaterialUtil.CreateLit(new Color(0.9f, 0.1f, 0.2f));
        }
    }
}
