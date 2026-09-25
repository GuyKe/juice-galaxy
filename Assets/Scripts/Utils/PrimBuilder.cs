using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Thin wrapper around GameObject.CreatePrimitive for quickly blocking out low-poly level geometry.</summary>
    public static class PrimBuilder
    {
        public static GameObject Cube(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat, bool collider = true, Quaternion? rot = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = rot ?? Quaternion.identity;
            go.transform.localScale = localScale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (!collider) Object.Destroy(go.GetComponent<Collider>());
            return go;
        }

        public static GameObject Plane(Transform parent, string name, Vector3 localPos, Vector2 sizeXZ, Material mat, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(sizeXZ.x / 10f, 1f, sizeXZ.y / 10f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (!collider) Object.Destroy(go.GetComponent<Collider>());
            return go;
        }

        public static GameObject Sphere(Transform parent, string name, Vector3 localPos, float diameter, Material mat, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * diameter;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (!collider) Object.Destroy(go.GetComponent<Collider>());
            return go;
        }

        public static GameObject Quad(Transform parent, string name, Vector3 localPos, Vector2 size, Material mat, Quaternion? rot = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = rot ?? Quaternion.identity;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.Destroy(go.GetComponent<Collider>());
            return go;
        }
    }
}
