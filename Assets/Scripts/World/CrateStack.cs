using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// A precarious stack of crates. Punch one loose and the stack can topple and crush you
    /// (see CrushHazard); breaking a crate rewards Juice.
    /// </summary>
    public static class CrateStack
    {
        public static void Build(Transform parent, Vector3 basePosition, int count, float crateSize = 0.6f)
        {
            var mat = MaterialUtil.CreateLit(new Color(0.55f, 0.35f, 0.15f),
                MaterialUtil.CreateMottleTexture(new Color(0.55f, 0.35f, 0.15f), new Color(0.4f, 0.24f, 0.08f), 24, Random.Range(0, 1000)));

            for (int i = 0; i < count; i++)
            {
                Vector3 jitter = new Vector3(Random.Range(-0.03f, 0.03f), 0f, Random.Range(-0.03f, 0.03f));
                Vector3 pos = basePosition + Vector3.up * (crateSize * 0.52f + i * (crateSize + 0.02f)) + jitter;

                var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crate.name = $"Crate_{i}";
                crate.transform.SetParent(parent, true);
                crate.transform.position = pos;
                crate.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 6f), 0);
                crate.transform.localScale = Vector3.one * crateSize;
                crate.GetComponent<MeshRenderer>().sharedMaterial = mat;

                var rb = crate.AddComponent<Rigidbody>();
                rb.mass = 8f;
                rb.drag = 0.2f;

                var health = crate.AddComponent<Health>();
                health.maxHealth = 25f;

                crate.AddComponent<CrushHazard>();

                health.OnDeath += () =>
                {
                    JuicePickup.Spawn(crate.transform.position, Random.Range(8f, 16f));
                    Object.Destroy(crate);
                };
            }
        }
    }
}
