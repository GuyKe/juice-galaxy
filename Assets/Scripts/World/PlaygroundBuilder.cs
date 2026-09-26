using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// The playground just outside the school door: cracked green turf, crate stacks (juice, but
    /// they can crush you), a spiked mine, a stone wall with painted-on lips, a rainbow bouncy
    /// ball, and a spot reserved for Ingot to teach flight.
    /// </summary>
    public static class PlaygroundBuilder
    {
        public class Result
        {
            public Transform ingotSpawnPoint;
        }

        public static Result Build(Transform parent, Vector3 center)
        {
            var root = new GameObject("Playground").transform;
            root.SetParent(parent, false);
            root.position = center;

            var groundMat = MaterialUtil.CreateLit(Color.white,
                MaterialUtil.CreateCrackedGroundTexture(new Color(0.42f, 0.58f, 0.26f), new Color(0.16f, 0.22f, 0.1f), 64, 24));
            PrimBuilder.Plane(root, "PlaygroundGround", Vector3.zero, new Vector2(22, 20), groundMat);

            // Path connecting the school door to the playground.
            var pathMat = MaterialUtil.CreateLit(new Color(0.55f, 0.5f, 0.45f));
            PrimBuilder.Plane(root, "Path", new Vector3(0, 0.01f, 8), new Vector2(2.5f, 8f), pathMat);

            CrateStack.Build(root, center + new Vector3(-6f, 0f, -3f), 4);
            CrateStack.Build(root, center + new Vector3(-6f, 0f, -1f), 3);
            CrateStack.Build(root, center + new Vector3(6f, 0f, -4f), 5);

            RainbowBouncyBall.Spawn(root, center + new Vector3(2f, 1.5f, 1f));
            SpikeHazard.Spawn(root, center + new Vector3(6.5f, 0.5f, -1.5f));

            BuildStoneLipsWall(root, new Vector3(2.5f, 0f, -5.5f));
            BuildObelisk(root, new Vector3(4.4f, 0f, -5.2f), 2.4f);
            BuildObelisk(root, new Vector3(1.2f, 0f, -5.0f), 1.6f);

            var ingotSpawn = new GameObject("IngotSpawnPoint").transform;
            ingotSpawn.SetParent(root, false);
            ingotSpawn.localPosition = new Vector3(0f, 0f, -6f);
            ingotSpawn.localRotation = Quaternion.identity;

            return new Result { ingotSpawnPoint = ingotSpawn };
        }

        static void BuildStoneLipsWall(Transform root, Vector3 localOffset)
        {
            var mat = MaterialUtil.CreateLit(Color.white, MaterialUtil.CreateStoneLipsTexture());
            PrimBuilder.Cube(root, "StoneLipsWall", localOffset + new Vector3(0, 1.1f, 0), new Vector3(2.2f, 2.2f, 0.35f), mat);
        }

        static void BuildObelisk(Transform root, Vector3 localOffset, float height)
        {
            var mat = MaterialUtil.CreateLit(new Color(0.3f, 0.28f, 0.3f),
                MaterialUtil.CreateMottleTexture(new Color(0.3f, 0.28f, 0.3f), new Color(0.18f, 0.16f, 0.18f), 24, 4));
            PrimBuilder.Cube(root, "Obelisk", localOffset + new Vector3(0, height / 2f, 0), new Vector3(0.6f, height, 0.6f), mat);
        }
    }
}
