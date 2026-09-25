using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Top-level world assembly: school, playground, and the surrounding galaxy of planetoids.</summary>
    public static class WorldBuilder
    {
        public class Result
        {
            public Transform playerSpawn;
            public Transform slithersSpawn;
            public Transform ingotSpawn;
        }

        public static Result Build()
        {
            var worldRoot = new GameObject("World").transform;

            var school = SchoolBuilder.Build(worldRoot);

            Vector3 playgroundCenter = new Vector3(-3.5f, 0f, -12f);
            var playground = PlaygroundBuilder.Build(worldRoot, playgroundCenter);

            Vector3 islandCenter = new Vector3(-1.5f, 0f, -6f);
            var planetoids = PlanetoidFieldBuilder.Build(worldRoot, islandCenter, new Vector2(26f, 30f));

            SpawnCreatures(worldRoot, playgroundCenter, planetoids);

            return new Result
            {
                playerSpawn = school.spawnPoint,
                slithersSpawn = school.slithersSpawnPoint,
                ingotSpawn = playground.ingotSpawnPoint
            };
        }

        static void SpawnCreatures(Transform worldRoot, Vector3 playgroundCenter, System.Collections.Generic.List<Transform> planetoids)
        {
            var palette = new[]
            {
                new Color(0.3f, 0.85f, 0.5f), new Color(0.9f, 0.4f, 0.75f), new Color(0.4f, 0.55f, 0.95f)
            };

            WobblyCreature.Spawn(worldRoot, playgroundCenter + new Vector3(7f, 0f, 3f), palette[0]);
            WobblyCreature.Spawn(worldRoot, playgroundCenter + new Vector3(-8f, 0f, 4f), palette[1]);

            for (int i = 0; i < planetoids.Count; i++)
            {
                if (i % 2 != 0) continue;
                var p = planetoids[i];
                Vector3 spawnPos = p.position + Vector3.up * (EstimateRadius(p) + 0.4f);
                WobblyCreature.Spawn(worldRoot, spawnPos, palette[i % palette.Length]);
            }
        }

        static float EstimateRadius(Transform planetoid)
        {
            var mf = planetoid.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null) return mf.sharedMesh.bounds.extents.magnitude * 0.6f;
            return 3f;
        }
    }
}
