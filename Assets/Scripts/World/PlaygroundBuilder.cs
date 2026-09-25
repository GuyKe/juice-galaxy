using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// The playground just outside the school door: crate stacks (juice, but they can crush you),
    /// a rainbow bouncy ball, and a spot reserved for Ingot to teach flight.
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
                MaterialUtil.CreateCheckerTexture(new Color(0.5f, 0.7f, 0.35f), new Color(0.42f, 0.6f, 0.3f), 64, 12));
            PrimBuilder.Plane(root, "PlaygroundGround", Vector3.zero, new Vector2(22, 20), groundMat);

            // Path connecting the school door to the playground.
            var pathMat = MaterialUtil.CreateLit(new Color(0.55f, 0.5f, 0.45f));
            PrimBuilder.Plane(root, "Path", new Vector3(0, 0.01f, 8), new Vector2(2.5f, 8f), pathMat);

            CrateStack.Build(root, center + new Vector3(-6f, 0f, -3f), 4);
            CrateStack.Build(root, center + new Vector3(-6f, 0f, -1f), 3);
            CrateStack.Build(root, center + new Vector3(6f, 0f, -4f), 5);

            RainbowBouncyBall.Spawn(root, center + new Vector3(2f, 1.5f, 1f));

            var ingotSpawn = new GameObject("IngotSpawnPoint").transform;
            ingotSpawn.SetParent(root, false);
            ingotSpawn.localPosition = new Vector3(0f, 0f, -6f);
            ingotSpawn.localRotation = Quaternion.identity;

            return new Result { ingotSpawnPoint = ingotSpawn };
        }
    }
}
