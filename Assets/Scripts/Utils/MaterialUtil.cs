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

        public static Material CreateUnlit(Color color, Texture2D texture = null)
        {
            var mat = new Material(UnlitShader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (texture != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
            }
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

        /// <summary>Patchy green ground broken up by dark cracks (Voronoi cells) - the playground's turf.</summary>
        public static Texture2D CreateCrackedGroundTexture(Color baseColor, Color crackColor, int size = 64, int cellCount = 24, int seed = 0)
        {
            var rng = new System.Random(seed);
            var points = new Vector2[cellCount];
            var shades = new float[cellCount];
            for (int i = 0; i < cellCount; i++)
            {
                points[i] = new Vector2((float)rng.NextDouble() * size, (float)rng.NextDouble() * size);
                shades[i] = Mathf.Lerp(0.82f, 1.15f, (float)rng.NextDouble());
            }

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float best = float.MaxValue, second = float.MaxValue;
                    int bestIdx = 0;
                    for (int i = 0; i < cellCount; i++)
                    {
                        float dx = Mathf.Min(Mathf.Abs(x - points[i].x), size - Mathf.Abs(x - points[i].x));
                        float dy = Mathf.Min(Mathf.Abs(y - points[i].y), size - Mathf.Abs(y - points[i].y));
                        float d = dx * dx + dy * dy;
                        if (d < best) { second = best; best = d; bestIdx = i; }
                        else if (d < second) second = d;
                    }

                    float edge = Mathf.Sqrt(second) - Mathf.Sqrt(best);
                    Color c = baseColor * shades[bestIdx];
                    c.a = 1f;
                    if (edge < size * 0.035f) c = crackColor;

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Wooden crate planks with a diagonal support beam and dark corner brackets.</summary>
        public static Texture2D CreateCrateTexture(int size = 64)
        {
            Color plank = new Color(0.58f, 0.37f, 0.16f);
            Color plankDark = new Color(0.47f, 0.28f, 0.11f);
            Color metal = new Color(0.12f, 0.08f, 0.06f);

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            int plankWidth = Mathf.Max(1, size / 5);
            float cornerSize = size * 0.22f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color c = ((x / plankWidth) % 2 == 0) ? plank : plankDark;

                    float nx = x / (float)size, ny = y / (float)size;
                    if (Mathf.Abs(nx - ny) < 0.06f) c = metal;

                    bool nearCorner =
                        (x < cornerSize && y < cornerSize) || (x < cornerSize && y > size - cornerSize) ||
                        (x > size - cornerSize && y < cornerSize) || (x > size - cornerSize && y > size - cornerSize);
                    if (nearCorner)
                    {
                        float distToEdge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                        if (distToEdge < cornerSize * 0.35f) c = metal;
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>A rough stone panel with a big pair of red lips painted on it - creepy set dressing.</summary>
        public static Texture2D CreateStoneLipsTexture(int size = 96)
        {
            var tex = CreateMottleTexture(new Color(0.42f, 0.4f, 0.4f), new Color(0.26f, 0.24f, 0.25f), size, 11);

            Color lipDark = new Color(0.5f, 0.05f, 0.1f);
            Color lipMid = new Color(0.75f, 0.12f, 0.18f);
            Color seam = new Color(0.15f, 0.02f, 0.03f);
            Color tooth = new Color(0.92f, 0.9f, 0.85f);

            float cx = size * 0.5f, cy = size * 0.46f;
            float rx = size * 0.34f, ry = size * 0.16f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - cx) / rx;
                    float ny = (y - cy) / ry;
                    float d = nx * nx + ny * ny;
                    if (d <= 1f)
                    {
                        float edge = 1f - d;
                        Color c = Color.Lerp(lipDark, lipMid, Mathf.Clamp01(edge * 1.5f));

                        if (Mathf.Abs(y - cy) < size * 0.012f) c = seam;
                        if (y > cy && y < cy + size * 0.05f && d < 0.55f) c = tooth;

                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>A trippy rainbow spiral on a dark background - used for Mrs. Slithers' swirling eyes.</summary>
        public static Texture2D CreateSwirlTexture(int size = 64, int arms = 5)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxRadius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f) - center;
                    float radius = p.magnitude / maxRadius;
                    if (radius > 1f)
                    {
                        tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                        continue;
                    }

                    float angle = Mathf.Atan2(p.y, p.x) / (Mathf.PI * 2f);
                    float hue = Mathf.Repeat(angle * arms + radius * 1.6f, 1f);
                    Color swirl = Color.HSVToRGB(hue, 0.9f, 1f);

                    // Dark iris ring near the edge, tiny bright pupil dead center.
                    Color c = radius < 0.18f
                        ? Color.Lerp(Color.black, swirl, radius / 0.18f)
                        : Color.Lerp(swirl, new Color(0.03f, 0.02f, 0.05f), Mathf.SmoothStep(0f, 1f, (radius - 0.6f) / 0.4f));

                    tex.SetPixel(x, y, new Color(c.r, c.g, c.b, 1f));
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
