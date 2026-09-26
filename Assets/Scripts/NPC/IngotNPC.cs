using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Builds Ingot: a round, dark, googly-eyed creature who teaches the player to fly.</summary>
    public static class IngotNPC
    {
        public static Transform Spawn(Transform parent, Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("Ingot").transform;
            root.SetParent(parent, true);
            root.position = position;
            root.rotation = rotation;

            var darkMat = MaterialUtil.CreateLit(new Color(0.08f, 0.08f, 0.09f));
            if (darkMat.HasProperty("_Smoothness")) darkMat.SetFloat("_Smoothness", 0.7f);
            if (darkMat.HasProperty("_Metallic")) darkMat.SetFloat("_Metallic", 0.5f);
            var whiteMat = MaterialUtil.CreateLit(new Color(0.93f, 0.9f, 0.85f));
            var blackMat = MaterialUtil.CreateLit(new Color(0.03f, 0.03f, 0.03f));

            const float scale = 1.7f;
            const float bodyRadius = 0.42f * scale;
            const float headRadius = 0.22f * scale;

            var body = new GameObject("Body").transform;
            body.SetParent(root, false);
            body.localPosition = new Vector3(0, bodyRadius + 0.03f, 0);

            var bodyVisual = new GameObject("BodyVisual");
            bodyVisual.transform.SetParent(body, false);
            var bodyMf = bodyVisual.AddComponent<MeshFilter>();
            bodyMf.sharedMesh = ProceduralMesh.CreateFlatShadedIcosphere(bodyRadius, 2);
            bodyVisual.AddComponent<MeshRenderer>().sharedMaterial = darkMat;

            var head = new GameObject("Head").transform;
            head.SetParent(body, false);
            head.localPosition = new Vector3(0, bodyRadius * 0.75f, bodyRadius * 0.2f);
            var headMf = head.gameObject.AddComponent<MeshFilter>();
            headMf.sharedMesh = ProceduralMesh.CreateFlatShadedIcosphere(headRadius, 1);
            head.gameObject.AddComponent<MeshRenderer>().sharedMaterial = darkMat;

            BuildFace(head, whiteMat, blackMat, headRadius);

            // Floppy dangling arms and legs, dark like the body, each capped with a pale tip.
            var leftShoulder = new GameObject("LeftShoulder").transform;
            leftShoulder.SetParent(body, false);
            leftShoulder.localPosition = new Vector3(-bodyRadius * 0.8f, bodyRadius * 0.15f, 0);
            var rightShoulder = new GameObject("RightShoulder").transform;
            rightShoulder.SetParent(body, false);
            rightShoulder.localPosition = new Vector3(bodyRadius * 0.8f, bodyRadius * 0.15f, 0);
            var leftHip = new GameObject("LeftHip").transform;
            leftHip.SetParent(body, false);
            leftHip.localPosition = new Vector3(-bodyRadius * 0.45f, -bodyRadius * 0.85f, 0);
            var rightHip = new GameObject("RightHip").transform;
            rightHip.SetParent(body, false);
            rightHip.localPosition = new Vector3(bodyRadius * 0.45f, -bodyRadius * 0.85f, 0);
            var tailRoot = new GameObject("TailRoot").transform;
            tailRoot.SetParent(body, false);
            tailRoot.localPosition = new Vector3(bodyRadius * 0.3f, bodyRadius * 0.7f, -bodyRadius * 0.3f);

            AddCappedLimb(root, "LeftArm", leftShoulder, 3, 0.16f * scale, 0.09f * scale, 0.05f * scale, darkMat, whiteMat);
            AddCappedLimb(root, "RightArm", rightShoulder, 3, 0.16f * scale, 0.09f * scale, 0.05f * scale, darkMat, whiteMat);
            AddCappedLimb(root, "LeftLeg", leftHip, 3, 0.15f * scale, 0.08f * scale, 0.05f * scale, darkMat, whiteMat);
            AddCappedLimb(root, "RightLeg", rightHip, 3, 0.15f * scale, 0.08f * scale, 0.05f * scale, darkMat, whiteMat);
            AddCappedLimb(root, "Tail", tailRoot, 3, 0.13f * scale, 0.05f * scale, 0.02f * scale, darkMat, whiteMat);

            var label = WorldSpaceLabel.Create(body, new Vector3(0, 0.95f * scale, 0), "Ingot", new Color(0.95f, 0.9f, 0.8f));

            var promptLabel = WorldSpaceLabel.Create(body, new Vector3(0, 0.72f * scale, 0), "", Color.white, 32, 0.2f);
            promptLabel.SetVisible(false);

            var trigger = root.gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 3f * Mathf.Max(1f, scale * 0.7f);
            trigger.center = new Vector3(0, bodyRadius, 0);
            var rb = root.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var tutor = root.gameObject.AddComponent<IngotFlightTutor>();
            tutor.promptLabel = promptLabel;

            return root;
        }

        static void BuildFace(Transform head, Material whiteMat, Material blackMat, float headRadius)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = "Eye";
                eye.transform.SetParent(head, false);
                eye.transform.localPosition = new Vector3(side * headRadius * 0.45f, headRadius * 0.25f, headRadius * 0.85f);
                eye.transform.localRotation = Quaternion.Euler(0, 0, side * -25f);
                eye.transform.localScale = new Vector3(headRadius * 0.55f, headRadius * 0.22f, headRadius * 0.18f);
                Object.Destroy(eye.GetComponent<Collider>());
                eye.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;
            }

            var mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mouth.name = "Mouth";
            mouth.transform.SetParent(head, false);
            mouth.transform.localPosition = new Vector3(0, -headRadius * 0.3f, headRadius * 0.85f);
            mouth.transform.localScale = new Vector3(headRadius * 0.75f, headRadius * 0.32f, headRadius * 0.1f);
            Object.Destroy(mouth.GetComponent<Collider>());
            mouth.GetComponent<MeshRenderer>().sharedMaterial = blackMat;

            for (int i = 0; i < 3; i++)
            {
                float x = Mathf.Lerp(-0.3f, 0.3f, i / 2f);
                var tooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tooth.name = "Tooth";
                tooth.transform.SetParent(mouth.transform, false);
                tooth.transform.localPosition = new Vector3(x, 0.55f, 0.6f);
                tooth.transform.localScale = new Vector3(0.4f, 0.9f, 0.4f);
                Object.Destroy(tooth.GetComponent<Collider>());
                tooth.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;
            }
        }

        static void AddCappedLimb(Transform root, string name, Transform driver, int segments, float segmentLength,
            float startRadius, float endRadius, Material limbMat, Material tipMat)
        {
            var chain = FloppyChain.Build(root, name, driver, segments, segmentLength, startRadius, endRadius, limbMat);
            var tip = chain.TipTransform;
            if (tip == null) return;

            // The chain's visuals are unscaled meshes sized in world units, so the cap must be
            // scaled from the actual tip radius rather than a flat multiplier.
            var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = "Tip";
            cap.transform.SetParent(tip, false);
            cap.transform.localScale = Vector3.one * endRadius * 2.2f;
            Object.Destroy(cap.GetComponent<Collider>());
            cap.GetComponent<MeshRenderer>().sharedMaterial = tipMat;
        }
    }
}
