using UnityEngine;
using UnityEngine.UI;

namespace JuiceGalaxy
{
    /// <summary>
    /// Segmented health + juice bars pinned to the top-left of view, styled after the reference
    /// screenshot's retro tick-marked HUD bars.
    /// </summary>
    public class JuiceBarUI : MonoBehaviour
    {
        RectTransform _healthFill;
        RectTransform _juiceFill;
        float _fullWidth = 380f;

        Health _health;
        JuiceSystem _juice;

        public static JuiceBarUI Attach(Camera cam, Health health, JuiceSystem juice)
        {
            var canvasGo = new GameObject("HUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1.4f;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var hud = canvasGo.AddComponent<JuiceBarUI>();
            hud._health = health;
            hud._juice = juice;

            hud._healthFill = BuildBar(canvasGo.transform, "HealthBar", new Vector2(24, -24), new Color(0.85f, 0.1f, 0.1f), hud._fullWidth);
            hud._juiceFill = BuildBar(canvasGo.transform, "JuiceBar", new Vector2(24, -54), new Color(0.95f, 0.55f, 0.1f), hud._fullWidth);

            if (health != null) health.OnDamaged += (a, p) => hud.UpdateBars();
            if (juice != null) juice.OnJuiceChanged += (c, m) => hud.UpdateBars();
            hud.UpdateBars();

            return hud;
        }

        static RectTransform BuildBar(Transform parent, string name, Vector2 anchoredPos, Color fillColor, float width)
        {
            var container = new GameObject(name).AddComponent<RectTransform>();
            container.SetParent(parent, false);
            container.anchorMin = new Vector2(0, 1);
            container.anchorMax = new Vector2(0, 1);
            container.pivot = new Vector2(0, 1);
            container.anchoredPosition = anchoredPos;
            container.sizeDelta = new Vector2(width, 22);

            var bg = container.gameObject.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.05f, 0.75f);

            var fillGo = new GameObject("Fill");
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.SetParent(container, false);
            fillRt.anchorMin = new Vector2(0, 0);
            fillRt.anchorMax = new Vector2(0, 1);
            fillRt.pivot = new Vector2(0, 0.5f);
            fillRt.anchoredPosition = new Vector2(2, 0);
            fillRt.sizeDelta = new Vector2(width - 4, -4);

            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.sprite = BuildTickSprite();
            fillImg.type = Image.Type.Tiled;
            fillImg.pixelsPerUnitMultiplier = 6f;

            return fillRt;
        }

        static Sprite _tickSprite;
        static Sprite BuildTickSprite()
        {
            if (_tickSprite != null) return _tickSprite;
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, x < 2 ? new Color(0, 0, 0, 0.6f) : Color.white);
            tex.Apply();
            _tickSprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
            return _tickSprite;
        }

        void UpdateBars()
        {
            if (_health != null && _healthFill != null)
                _healthFill.sizeDelta = new Vector2((_fullWidth - 4) * _health.Percent01, _healthFill.sizeDelta.y);

            if (_juice != null && _juiceFill != null)
            {
                float pct = _juice.maxJuice > 0 ? _juice.currentJuice / _juice.maxJuice : 0f;
                _juiceFill.sizeDelta = new Vector2((_fullWidth - 4) * pct, _juiceFill.sizeDelta.y);
            }
        }
    }
}
