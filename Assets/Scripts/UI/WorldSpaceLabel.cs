using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>A floating billboard text label - name tags and speech prompts for NPCs.</summary>
    public class WorldSpaceLabel : MonoBehaviour
    {
        TextMesh _textMesh;

        public static WorldSpaceLabel Create(Transform parent, Vector3 localPos, string text, Color color, int fontSize = 40, float scale = 0.25f)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * scale;

            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.color = color;
            tm.fontSize = fontSize;
            tm.characterSize = 1f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;

            var label = go.AddComponent<WorldSpaceLabel>();
            label._textMesh = tm;
            return label;
        }

        public void SetText(string text) => _textMesh.text = text;
        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        void LateUpdate()
        {
            var cam = GameManager.Instance != null ? GameManager.Instance.playerCamera : Camera.main;
            if (cam == null) return;
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position, Vector3.up);
        }
    }
}
