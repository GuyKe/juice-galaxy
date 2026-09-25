using System.Collections.Generic;
using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// Builds the galaxy: a starfield backdrop, a chunky rock base under the school/playground
    /// island, and a scatter of smaller floating planetoids around it for the player to fly to.
    /// </summary>
    public static class PlanetoidFieldBuilder
    {
        public static List<Transform> Build(Transform parent, Vector3 islandCenter, Vector2 islandFootprint)
        {
            SetupSpaceAtmosphere();
            SpawnStarfield(parent);
            BuildIslandBase(parent, islandCenter, islandFootprint);
            return SpawnSatellitePlanetoids(parent, islandCenter, 7);
        }

        static void SetupSpaceAtmosphere()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.18f, 0.3f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.06f, 0.03f, 0.12f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 40f;
            RenderSettings.fogEndDistance = 140f;
        }

        static void SpawnStarfield(Transform parent)
        {
            var go = new GameObject("Starfield");
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = true;
            main.startLifetime = Mathf.Infinity;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);
            main.maxParticles = 2000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.9f), new Color(0.7f, 0.8f, 1f, 0.9f));

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 300f;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 2000) });

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = MaterialUtil.CreateUnlit(Color.white);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        static void BuildIslandBase(Transform parent, Vector3 center, Vector2 footprint)
        {
            var rockMat = MaterialUtil.CreateLit(new Color(0.32f, 0.24f, 0.4f),
                MaterialUtil.CreateMottleTexture(new Color(0.32f, 0.24f, 0.4f), new Color(0.18f, 0.12f, 0.24f), 32, 21));

            var baseGo = new GameObject("IslandBase");
            baseGo.transform.SetParent(parent, false);
            baseGo.transform.position = center + Vector3.down * 3.5f;
            baseGo.transform.localScale = new Vector3(footprint.x * 0.62f, 5f, footprint.y * 0.62f);

            var mf = baseGo.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMesh.CreateFlatShadedIcosphere(1f, 2);
            var mr = baseGo.AddComponent<MeshRenderer>();
            mr.sharedMaterial = rockMat;
            var mc = baseGo.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;
        }

        static List<Transform> SpawnSatellitePlanetoids(Transform parent, Vector3 islandCenter, int count)
        {
            var results = new List<Transform>();
            var palette = new[]
            {
                new Color(0.75f, 0.25f, 0.55f), new Color(0.2f, 0.65f, 0.7f), new Color(0.85f, 0.55f, 0.15f),
                new Color(0.45f, 0.75f, 0.25f), new Color(0.6f, 0.3f, 0.85f)
            };

            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i + Random.Range(-15f, 15f);
                float dist = Random.Range(16f, 34f);
                float heightOffset = Random.Range(-6f, 10f);
                Vector3 pos = islandCenter + Quaternion.Euler(0, angle, 0) * Vector3.forward * dist + Vector3.up * heightOffset;

                float radius = Random.Range(2f, 5f);
                Color baseColor = palette[i % palette.Length];
                var mat = MaterialUtil.CreateLit(baseColor,
                    MaterialUtil.CreateMottleTexture(baseColor, Color.Lerp(baseColor, Color.black, 0.4f), 32, i * 13 + 1));

                var go = new GameObject($"Planetoid_{i}");
                go.transform.SetParent(parent, false);
                go.transform.position = pos;
                go.transform.rotation = Random.rotation;

                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = ProceduralMesh.CreateFlatShadedIcosphere(radius, 2);
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                var mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;

                // Purely cosmetic drift; kept slow so the (static, concave) collider doesn't need
                // frequent broadphase rebuilds while the player is standing on a planetoid.
                go.AddComponent<SlowSpin>().speed = Random.Range(-1.5f, 1.5f);

                results.Add(go.transform);
            }

            return results;
        }
    }
}
