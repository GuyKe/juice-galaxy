using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// The single object living in Bootstrap.unity. Everything else - world geometry, the player
    /// rig, NPCs and UI - is constructed procedurally at runtime, which keeps the whole project to
    /// one hand-authored scene file and lets the entire game live in code.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        void Awake()
        {
            var gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();

            var world = WorldBuilder.Build();

            Vector3 spawnPos = world.playerSpawn != null ? world.playerSpawn.position : Vector3.zero;
            Quaternion spawnRot = world.playerSpawn != null ? world.playerSpawn.rotation : Quaternion.identity;
            var player = PlayerFactory.Spawn(spawnPos, spawnRot);

            if (world.slithersSpawn != null)
                MrsSlithersNPC.Spawn(null, world.slithersSpawn.position, world.slithersSpawn.rotation);

            if (world.ingotSpawn != null)
                IngotNPC.Spawn(null, world.ingotSpawn.position, world.ingotSpawn.rotation);

            var gm = GameManager.Instance;
            if (gm != null && gm.playerCamera != null)
                JuiceBarUI.Attach(gm.playerCamera, gm.playerHealth, gm.playerJuice);
        }
    }
}
