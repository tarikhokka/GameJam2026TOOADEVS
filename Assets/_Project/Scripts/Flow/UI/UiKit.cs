using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    // Flow ekranlarının kodla UI kurmak için kullandığı küçük yardımcılar (prefab YAML'ı elle yazmamak için).
    internal static class UiKit
    {
        public static readonly Color Ink = new Color(0.10f, 0.08f, 0.06f, 1f);
        public static readonly Color Paper = new Color(0.93f, 0.88f, 0.76f, 1f);
        public static readonly Color Dark = new Color(0.13f, 0.11f, 0.09f, 1f);
        public static readonly Color Accent = new Color(0.55f, 0.16f, 0.12f, 1f);

        public static string Money(int value) =>
            value.ToString("N0", CultureInfo.InvariantCulture).Replace(',', '.') + " TL";

        public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static RectTransform Stretch(Transform parent, string name) =>
            Rect(parent, name, Vector2.zero, Vector2.one);

        public static Image Image(Transform parent, string name, Sprite sprite, Color color,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var rt = Rect(parent, name, anchorMin, anchorMax);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static TextMeshProUGUI Text(Transform parent, string name, string text, float size, Color color,
            TextAlignmentOptions align, Vector2 anchorMin, Vector2 anchorMax, bool autoSize = false,
            float minSize = 14f)
        {
            var rt = Rect(parent, name, anchorMin, anchorMax);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;
            if (autoSize)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMax = size;
                tmp.fontSizeMin = minSize;
            }
            return tmp;
        }

        public static Button Button(Transform parent, string name, string label, Action onClick,
            Vector2 anchorMin, Vector2 anchorMax, float fontSize = 34f, bool allowWhilePaused = false)
        {
            var rt = Rect(parent, name, anchorMin, anchorMax);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = Dark;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.92f, 0.8f, 1f);
            colors.pressedColor = new Color(0.75f, 0.7f, 0.6f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            btn.colors = colors;
            btn.onClick.AddListener(() =>
            {
                if (allowWhilePaused || !PauseMenu.InputBlocked) onClick();
            });

            Text(rt, "Label", label, fontSize, Paper, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            return btn;
        }

        public static void SetLabel(Button button, string label)
        {
            var tmp = button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = label;
        }

        public static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
        }
    }
}
