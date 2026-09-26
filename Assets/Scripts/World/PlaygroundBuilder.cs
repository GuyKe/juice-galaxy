using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// The playground just outside the school door: a wide, sprawling stretch of cracked green
    /// turf with crate stacks (juice, but they can crush you), a spiked mine, a stone wall with
    /// painted-on lips, a rainbow bouncy ball, and a spot reserved for Ingot to teach flight.
    /// </summary>
    public static class PlaygroundBuilder
    {
        public class Result
        {
            public Transform ingotSpawnPoint;
        }

        // How much further out from the center every prop below sits, to make the field feel as
        // wide and open as the reference photo instead of a tight little yard.
        const float spread = 2.2f;

        public static Result Build(Transform parent, Vector3 center)
        {
            var root = new GameObject("Playground").transform;
            root.SetParent(parent, false);
            root.position = center;

            var groundMat = MaterialUtil.CreateLit(Color.white,
                MaterialUtil.CreateCrackedGroundTexture(new Color(0.42f, 0.58f, 0.26f), new Color(0.16f, 0.22f, 0.1f), 64, 24));
            // A long stretch of turf running away from the school. Its near edge reaches exactly to
            // the school's exterior wall face - flush, not overlapping, so there's no gap when you
            // walk out the door, but also no z-fighting with the school's own interior floor (which
            // only extends on the other side of that same wall).
            const float groundLength = 140f;
            const float groundNearEdge = 8f;
            PrimBuilder.Plane(root, "PlaygroundGround", new Vector3(0, 0, groundNearEdge - groundLength / 2f),
                new Vector2(60, groundLength), groundMat);

            // Path connecting the school door to the playground.
            var pathMat = MaterialUtil.CreateLit(new Color(0.55f, 0.5f, 0.45f));
            PrimBuilder.Plane(root, "Path", new Vector3(0, 0.01f, 8), new Vector2(2.5f, 8f), pathMat);

            // Crates near the school door...
            CrateStack.Build(root, center + Spread(-6f, -3f), 4);
            CrateStack.Build(root, center + Spread(-6f, -1f), 3);
            CrateStack.Build(root, center + Spread(6f, -4f), 5);
            // ...more scattered through the middle of the field...
            CrateStack.Build(root, center + new Vector3(-9f, 0f, -60f), 4);
            CrateStack.Build(root, center + new Vector3(8f, 0f, -68f), 3);
            // ...and more still further back, near the far end.
            CrateStack.Build(root, center + new Vector3(9f, 0f, -102f), 5);
            CrateStack.Build(root, center + new Vector3(-8f, 0f, -110f), 4);

            RainbowBouncyBall.Spawn(root, center + new Vector3(0f, 1.5f, -62f));
            SpikeHazard.Spawn(root, center + Spread(6.5f, -1.5f, 0.5f));

            // The lips wall waits all the way out at the far end of the long field.
            BuildStoneLipsWall(root, new Vector3(0f, 0f, groundNearEdge - groundLength + 8f));
            BuildObelisk(root, Spread(4.4f, -5.2f), 2.4f);
            BuildObelisk(root, Spread(1.2f, -5.0f), 1.6f);

            var ingotSpawn = new GameObject("IngotSpawnPoint").transform;
            ingotSpawn.SetParent(root, false);
            ingotSpawn.localPosition = Spread(0f, -6f);
            ingotSpawn.localRotation = Quaternion.identity;

            return new Result { ingotSpawnPoint = ingotSpawn };
        }

        /// <summary>A world/local XZ offset scaled by <see cref="spread"/>, with an unscaled Y.</summary>
        static Vector3 Spread(float x, float z, float y = 0f) => new Vector3(x * spread, y, z * spread);

        static void BuildStoneLipsWall(Transform root, Vector3 localOffset)
        {
            var mat = MaterialUtil.CreateLit(Color.white, MaterialUtil.CreateStoneLipsTexture());
            PrimBuilder.Cube(root, "StoneLipsWall", localOffset + new Vector3(0, 1.9f, 0), new Vector3(4f, 3.8f, 0.5f), mat);
        }

        static void BuildObelisk(Transform root, Vector3 localOffset, float height)
        {
            var mat = MaterialUtil.CreateLit(new Color(0.3f, 0.28f, 0.3f),
                MaterialUtil.CreateMottleTexture(new Color(0.3f, 0.28f, 0.3f), new Color(0.18f, 0.16f, 0.18f), 24, 4));
            PrimBuilder.Cube(root, "Obelisk", localOffset + new Vector3(0, height / 2f, 0), new Vector3(0.6f, height, 0.6f), mat);
        }
    }
}
