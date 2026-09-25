using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Creates runtime materials/textures so the whole world can be built without imported art assets.</summary>
    public static class MaterialUtil
    {
        static Shader _litShader;

        static Shader LitShader
        {
            get
            {
                if (_litShader == null)
                {
                    _litShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (_litShader == null) _litShader = Shader.Find("Standard");
                    if (_litShader == null) _litShader = Shader.Find("Diffuse");
                }
                return _litShader;
            }
        }

        static Shader _unlitShader;
        static Shader UnlitShader
        {
            get
            {
                if (_unlitShader == null)
                {
                    _unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                    if (_unlitShader == null) _unlitShader = Shader.Find("Unlit/Color");
                }
                return _unlitShader;
            }
        }

        public static Material CreateLit(Color color, Texture2D texture = null)
        {
            var mat = new Material(LitShader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (texture != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
            }
            return mat;
        }

        public static Material CreateUnlit(Color color)
        {
            var mat = new Material(UnlitShader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            return mat;
        }

        /// <summary>Blocky, low-resolution point-filtered checker texture like a PS1-era floor tile.</summary>
        public static Texture2D CreateCheckerTexture(Color a, Color b, int size = 64, int tiles = 8)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            int cell = Mathf.Max(1, size / tiles);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool checker = ((x / cell) + (y / cell)) % 2 == 0;
                    tex.SetPixel(x, y, checker ? a : b);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Noisy blotchy wall texture - mottled paint look from the reference school room.</summary>
        public static Texture2D CreateMottleTexture(Color baseColor, Color accentColor, int size = 64, int seed = 0)
        {
            var rng = new System.Random(seed);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;

            float[,] noise = new float[size, size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    noise[x, y] = 0f;

            int blobCount = Mathf.Max(6, size / 4);
            for (int i = 0; i < blobCount; i++)
            {
                int cx = rng.Next(0, size);
                int cy = rng.Next(0, size);
                float radius = Mathf.Lerp(size * 0.05f, size * 0.22f, (float)rng.NextDouble());
                float strength = Mathf.Lerp(0.2f, 1f, (float)rng.NextDouble());
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = Mathf.Min(Mathf.Abs(x - cx), size - Mathf.Abs(x - cx));
                        float dy = Mathf.Min(Mathf.Abs(y - cy), size - Mathf.Abs(y - cy));
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        float falloff = Mathf.Clamp01(1f - d / radius);
                        noise[x, y] += falloff * falloff * strength;
                    }
                }
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float t = Mathf.Clamp01(noise[x, y]);
                    Color c = Color.Lerp(baseColor, accentColor, t);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        public static Texture2D CreateSolidTexture(Color color, int size = 4)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
