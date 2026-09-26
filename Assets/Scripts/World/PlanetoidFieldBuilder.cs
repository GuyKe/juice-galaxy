using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Builds the space backdrop: a starfield and the chunky rock base under the school/playground island.</summary>
    public static class PlanetoidFieldBuilder
    {
        public static void Build(Transform parent, Vector3 islandCenter, Vector2 islandFootprint)
        {
            SetupSpaceAtmosphere();
            SpawnStarfield(parent);
            BuildIslandBase(parent, islandCenter, islandFootprint);
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
