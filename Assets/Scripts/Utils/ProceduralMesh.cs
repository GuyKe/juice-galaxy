using System.Collections.Generic;
using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Generates low-poly, flat-shaded meshes so planetoids/blobs match the chunky PS1-era look.</summary>
    public static class ProceduralMesh
    {
        public static Mesh CreateFlatShadedIcosphere(float radius, int subdivisions)
        {
            var (verts, tris) = BuildIcosphere(subdivisions);

            // Flat shading: duplicate a vertex per triangle so each face gets its own normal.
            var flatVerts = new List<Vector3>(tris.Count);
            var flatTris = new List<int>(tris.Count);
            var normals = new List<Vector3>(tris.Count);

            for (int i = 0; i < tris.Count; i += 3)
            {
                Vector3 a = verts[tris[i]].normalized * radius;
                Vector3 b = verts[tris[i + 1]].normalized * radius;
                Vector3 c = verts[tris[i + 2]].normalized * radius;
                Vector3 normal = Vector3.Cross(b - a, c - a).normalized;

                int baseIndex = flatVerts.Count;
                flatVerts.Add(a); flatVerts.Add(b); flatVerts.Add(c);
                normals.Add(normal); normals.Add(normal); normals.Add(normal);
                flatTris.Add(baseIndex); flatTris.Add(baseIndex + 1); flatTris.Add(baseIndex + 2);
            }

            var mesh = new Mesh { name = "Icosphere" };
            mesh.SetVertices(flatVerts);
            mesh.SetTriangles(flatTris, 0);
            mesh.SetNormals(normals);
            mesh.RecalculateBounds();
            return mesh;
        }

        static (List<Vector3> verts, List<int> tris) BuildIcosphere(int subdivisions)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();

            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            verts.AddRange(new[]
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
            });

            int[] faces =
            {
                0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
                1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
                3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
                4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
            };
            tris.AddRange(faces);

            var cache = new Dictionary<long, int>();
            for (int s = 0; s < subdivisions; s++)
            {
                var newTris = new List<int>();
                cache.Clear();
                for (int i = 0; i < tris.Count; i += 3)
                {
                    int a = tris[i], b = tris[i + 1], c = tris[i + 2];
                    int ab = MidPoint(a, b, verts, cache);
                    int bc = MidPoint(b, c, verts, cache);
                    int ca = MidPoint(c, a, verts, cache);
                    newTris.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
                }
                tris = newTris;
            }

            return (verts, tris);
        }

        static int MidPoint(int a, int b, List<Vector3> verts, Dictionary<long, int> cache)
        {
            long key = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
            if (cache.TryGetValue(key, out int existing)) return existing;
            Vector3 mid = ((verts[a] + verts[b]) * 0.5f).normalized;
            verts.Add(mid);
            int index = verts.Count - 1;
            cache[key] = index;
            return index;
        }

        /// <summary>Simple flat-shaded capsule-ish blob (stretched icosphere) used for floppy body segments.</summary>
        public static Mesh CreateBlob(float radius, float stretch, int subdivisions = 1)
        {
            Mesh mesh = CreateFlatShadedIcosphere(radius, subdivisions);
            var verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++) verts[i].y *= stretch;
            mesh.SetVertices(verts);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
