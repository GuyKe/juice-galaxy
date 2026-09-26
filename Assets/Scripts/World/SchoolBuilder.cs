using System.Collections.Generic;
using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// Builds the school: deep red mottled walls with a flat dark overhanging roof and plain dark
    /// window cutouts on the outside, a teal/blue checkered floor, rows of desks and a blank
    /// blackboard on the inside. Sits on a sealed, empty first floor the player can't get into.
    /// </summary>
    public static class SchoolBuilder
    {
        public class Result
        {
            public Transform spawnPoint;
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

            var wallMat = MaterialUtil.CreateLit(new Color(0.6f, 0.08f, 0.08f),
                MaterialUtil.CreateMottleTexture(new Color(0.6f, 0.08f, 0.08f), new Color(0.4f, 0.04f, 0.05f), 48, 3));
            var floorMat = MaterialUtil.CreateLit(Color.white,
                MaterialUtil.CreateCheckerTexture(new Color(0.35f, 0.62f, 0.55f), new Color(0.32f, 0.48f, 0.66f), 64, 10));
            var roofMat = MaterialUtil.CreateLit(new Color(0.04f, 0.04f, 0.06f));
            var boardMat = MaterialUtil.CreateLit(new Color(0.05f, 0.05f, 0.05f));

            // Floor & a flat dark roof that overhangs the walls on every side.
            PrimBuilder.Plane(root, "Floor", Vector3.zero, new Vector2(length, depth), floorMat);
            const float roofOverhang = 0.7f;
            PrimBuilder.Cube(root, "Roof", new Vector3(0, height, 0), new Vector3(length + roofOverhang, thickness, depth + roofOverhang), roofMat);

            // A sealed, empty first floor beneath the classroom - solid on every side, so it's
            // just there to ground the building and can't actually be entered.
            BuildFirstFloor(root, length, depth, wallMat);

            // Back wall (+Z) holds the blackboard.
            var backWall = new GameObject("BackWall").transform;
            backWall.SetParent(root, false);
            backWall.localPosition = new Vector3(0, 0, depth / 2f);
            backWall.localRotation = Quaternion.identity;
            BuildPlainWall(backWall, length, height, thickness, wallMat);

            var blackboard = PrimBuilder.Cube(backWall, "Blackboard", new Vector3(1.5f, 2.1f, -thickness / 2f - 0.02f),
                new Vector3(4.2f, 1.9f, 0.05f), boardMat, false).transform;

            // Front wall (-Z) has an actual walk-through doorway (not just a decal) the player
            // spawns near - otherwise the room would be sealed and unreachable from outside.
            var frontWall = new GameObject("FrontWall").transform;
            frontWall.SetParent(root, false);
            frontWall.localPosition = new Vector3(0, 0, -depth / 2f);
            frontWall.localRotation = Quaternion.Euler(0, 180f, 0);
            BuildWallWithDoor(frontWall, length, height, thickness, wallMat, -3.5f, 1.6f, 2.4f);

            // Right wall (+X), plain.
            var rightWall = new GameObject("RightWall").transform;
            rightWall.SetParent(root, false);
            rightWall.localPosition = new Vector3(length / 2f, 0, 0);
            rightWall.localRotation = Quaternion.Euler(0, -90f, 0);
            BuildPlainWall(rightWall, depth, height, thickness, wallMat);

            // Left wall (-X): five plain dark cut-out windows, like the reference image.
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

            AddDirectionalLightIfMissing();

            return new Result { spawnPoint = spawnPoint, blackboard = blackboard };
        }

        static void BuildPlainWall(Transform wallRoot, float wallLength, float wallHeight, float thickness, Material mat)
        {
            PrimBuilder.Cube(wallRoot, "Wall", new Vector3(0, wallHeight / 2f, 0), new Vector3(wallLength, wallHeight, thickness), mat);
        }

        /// <summary>A wall with an actual open doorway (floor to <paramref name="doorHeight"/>) - side
        /// pillars and a lintel above the gap, but nothing blocking the opening itself.</summary>
        static void BuildWallWithDoor(Transform wallRoot, float wallLength, float wallHeight, float thickness,
            Material mat, float doorCenter, float doorWidth, float doorHeight)
        {
            float halfLen = wallLength / 2f;
            float dStart = doorCenter - doorWidth / 2f;
            float dEnd = doorCenter + doorWidth / 2f;

            float leftWidth = dStart - (-halfLen);
            if (leftWidth > 0.02f)
                PrimBuilder.Cube(wallRoot, "Wall_Left", new Vector3(-halfLen + leftWidth / 2f, wallHeight / 2f, 0),
                    new Vector3(leftWidth, wallHeight, thickness), mat);

            float rightWidth = halfLen - dEnd;
            if (rightWidth > 0.02f)
                PrimBuilder.Cube(wallRoot, "Wall_Right", new Vector3(halfLen - rightWidth / 2f, wallHeight / 2f, 0),
                    new Vector3(rightWidth, wallHeight, thickness), mat);

            float lintelHeight = wallHeight - doorHeight;
            if (lintelHeight > 0.02f)
                PrimBuilder.Cube(wallRoot, "Wall_Lintel", new Vector3(doorCenter, doorHeight + lintelHeight / 2f, 0),
                    new Vector3(doorWidth, lintelHeight, thickness), mat);
        }

        static void BuildWindowWall(Transform wallRoot, float wallLength, float wallHeight, float thickness, Material mat)
        {
            float sillBottom = 1.1f;
            float windowHeight = 1f;
            float sillTop = sillBottom + windowHeight;

            // Five plain dark square-ish cutouts evenly spaced along the wall, like the reference,
            // sized to leave a visible pillar of wall between each one regardless of wall length.
            const int windowCount = 5;
            float usableSpan = wallLength * 0.84f;
            float slot = usableSpan / windowCount;
            float windowWidth = Mathf.Min(windowHeight, slot * 0.62f);
            var darkWindow = new Color(0.03f, 0.03f, 0.04f);
            var windows = new (float center, float width, Color color)[windowCount];
            for (int i = 0; i < windowCount; i++)
            {
                float t = (i + 0.5f) / windowCount;
                windows[i] = (Mathf.Lerp(-usableSpan / 2f, usableSpan / 2f, t), windowWidth, darkWindow);
            }

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
