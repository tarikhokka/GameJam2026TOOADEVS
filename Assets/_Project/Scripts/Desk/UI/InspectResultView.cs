using System.Collections;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    /// <summary>
    /// İncelemenin yalnızca sunumu. Boş bir UI nesnesine eklenir; kaşeyi kendisi kurar.
    /// Konum, merkezlenmiş parent RectTransform'a göre anchoredPosition'dır.
    /// Çizgi, seçim, yaklaşık 1,6 sn bekleme ve iptal Desk'in çağıran bileşenine aittir.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public sealed class InspectResultView : MonoBehaviour
    {
        [Header("Yazı tipleri")]
        [SerializeField] TMP_FontAsset titleFont;
        [SerializeField] TMP_FontAsset detailFont;

        [Header("Ses — Inspector'dan bağlanır")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip click;
        [SerializeField] AudioClip stamp;
        [SerializeField, Range(0f, 1f)] float volume = 0.65f;

        static readonly Color MatchColor = new Color(63f / 255f, 138f / 255f, 79f / 255f);
        static readonly Color MismatchColor = new Color(194f / 255f, 65f / 255f, 47f / 255f);
        static readonly Color UnrelatedColor = new Color(122f / 255f, 102f / 255f, 80f / 255f);
        static readonly Color Paper = new Color(1f, 0.97f, 0.89f);
        const float Width = 560f, Padding = 24f, Tilt = -4f, Rise = 12f;

        RectTransform root, card;
        CanvasGroup group;
        TMP_Text title, detail, eyebrow;
        Outline border;
        Image rule;
        Coroutine animation;

        void Awake()
        {
            EnsureBuilt();
            HideInstant();
        }

        public void Show(InspectResult r, Vector2 anchoredPosition)
        {
            if (r == null) { Hide(); return; }
            EnsureBuilt();
            gameObject.SetActive(true);
            StopAnimation();

            Color color = r.Verdict == InspectVerdict.Match ? MatchColor
                : r.Verdict == InspectVerdict.Mismatch ? MismatchColor : UnrelatedColor;
            title.text = r.Title ?? "";
            detail.text = r.Detail ?? "";
            title.color = border.effectColor = rule.color = color;
            eyebrow.color = new Color(color.r, color.g, color.b, 0.8f);
            if (titleFont != null) title.font = titleFont;
            if (detailFont != null) detail.font = eyebrow.font = detailFont;
            Resize();
            root.anchoredPosition = ClampPosition(anchoredPosition);
            root.SetAsLastSibling();
            group.interactable = group.blocksRaycasts = false;

            if (Application.isPlaying)
            {
                group.alpha = 0f;
                card.localScale = Vector3.one * 0.92f;
                card.anchoredPosition = Vector2.down * Rise;
                animation = StartCoroutine(Enter());
                Play(r.Verdict == InspectVerdict.Mismatch ? stamp : click);
            }
            else Rest(1f); // Inspector önizlemesi; oyun dışında coroutine/ses başlatmaz.
        }

        public void Hide()
        {
            if (group == null) return;
            StopAnimation();
            if (Application.isPlaying && gameObject.activeInHierarchy && group.alpha > 0f)
                animation = StartCoroutine(Exit());
            else HideInstant();
        }

        void EnsureBuilt()
        {
            if (card != null) return;
            root = (RectTransform)transform;
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            root.localScale = Vector3.one;
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = group.blocksRaycasts = false;

            card = NewRect("Card", root);
            card.anchorMin = Vector2.zero;
            card.anchorMax = Vector2.one;
            card.sizeDelta = Vector2.zero;
            card.localRotation = Quaternion.Euler(0f, 0f, Tilt);
            var paper = card.gameObject.AddComponent<Image>();
            paper.color = Paper;
            paper.raycastTarget = false;
            border = card.gameObject.AddComponent<Outline>();
            border.effectDistance = new Vector2(2f, -2f);
            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.12f, 0.08f, 0.04f, 0.25f);
            shadow.effectDistance = new Vector2(3f, -5f);

            eyebrow = NewText("Eyebrow", 13f, detailFont);
            eyebrow.text = "BELGE İNCELEMESİ";
            eyebrow.characterSpacing = 3f;
            Place(eyebrow.rectTransform, 16f, 20f);
            title = NewText("Title", 32f, titleFont);
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.enableAutoSizing = true;
            title.fontSizeMin = 22f;
            title.fontSizeMax = 32f;
            Place(title.rectTransform, 38f, 44f);

            var line = NewRect("Rule", card);
            Place(line, 86f, 2f);
            rule = line.gameObject.AddComponent<Image>();
            rule.raycastTarget = false;
            detail = NewText("Detail", 20f, detailFont);
            detail.textWrappingMode = TextWrappingModes.Normal;
            detail.enableAutoSizing = true;
            detail.fontSizeMin = 16f;
            detail.fontSizeMax = 20f;
            detail.color = new Color(0.23f, 0.18f, 0.13f);
        }

        void Resize()
        {
            var parent = root.parent as RectTransform;
            float width = parent != null && parent.rect.width > 0f
                ? Mathf.Min(Width, Mathf.Max(1f, parent.rect.width - 48f)) : Width;
            float detailHeight = Mathf.Clamp(detail.GetPreferredValues(detail.text, width - 2f * Padding, 0f).y, 28f, 128f);
            root.sizeDelta = new Vector2(width, 100f + detailHeight + 20f);
            Place(detail.rectTransform, 100f, detailHeight);
        }

        Vector2 ClampPosition(Vector2 position)
        {
            var parent = root.parent as RectTransform;
            if (parent == null || parent.rect.width <= 0f || parent.rect.height <= 0f) return position;
            float angle = Mathf.Abs(Tilt) * Mathf.Deg2Rad;
            Vector2 size = root.rect.size;
            var extent = new Vector2(size.x * Mathf.Cos(angle) + size.y * Mathf.Sin(angle),
                size.x * Mathf.Sin(angle) + size.y * Mathf.Cos(angle)) * 0.54f + Vector2.one * 12f;
            Vector2 limit = Vector2.Max(Vector2.zero, parent.rect.size * 0.5f - extent);
            return new Vector2(Mathf.Clamp(position.x, -limit.x, limit.x), Mathf.Clamp(position.y, -limit.y, limit.y));
        }

        IEnumerator Enter()
        {
            for (float t = 0f; t < DeskMotion.Base; t += Time.deltaTime)
            {
                float p = DeskMotion.EaseOutBack(t / DeskMotion.Base);
                group.alpha = DeskMotion.EaseOut(t / DeskMotion.Base);
                card.localScale = Vector3.one * Mathf.LerpUnclamped(0.92f, 1f, p);
                card.anchoredPosition = Vector2.down * (Rise * (1f - p));
                yield return null;
            }
            Rest(1f);
            animation = null;
        }

        IEnumerator Exit()
        {
            float alpha = group.alpha;
            Vector3 scale = card.localScale;
            Vector2 position = card.anchoredPosition;
            float duration = DeskMotion.Exit(DeskMotion.Base);
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float p = DeskMotion.EaseIn(t / duration);
                group.alpha = alpha * (1f - p);
                card.localScale = Vector3.Lerp(scale, Vector3.one * 0.96f, p);
                card.anchoredPosition = Vector2.Lerp(position, Vector2.down * Rise, p);
                yield return null;
            }
            Rest(0f);
            animation = null;
        }

        void Play(AudioClip clip)
        {
            if (clip == null) return;
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
            audioSource.PlayOneShot(clip, volume);
        }

        void StopAnimation()
        {
            if (animation != null) StopCoroutine(animation);
            animation = null;
        }

        void HideInstant()
        {
            StopAnimation();
            if (group != null) Rest(0f);
        }

        void Rest(float alpha)
        {
            group.alpha = alpha;
            group.interactable = group.blocksRaycasts = false;
            card.localScale = Vector3.one;
            card.anchoredPosition = Vector2.zero;
        }

        void OnDisable() => HideInstant();

        TMP_Text NewText(string name, float size, TMP_FontAsset font)
        {
            var text = NewRect(name, card).gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.fontSize = size;
            text.alignment = TextAlignmentOptions.Center;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            return text;
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            return rect;
        }

        static void Place(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(-2f * Padding, height);
        }

