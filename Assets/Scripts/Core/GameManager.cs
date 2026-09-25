using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Small central lookup so world objects (crates, NPCs, UI) can find the player and its systems.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public Transform player;
        public JuiceSystem playerJuice;
        public Health playerHealth;
        public Camera playerCamera;
        public Transform schoolSpawnPoint;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RegisterPlayer(Transform playerRoot, JuiceSystem juice, Health health, Camera camera)
        {
            player = playerRoot;
            playerJuice = juice;
            playerHealth = health;
            playerCamera = camera;
        }
    }
}
