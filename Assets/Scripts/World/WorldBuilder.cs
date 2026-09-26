using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Top-level world assembly: school, playground, and the surrounding rainbow sky.</summary>
    public static class WorldBuilder
    {
        public class Result
        {
            public Transform playerSpawn;
            public Transform ingotSpawn;
        }

        public static Result Build()
        {
            var worldRoot = new GameObject("World").transform;

            var school = SchoolBuilder.Build(worldRoot);

            Vector3 playgroundCenter = new Vector3(-3.5f, 0f, -12f);
            var playground = PlaygroundBuilder.Build(worldRoot, playgroundCenter);

            // Centered and sized to stay hidden under both the school and the now much longer
            // playground field, so flying around outside doesn't reveal an unsupported floating edge.
            Vector3 islandCenter = new Vector3(-3.5f, 0f, -71f);
            PlanetoidFieldBuilder.Build(worldRoot, islandCenter, new Vector2(60f, 160f));

            return new Result
            {
                playerSpawn = school.spawnPoint,
                ingotSpawn = playground.ingotSpawnPoint
            };
        }
    }
}
