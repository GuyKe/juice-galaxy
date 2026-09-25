using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Builds Ingot: a friendly golden bar-shaped NPC who teaches the player to fly.</summary>
    public static class IngotNPC
    {
        public static Transform Spawn(Transform parent, Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("Ingot").transform;
            root.SetParent(parent, true);
            root.position = position;
            root.rotation = rotation;

            var goldMat = MaterialUtil.CreateLit(new Color(0.95f, 0.78f, 0.15f));
            if (goldMat.HasProperty("_Smoothness")) goldMat.SetFloat("_Smoothness", 0.75f);
            if (goldMat.HasProperty("_Metallic")) goldMat.SetFloat("_Metallic", 0.6f);

            var body = new GameObject("Body").transform;
            body.SetParent(root, false);
            body.localPosition = new Vector3(0, 0.75f, 0);

            var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "BarVisual";
            bar.transform.SetParent(body, false);
            bar.transform.localScale = new Vector3(0.55f, 0.75f, 0.35f);
            bar.GetComponent<MeshRenderer>().sharedMaterial = goldMat;

            var whiteMat = MaterialUtil.CreateLit(Color.white);
            var blackMat = MaterialUtil.CreateLit(new Color(0.05f, 0.05f, 0.05f));
            foreach (float side in new[] { -1f, 1f })
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.transform.SetParent(body, false);
                eye.transform.localPosition = new Vector3(side * 0.13f, 0.12f, 0.18f);
                eye.transform.localScale = Vector3.one * 0.12f;
                Object.Destroy(eye.GetComponent<Collider>());
                eye.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

                var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pupil.transform.SetParent(eye.transform, false);
                pupil.transform.localPosition = new Vector3(0, 0, 0.6f);
                pupil.transform.localScale = Vector3.one * 0.5f;
                Object.Destroy(pupil.GetComponent<Collider>());
                pupil.GetComponent<MeshRenderer>().sharedMaterial = blackMat;
            }

            var smile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            smile.transform.SetParent(body, false);
            smile.transform.localPosition = new Vector3(0, -0.08f, 0.18f);
            smile.transform.localScale = new Vector3(0.22f, 0.04f, 0.05f);
            Object.Destroy(smile.GetComponent<Collider>());
            smile.GetComponent<MeshRenderer>().sharedMaterial = blackMat;

            // Floppy dangling arms and legs.
            var leftShoulder = new GameObject("LeftShoulder").transform;
            leftShoulder.SetParent(body, false);
            leftShoulder.localPosition = new Vector3(-0.3f, 0.25f, 0);
            var rightShoulder = new GameObject("RightShoulder").transform;
            rightShoulder.SetParent(body, false);
            rightShoulder.localPosition = new Vector3(0.3f, 0.25f, 0);
            var leftHip = new GameObject("LeftHip").transform;
            leftHip.SetParent(body, false);
            leftHip.localPosition = new Vector3(-0.15f, -0.4f, 0);
            var rightHip = new GameObject("RightHip").transform;
            rightHip.SetParent(body, false);
            rightHip.localPosition = new Vector3(0.15f, -0.4f, 0);

            FloppyChain.Build(root, "LeftArm", leftShoulder, 3, 0.16f, 0.09f, 0.05f, goldMat);
            FloppyChain.Build(root, "RightArm", rightShoulder, 3, 0.16f, 0.09f, 0.05f, goldMat);
            FloppyChain.Build(root, "LeftLeg", leftHip, 3, 0.18f, 0.1f, 0.06f, goldMat);
            FloppyChain.Build(root, "RightLeg", rightHip, 3, 0.18f, 0.1f, 0.06f, goldMat);

            var label = WorldSpaceLabel.Create(body, new Vector3(0, 1.1f, 0), "Ingot", new Color(1f, 0.95f, 0.6f));

            var promptLabel = WorldSpaceLabel.Create(body, new Vector3(0, 0.85f, 0), "", Color.white, 32, 0.2f);
            promptLabel.SetVisible(false);

            var trigger = root.gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 3f;
            trigger.center = new Vector3(0, 0.9f, 0);
            var rb = root.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var tutor = root.gameObject.AddComponent<IngotFlightTutor>();
            tutor.promptLabel = promptLabel;

            return root;
        }
    }
}
