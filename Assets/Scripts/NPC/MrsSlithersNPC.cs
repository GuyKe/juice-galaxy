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

            var skinMat = MaterialUtil.CreateLit(new Color(0.7f, 0.06f, 0.08f),
                MaterialUtil.CreateMottleTexture(new Color(0.7f, 0.06f, 0.08f), new Color(0.85f, 0.15f, 0.15f), 32, 5));

            // The head is a script-driven "driver" that the floppy body chain trails behind.
            var head = new GameObject("Head").transform;
            head.SetParent(root, true);
            head.localPosition = new Vector3(0, 1.1f, 0);

            var headVisual = new GameObject("HeadVisual");
            headVisual.transform.SetParent(head, false);
            var headMf = headVisual.AddComponent<MeshFilter>();
            headMf.sharedMesh = ProceduralMesh.CreateFlatShadedIcosphere(0.42f, 1);
            headVisual.transform.localScale = new Vector3(1f, 1.1f, 0.95f);
            headVisual.AddComponent<MeshRenderer>().sharedMaterial = skinMat;

            BuildFace(head);

            var headCollider = head.gameObject.AddComponent<SphereCollider>();
            headCollider.radius = 0.45f;
            headCollider.isTrigger = true;

            var health = head.gameObject.AddComponent<Health>();
            health.maxHealth = 140f;

            var hitbox = head.gameObject.AddComponent<MomentumMeleeHitbox>();
            hitbox.owner = root.gameObject;
            hitbox.minSpeedToHurt = 2.5f;
            hitbox.damagePerSpeed = 4f;
            hitbox.maxDamage = 18f;

            FloppyChain.Build(root, "SlithersBody", head, 9, 0.32f, 0.28f, 0.05f, skinMat, spring: 650f, damper: 8f, segmentMass: 0.8f);

            var ai = root.gameObject.AddComponent<MrsSlithersAI>();
            ai.head = head;
            ai.homePosition = head.position;
            ai.health = health;

            return root;
        }

        static void BuildFace(Transform head)
        {
            var whiteMat = MaterialUtil.CreateLit(Color.white);
            var mouthMat = MaterialUtil.CreateLit(new Color(0.4f, 0.04f, 0.06f));
            var swirlMat = MaterialUtil.CreateUnlit(Color.white, MaterialUtil.CreateSwirlTexture(64, 5));

            // Big, flush, psychedelic swirl eyes rather than cartoon googly ones.
            foreach (float side in new[] { -1f, 1f })
            {
                PrimBuilder.Quad(head, "Eye", new Vector3(side * 0.19f, 0.08f, 0.36f),
                    new Vector2(0.24f, 0.24f), swirlMat, Quaternion.identity);
            }

            var mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mouth.name = "Mouth";
            mouth.transform.SetParent(head, false);
            mouth.transform.localPosition = new Vector3(0, -0.16f, 0.38f);
            mouth.transform.localScale = new Vector3(0.28f, 0.1f, 0.08f);
            Object.Destroy(mouth.GetComponent<Collider>());
            mouth.GetComponent<MeshRenderer>().sharedMaterial = mouthMat;

            for (int i = 0; i < 4; i++)
            {
                float x = Mathf.Lerp(-0.13f, 0.13f, i / 3f);
                var tooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tooth.name = "Tooth";
                tooth.transform.SetParent(mouth.transform, false);
                tooth.transform.localPosition = new Vector3(x / 0.28f, 0.6f, 0.4f);
                tooth.transform.localScale = new Vector3(0.45f, 1.1f, 0.45f);
                Object.Destroy(tooth.GetComponent<Collider>());
                tooth.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;
            }
        }
    }
}