#if UNITY_EDITOR
        void Reset() => BindEditorAssets();

        void BindEditorAssets()
        {
            if (titleFont == null) titleFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/PaytoneOne-Regular SDF.asset");
            if (detailFont == null) detailFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/FiraSans-Regular SDF.asset");
            if (click == null) click = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/sfx_click.ogg");
            if (stamp == null) stamp = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/sfx_stamp.ogg");
        }

        [ContextMenu("Önizleme/SORUN YOK")]
        void PreviewMatch() => Preview(InspectVerdict.Match, Inspection.TitleMatch, "Ada / Parsel iki belgede aynı");

        [ContextMenu("Önizleme/UYUŞMAZLIK SAPTANDI")]
        void PreviewMismatch() => Preview(InspectVerdict.Mismatch, Inspection.TitleMismatch, "Ada / Parsel farklı: 561/8 ↔ 123/4");

        [ContextMenu("Önizleme/ALAKASIZ")]
        void PreviewUnrelated() => Preview(InspectVerdict.Unrelated, Inspection.TitleUnrelated, "Bu iki bilgi birbiriyle karşılaştırılamaz.");

        void Preview(InspectVerdict verdict, string heading, string explanation)
        {
            BindEditorAssets();
            Show(new InspectResult { Verdict = verdict, Title = heading, Detail = explanation }, root != null ? root.anchoredPosition : Vector2.zero);
        }
#endif
    }
}
