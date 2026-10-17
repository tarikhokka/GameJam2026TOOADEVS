using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    /// <summary>
    /// (EEE, 17 Ekim) Mesai bitince masanın üstüne kararan geçiş ekranı: "MESAİ BİTTİ" + günün özeti.
    /// Prefab'a eklenmez, DeskScreen ilk kullanımda kodla kurar. Ölçekli zaman kullanır: oyun duraklatılınca geçiş de durur.
    /// </summary>
    public class ShiftEndOverlay : MonoBehaviour
    {
        static readonly Color Cream = new Color(1f, 0.96f, 0.86f, 1f);
        static readonly Color Gold = new Color(0.98f, 0.74f, 0.36f, 1f);
        static readonly Color Ink = new Color(0.23f, 0.16f, 0.15f, 1f);

        CanvasGroup group;
        TMP_Text title;
        TMP_Text info;

        public static ShiftEndOverlay Create(Transform parent, TMP_FontAsset titleFont, TMP_FontAsset infoFont)
        {
            var root = new GameObject("ShiftEndOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var rt = (RectTransform)root.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.SetAsLastSibling();
            root.GetComponent<Image>().color = new Color(0.07f, 0.05f, 0.04f, 0.94f); // arkadaki masaya tıklanmasın

            var overlay = root.AddComponent<ShiftEndOverlay>();
            overlay.group = root.GetComponent<CanvasGroup>();
            overlay.title = NewText(rt, "Title", titleFont, 120, new Vector2(0.5f, 0.55f));
            overlay.title.color = Color.white;
            overlay.title.enableVertexGradient = true;
            overlay.title.colorGradient = new VertexGradient(Cream, Cream, Gold, Gold);
            if (overlay.title.fontSharedMaterial != null) // malzeme yoksa TMP kontur verirken hata atar
            {
                overlay.title.outlineWidth = 0.24f;
                overlay.title.outlineColor = Ink;
            }
            overlay.info = NewText(rt, "Info", infoFont, 40, new Vector2(0.5f, 0.42f));
            overlay.info.color = Cream;
            overlay.Hide();
            return overlay;
        }

        static TMP_Text NewText(RectTransform parent, string name, TMP_FontAsset font, float size, Vector2 anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(1600, 160);
            var text = go.GetComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.fontSize = size;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        public IEnumerator Play(string titleText, string infoText, float fadeIn, float hold)
        {
            title.text = titleText;
            info.text = infoText;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            group.blocksRaycasts = true;
            for (float t = 0f; t < fadeIn; t += Time.deltaTime)
            {
                float k = t / fadeIn;
                group.alpha = k * k * (3f - 2f * k);
                title.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, k);
                yield return null;
            }
            group.alpha = 1f;
            title.rectTransform.localScale = Vector3.one;
            yield return new WaitForSeconds(hold);
        }

        public void Hide()
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }
}
