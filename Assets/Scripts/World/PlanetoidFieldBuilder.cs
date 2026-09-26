using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Builds the backdrop: a psychedelic rainbow sky dome and the chunky rock base under the school/playground island.</summary>
    public static class PlanetoidFieldBuilder
    {
        public static void Build(Transform parent, Vector3 islandCenter, Vector2 islandFootprint)
        {
            SetupSkyAtmosphere();
            SpawnRainbowSky(parent, islandCenter);
            BuildIslandBase(parent, islandCenter, islandFootprint);
        }

        static void SetupSkyAtmosphere()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.5f, 0.6f);
            RenderSettings.fog = false;
        }

        /// <summary>
        /// A huge inverted sphere painted with a vertical rainbow gradient - the player always
        /// stands inside it. Winding is flipped so the (otherwise back-face-culled) inner surface
        /// renders, and it uses the same URP Unlit shader as everything else in the world so it
        /// isn't at risk of being stripped from the on-device build.
        /// </summary>
        static void SpawnRainbowSky(Transform parent, Vector3 center)
        {
            const float radius = 400f;
            var mesh = ProceduralMesh.CreateFlatShadedIcosphere(radius, 3);

            var verts = mesh.vertices;
            var uvs = new Vector2[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                float t = Mathf.InverseLerp(-radius, radius, verts[i].y);
                uvs[i] = new Vector2(0f, t);
            }
            mesh.uv = uvs;

            var tris = mesh.triangles;
            for (int i = 0; i < tris.Length; i += 3)
            {
                (tris[i + 1], tris[i + 2]) = (tris[i + 2], tris[i + 1]);
            }
            mesh.triangles = tris;

            var go = new GameObject("RainbowSky");
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.sharedMaterial = MaterialUtil.CreateUnlit(Color.white, BuildRainbowGradientTexture());
        }

        static Texture2D BuildRainbowGradientTexture()
        {
            var stops = new (float t, Color c)[]
            {
                (0.0f,  new Color(0.85f, 0.25f, 0.1f)),
                (0.25f, new Color(0.9f, 0.5f, 0.55f)),
                (0.45f, new Color(0.75f, 0.65f, 0.85f)),
                (0.65f, new Color(0.55f, 0.7f, 0.85f)),
                (0.85f, new Color(0.55f, 0.8f, 0.55f)),
                (1.0f,  new Color(0.8f, 0.85f, 0.45f)),
            };

            const int size = 128;
            var tex = new Texture2D(4, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                float t = y / (float)(size - 1);
                Color c = stops[stops.Length - 1].c;
                for (int i = 0; i < stops.Length - 1; i++)
                {
                    if (t >= stops[i].t && t <= stops[i + 1].t)
                    {
                        float localT = Mathf.InverseLerp(stops[i].t, stops[i + 1].t, t);
                        c = Color.Lerp(stops[i].c, stops[i + 1].c, localT);
                        break;
                    }
                }
                for (int x = 0; x < 4; x++) tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return tex;
        }

        static void BuildIslandBase(Transform parent, Vector3 center, Vector2 footprint)
        {
            var rockMat = MaterialUtil.CreateLit(new Color(0.32f, 0.24f, 0.4f),
                MaterialUtil.CreateMottleTexture(new Color(0.32f, 0.24f, 0.4f), new Color(0.18f, 0.12f, 0.24f), 32, 21));

            const float islandHeight = 5f;
            // Push the rock down far enough that even its highest point (directly below the
            // island center) stays under floor level (y=0) - otherwise it pokes up through the
            // school/playground floor as a stray blob in the middle of the room.
            const float clearance = 0.5f;

            var baseGo = new GameObject("IslandBase");
            baseGo.transform.SetParent(parent, false);
            baseGo.transform.position = center + Vector3.down * (islandHeight + clearance);
            baseGo.transform.localScale = new Vector3(footprint.x * 0.62f, islandHeight, footprint.y * 0.62f);

            var mf = baseGo.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMesh.CreateFlatShadedIcosphere(1f, 2);
            var mr = baseGo.AddComponent<MeshRenderer>();
            mr.sharedMaterial = rockMat;
            var mc = baseGo.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;
        }
    }
}
