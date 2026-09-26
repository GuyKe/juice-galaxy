using System.Collections.Generic;
using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// Builds the school spawn room from the reference screenshot: mottled tan walls, a teal/blue
    /// checkered floor, three colorful cut-out windows, rows of desks, and a blank blackboard where
    /// Mrs. Slithers waits. Sits on a sealed, empty first floor the player can't get into.
    /// </summary>
    public static class SchoolBuilder
    {
        public class Result
        {
            public Transform spawnPoint;
            public Transform slithersSpawnPoint;
            public Transform blackboard;
        }

        public static Result Build(Transform parent)
        {
            var root = new GameObject("School").transform;
            root.SetParent(parent, false);

            const float length = 10f;  // along X (window wall)
            const float depth = 8f;    // along Z
            const float height = 7f;
            const float thickness = 0.3f;

            var wallMat = MaterialUtil.CreateLit(new Color(0.62f, 0.5f, 0.32f),
                MaterialUtil.CreateMottleTexture(new Color(0.62f, 0.5f, 0.32f), new Color(0.42f, 0.32f, 0.18f), 48, 3));
            var floorMat = MaterialUtil.CreateLit(Color.white,
                MaterialUtil.CreateCheckerTexture(new Color(0.35f, 0.62f, 0.55f), new Color(0.32f, 0.48f, 0.66f), 64, 10));
            var ceilingMat = MaterialUtil.CreateLit(new Color(0.05f, 0.05f, 0.07f));
            var boardMat = MaterialUtil.CreateLit(new Color(0.05f, 0.05f, 0.05f));
            var doorMat = MaterialUtil.CreateLit(new Color(0.4f, 0.22f, 0.12f),
                MaterialUtil.CreateMottleTexture(new Color(0.4f, 0.22f, 0.12f), new Color(0.25f, 0.13f, 0.07f), 32, 9));

            // Floor & ceiling
            PrimBuilder.Plane(root, "Floor", Vector3.zero, new Vector2(length, depth), floorMat);
            PrimBuilder.Cube(root, "Ceiling", new Vector3(0, height, 0), new Vector3(length, thickness, depth), ceilingMat);

            // A sealed, empty first floor beneath the classroom - solid on every side, so it's
            // just there to ground the building and can't actually be entered.
            BuildFirstFloor(root, length, depth, wallMat);

            // Back wall (+Z) holds the blackboard - Mrs. Slithers' domain.
            var backWall = new GameObject("BackWall").transform;
            backWall.SetParent(root, false);
            backWall.localPosition = new Vector3(0, 0, depth / 2f);
            backWall.localRotation = Quaternion.identity;
            BuildPlainWall(backWall, length, height, thickness, wallMat);

            var blackboard = PrimBuilder.Cube(backWall, "Blackboard", new Vector3(1.5f, 2.1f, -thickness / 2f - 0.02f),
                new Vector3(4.2f, 1.9f, 0.05f), boardMat, false).transform;

            // Front wall (-Z) has the doorway the player spawns near.
            var frontWall = new GameObject("FrontWall").transform;
            frontWall.SetParent(root, false);
            frontWall.localPosition = new Vector3(0, 0, -depth / 2f);
            frontWall.localRotation = Quaternion.Euler(0, 180f, 0);
            BuildPlainWall(frontWall, length, height, thickness, wallMat);
            PrimBuilder.Cube(frontWall, "Door", new Vector3(-3.5f, 1.1f, -thickness / 2f - 0.02f), new Vector3(1.2f, 2.2f, 0.05f), doorMat, false);

            // Right wall (+X), plain.
            var rightWall = new GameObject("RightWall").transform;
            rightWall.SetParent(root, false);
            rightWall.localPosition = new Vector3(length / 2f, 0, 0);
            rightWall.localRotation = Quaternion.Euler(0, -90f, 0);
            BuildPlainWall(rightWall, depth, height, thickness, wallMat);

            // Left wall (-X): three colorful cut-out windows, like the reference image.
            var leftWall = new GameObject("LeftWall").transform;
            leftWall.SetParent(root, false);
            leftWall.localPosition = new Vector3(-length / 2f, 0, 0);
            leftWall.localRotation = Quaternion.Euler(0, 90f, 0);
            BuildWindowWall(leftWall, depth, height, thickness, wallMat);

            BuildDesks(root);

            var spawnPoint = new GameObject("SpawnPoint").transform;
            spawnPoint.SetParent(root, false);
            spawnPoint.localPosition = new Vector3(0, 0, -depth / 2f + 2f);
            spawnPoint.localRotation = Quaternion.Euler(0, 0, 0);

            var slithersSpawn = new GameObject("SlithersSpawnPoint").transform;
            slithersSpawn.SetParent(root, false);
            slithersSpawn.localPosition = new Vector3(2.6f, 0, depth / 2f - 1f);
            slithersSpawn.localRotation = Quaternion.Euler(0, 180f, 0);

            AddDirectionalLightIfMissing();

            return new Result { spawnPoint = spawnPoint, slithersSpawnPoint = slithersSpawn, blackboard = blackboard };
        }

        static void BuildPlainWall(Transform wallRoot, float wallLength, float wallHeight, float thickness, Material mat)
        {
            PrimBuilder.Cube(wallRoot, "Wall", new Vector3(0, wallHeight / 2f, 0), new Vector3(wallLength, wallHeight, thickness), mat);
        }

        static void BuildWindowWall(Transform wallRoot, float wallLength, float wallHeight, float thickness, Material mat)
        {
            float sillBottom = 1.1f;
            float windowHeight = 1.3f;
            float sillTop = sillBottom + windowHeight;

            var windows = new (float center, float width, Color color)[]
            {
                (-wallLength * 0.32f, 1.5f, new Color(0.85f, 0.15f, 0.15f)),
                (0f,                   1.5f, new Color(0.95f, 0.55f, 0.15f)),
                (wallLength * 0.32f,  1.4f, new Color(0.85f, 0.9f, 0.75f)),
            };

            // Bottom band (floor to sill) and top band (sill+window to ceiling) run the full length.
            PrimBuilder.Cube(wallRoot, "Wall_Bottom", new Vector3(0, sillBottom / 2f, 0), new Vector3(wallLength, sillBottom, thickness), mat);
            float topBandHeight = wallHeight - sillTop;
            PrimBuilder.Cube(wallRoot, "Wall_Top", new Vector3(0, sillTop + topBandHeight / 2f, 0), new Vector3(wallLength, topBandHeight, thickness), mat);

            // Pillars between/around the windows at window height, plus the window "glass" quads.
            var gaps = new List<(float start, float end)>();
            float halfLen = wallLength / 2f;
            float cursor = -halfLen;
            foreach (var w in windows)
            {
                float wStart = w.center - w.width / 2f;
                float wEnd = w.center + w.width / 2f;
                gaps.Add((cursor, wStart));
                cursor = wEnd;

                var glass = PrimBuilder.Quad(wallRoot, "WindowView", new Vector3(w.center, (sillBottom + sillTop) / 2f, -thickness / 2f - 0.03f),
                    new Vector2(w.width, windowHeight), MaterialUtil.CreateUnlit(w.color));
                glass.transform.localRotation = Quaternion.identity;

                PrimBuilder.Cube(wallRoot, "WindowFrame", new Vector3(w.center, (sillBottom + sillTop) / 2f, 0f),
                    new Vector3(w.width + 0.08f, windowHeight + 0.08f, thickness * 0.4f), mat, false);
            }
            gaps.Add((cursor, halfLen));

            foreach (var gap in gaps)
            {
                float w = gap.end - gap.start;
                if (w <= 0.02f) continue;
                float center = (gap.start + gap.end) / 2f;
                PrimBuilder.Cube(wallRoot, "Wall_Pillar", new Vector3(center, (sillBottom + sillTop) / 2f, 0), new Vector3(w, windowHeight, thickness), mat);
            }
        }

        static void BuildFirstFloor(Transform root, float length, float depth, Material wallMat)
        {
            const float floorHeight = 3f;
            PrimBuilder.Cube(root, "FirstFloor", new Vector3(0, -floorHeight / 2f, 0), new Vector3(length, floorHeight, depth), wallMat);
        }

        static void BuildDesks(Transform root)
        {
            var topMat = MaterialUtil.CreateLit(new Color(0.75f, 0.4f, 0.12f));
            var legMat = MaterialUtil.CreateLit(new Color(0.15f, 0.15f, 0.18f));

            var positions = new Vector3[]
            {
                new Vector3(-1.8f, 0, -0.8f), new Vector3(0.6f, 0, -1.1f), new Vector3(2.6f, 0, -1.6f),
                new Vector3(-1.4f, 0, 1.0f), new Vector3(0.9f, 0, 1.3f), new Vector3(-3.0f, 0, 2.4f),
            };

            foreach (var pos in positions)
            {
                var desk = new GameObject("Desk").transform;
                desk.SetParent(root, false);
                desk.localPosition = pos;
                desk.localRotation = Quaternion.Euler(0, Random.Range(-8f, 8f), 0);

                PrimBuilder.Cube(desk, "Top", new Vector3(0, 0.55f, 0), new Vector3(0.7f, 0.08f, 0.5f), topMat);
                float lx = 0.28f, lz = 0.2f;
                foreach (var sign in new[] { new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1) })
                {
                    PrimBuilder.Cube(desk, "Leg", new Vector3(sign.x * lx, 0.27f, sign.y * lz), new Vector3(0.06f, 0.54f, 0.06f), legMat);
                }
            }
        }

        static void AddDirectionalLightIfMissing()
        {
            if (Object.FindObjectOfType<Light>() != null) return;
            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.97f, 0.9f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0);
        }
    }
}
