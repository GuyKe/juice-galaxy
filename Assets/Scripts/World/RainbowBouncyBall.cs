using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>A very bouncy, hue-cycling ball for the playground.</summary>
    public class RainbowBouncyBall : MonoBehaviour
    {
        public float hueSpeed = 0.25f;
        MeshRenderer _renderer;
        float _hue;

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            _hue = (_hue + Time.deltaTime * hueSpeed) % 1f;
            Color c = Color.HSVToRGB(_hue, 0.85f, 1f);
            _renderer.material.SetColor(_renderer.material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color", c);
        }

        public static GameObject Spawn(Transform parent, Vector3 position, float diameter = 0.8f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "RainbowBouncyBall";
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * diameter;
            go.GetComponent<MeshRenderer>().sharedMaterial = MaterialUtil.CreateLit(Color.red);

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.6f;
            rb.drag = 0.02f;
            rb.angularDrag = 0.05f;

            var collider = go.GetComponent<SphereCollider>();
            var bouncy = new PhysicMaterial("BouncyBall")
            {
                bounciness = 0.95f,
                dynamicFriction = 0.2f,
                staticFriction = 0.2f,
                bounceCombine = PhysicMaterialCombine.Maximum
            };
            collider.material = bouncy;

            go.AddComponent<RainbowBouncyBall>();
            return go;
        }
    }
}
