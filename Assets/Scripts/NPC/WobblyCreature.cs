using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>A small bizarre blob creature roaming the playground - the game's basic enemy.</summary>
    public static class WobblyCreature
    {
        public static Transform Spawn(Transform parent, Vector3 position, Color color)
        {
            var root = new GameObject("WobblyCreature").transform;
            root.SetParent(parent, true);
            root.position = position;

            var bodyMat = MaterialUtil.CreateLit(color,
                MaterialUtil.CreateMottleTexture(color, Color.Lerp(color, Color.white, 0.5f), 24, Random.Range(0, 1000)));

            var body = new GameObject("Body").transform;
            body.SetParent(root, false);
            body.localPosition = new Vector3(0, 0.35f, 0);

            var mf = body.gameObject.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMesh.CreateBlob(0.32f, 1.15f, 1);
            var mr = body.gameObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = bodyMat;

            var col = body.gameObject.AddComponent<SphereCollider>();
            col.radius = 0.32f;

            var whiteMat = MaterialUtil.CreateLit(Color.white);
            var blackMat = MaterialUtil.CreateLit(new Color(0.05f, 0.05f, 0.05f));
            foreach (float side in new[] { -1f, 1f })
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.transform.SetParent(body, false);
                eye.transform.localPosition = new Vector3(side * 0.14f, 0.15f, 0.26f);
                eye.transform.localScale = Vector3.one * 0.13f;
                Object.Destroy(eye.GetComponent<Collider>());
                eye.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

                var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pupil.transform.SetParent(eye.transform, false);
                pupil.transform.localPosition = new Vector3(0, 0, 0.6f);
                pupil.transform.localScale = Vector3.one * 0.55f;
                Object.Destroy(pupil.GetComponent<Collider>());
                pupil.GetComponent<MeshRenderer>().sharedMaterial = blackMat;
            }

            var health = body.gameObject.AddComponent<Health>();
            health.maxHealth = 35f;

            var hitbox = body.gameObject.AddComponent<MomentumMeleeHitbox>();
            hitbox.owner = root.gameObject;
            hitbox.minSpeedToHurt = 2f;
            hitbox.damagePerSpeed = 5f;
            hitbox.maxDamage = 14f;

            var leftAntenna = new GameObject("LeftAntennaRoot").transform;
            leftAntenna.SetParent(body, false);
            leftAntenna.localPosition = new Vector3(-0.12f, 0.28f, 0);
            var rightAntenna = new GameObject("RightAntennaRoot").transform;
            rightAntenna.SetParent(body, false);
            rightAntenna.localPosition = new Vector3(0.12f, 0.28f, 0);
            FloppyChain.Build(root, "LeftAntenna", leftAntenna, 2, 0.12f, 0.04f, 0.02f, bodyMat);
            FloppyChain.Build(root, "RightAntenna", rightAntenna, 2, 0.12f, 0.04f, 0.02f, bodyMat);

            var ai = root.gameObject.AddComponent<CreatureAI>();
            ai.body = body;
            ai.homePosition = body.position;
            ai.health = health;

            health.OnDeath += () =>
            {
                JuicePickup.Spawn(body.position, Random.Range(10f, 18f));
                Object.Destroy(root.gameObject, 0.5f);
            };

            return root;
        }
    }
}
