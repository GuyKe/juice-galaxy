using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Top-level world assembly: school, playground, and the surrounding starfield.</summary>
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
            PlanetoidFieldBuilder.Build(worldRoot, islandCenter, new Vector2(26f, 30f));

            SpawnCreatures(worldRoot, playgroundCenter);

            return new Result
            {
                playerSpawn = school.spawnPoint,
                slithersSpawn = school.slithersSpawnPoint,
                ingotSpawn = playground.ingotSpawnPoint
            };
        }

        static void SpawnCreatures(Transform worldRoot, Vector3 playgroundCenter)
        {
            var palette = new[]
            {
                new Color(0.3f, 0.85f, 0.5f), new Color(0.9f, 0.4f, 0.75f), new Color(0.4f, 0.55f, 0.95f)
            };

            WobblyCreature.Spawn(worldRoot, playgroundCenter + new Vector3(7f, 0f, 3f), palette[0]);
            WobblyCreature.Spawn(worldRoot, playgroundCenter + new Vector3(-8f, 0f, 4f), palette[1]);
        }
    }
}
