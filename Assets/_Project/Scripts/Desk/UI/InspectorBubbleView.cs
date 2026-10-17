using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    /// <summary>
    /// Denetçinin yalnızca konuşma sunumu. Boş bir UI nesnesine eklenir; kendi balonunu kurar.
    /// Konumlandırma, diyalog sırası, düğme kilitleri ve zarf teklifi çağıran masa yöneticisine aittir.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public sealed class InspectorBubbleView : MonoBehaviour, IPointerClickHandler
    {
        [Header("Yazı tipleri — Fira Sans")]
        [SerializeField] TMP_FontAsset bodyFont;
        [SerializeField] TMP_FontAsset labelFont;
        [SerializeField, Min(240f)] float preferredWidth = 720f;

        [Header("Harf harf yazı")]
        [SerializeField, Min(1f)] float charsPerSecond = 40f;
        [SerializeField, Min(1)] int soundEvery = 2;
        [SerializeField, Min(0f)] float sentencePause = 0.18f;
        [SerializeField, Min(0f)] float commaPause = 0.08f;

        [Header("Yazı sesi — ayrı 2D kaynak; müteahhitle paylaşma")]
        [SerializeField] AudioSource typingSource;
        [SerializeField] AudioClip typingClip;
        [SerializeField, Range(0f, 1f)] float typingVolume = 0.45f;

        public bool IsTyping { get; private set; }

        /// <summary>Yazı bitmişken sol tıklama: çağıran bir sonraki satıra geçebilir.
        /// Yazı sürerken ilk tıklama yalnızca CompleteTyping yapar, bu olayı tetiklemez.</summary>
        public event Action ContinueRequested;

        static readonly Color Paper = new Color(1f, 0.97f, 0.89f);
        static readonly Color Ink = new Color(0.23f, 0.18f, 0.13f);
        const float Padding = 24f;
        const float TypingStopDelay = 0.25f;

        RectTransform root, bubble;
        CanvasGroup group;
        TMP_Text body, label, hint;
        Coroutine typing, motion;
        bool shown, playingTyping;
        float lastTypedAt;

        void Awake()
        {
            EnsureBuilt();
            HideInstant();
        }

        public void Show(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) { Hide(); return; }
            EnsureBuilt();
            gameObject.SetActive(true);
            CancelTyping();
            StopMotion();
            shown = true;
            if (bodyFont != null) body.font = hint.font = bodyFont;
            if (labelFont != null) label.font = labelFont;
            body.text = text;
            body.maxVisibleCharacters = 0;
            Resize(); // Tam metne göre bir kez ölç: yazılırken balon büyümesin.
            body.ForceMeshUpdate();
            root.SetAsLastSibling();
            group.interactable = group.blocksRaycasts = true;

            if (!Application.isPlaying)
            {
                body.maxVisibleCharacters = int.MaxValue;
                Rest(1f);
                RefreshHint();
                return;
            }
            group.alpha = 0f;
            bubble.localScale = Vector3.one * 0.92f;
            motion = StartCoroutine(Enter());
            if (body.textInfo.characterCount > 0)
            {
                IsTyping = true;
                typing = StartCoroutine(TypeLine());
            }
            RefreshHint();
        }

        public void CompleteTyping()
        {
            if (!shown) return;
            CancelTyping();
            body.maxVisibleCharacters = int.MaxValue;
            RefreshHint();
        }

        public void Hide()
        {
            shown = false;
            CancelTyping();
            StopMotion();
            if (group == null) return;
            group.interactable = group.blocksRaycasts = false;
            if (Application.isPlaying && gameObject.activeInHierarchy && group.alpha > 0f)
                motion = StartCoroutine(Exit());
            else HideInstant();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!shown || Time.timeScale <= 0f || eventData.button != PointerEventData.InputButton.Left) return;
            if (IsTyping) CompleteTyping();
            else ContinueRequested?.Invoke();
        }

        IEnumerator TypeLine()
        {
            var info = body.textInfo;
            int total = info.characterCount, visible = 0, voiced = 0;
            float budget = 0f, pauseLeft = 0f;
            while (visible < total)
            {
                float dt = Time.deltaTime;
                if (pauseLeft > 0f) pauseLeft = Mathf.Max(0f, pauseLeft - dt);
                else
                {
                    budget += dt * Mathf.Max(1f, charsPerSecond);
                    while (budget >= 1f && visible < total)
                    {
                        budget -= 1f;
                        char c = info.characterInfo[visible].character;
                        body.maxVisibleCharacters = ++visible;
                        if (char.IsLetterOrDigit(c) && voiced++ % Mathf.Max(1, soundEvery) == 0) PlayTyping();
                        pauseLeft = c == '.' || c == '!' || c == '?' ? sentencePause
                            : c == ',' || c == ';' || c == ':' ? commaPause : 0f;
                        if (pauseLeft > 0f && visible < total) { budget = 0f; break; }
                    }
                }
                if (playingTyping && Time.time - lastTypedAt > TypingStopDelay) StopTypingSound();
                yield return null;
            }
            typing = null;
            IsTyping = false;
            StopTypingSound();
            RefreshHint();
        }

        void PlayTyping()
        {
            if (typingClip == null) return;
            if (typingSource == null)
            {
                typingSource = gameObject.AddComponent<AudioSource>();
                typingSource.playOnAwake = false;
                typingSource.spatialBlend = 0f;
            }
            lastTypedAt = Time.time;
            if (playingTyping && typingSource.isPlaying) return;
            typingSource.clip = typingClip;
            typingSource.loop = true;
            typingSource.volume = typingVolume;
            typingSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            typingSource.time = UnityEngine.Random.Range(0f, Mathf.Max(0f, typingClip.length - 0.5f));
            typingSource.Play();
            playingTyping = true;
        }

        void StopTypingSound()
        {
            if (playingTyping && typingSource != null) typingSource.Stop();
            playingTyping = false;
        }

        void CancelTyping()
        {
            if (typing != null) StopCoroutine(typing);
            typing = null;
            IsTyping = false;
            StopTypingSound();
        }

        void EnsureBuilt()
        {
            if (bubble != null) return;
            root = (RectTransform)transform;
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = group.blocksRaycasts = false;

            bubble = NewRect("Bubble", root);
            bubble.anchorMin = Vector2.zero;
            bubble.anchorMax = Vector2.one;
            bubble.sizeDelta = Vector2.zero;
            var tail = NewRect("Tail", bubble);
            tail.anchorMin = tail.anchorMax = new Vector2(0.14f, 0f);
            tail.anchoredPosition = new Vector2(0f, -7f);
            tail.sizeDelta = new Vector2(22f, 22f);
            tail.localRotation = Quaternion.Euler(0f, 0f, 45f);
            AddPaper(tail, false);

            var panel = NewRect("Paper", bubble);
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.sizeDelta = Vector2.zero;
            AddPaper(panel, true); // Sadece balonun kendisi tıklama alır, tam ekran perde yok.
            var shadow = panel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.1f, 0.06f, 0.03f, 0.22f);
            shadow.effectDistance = new Vector2(3f, -5f);

            var tag = NewRect("Nameplate", bubble);
            tag.anchorMin = tag.anchorMax = new Vector2(0f, 1f);
            tag.pivot = new Vector2(0f, 1f);
            tag.anchoredPosition = new Vector2(Padding, -14f);
            tag.sizeDelta = new Vector2(140f, 28f);
            var tagImage = tag.gameObject.AddComponent<Image>();
            tagImage.color = Ink;
            tagImage.raycastTarget = false;
            label = Text("Label", tag, labelFont, 18f);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.sizeDelta = Vector2.zero;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Paper;
            label.text = "DENETÇİ";
            label.characterSpacing = 2f;

            body = Text("Text", bubble, bodyFont, 26f);
            body.color = Ink;
            body.enableAutoSizing = true;
            body.fontSizeMin = 20f;
            body.fontSizeMax = 26f;
            body.textWrappingMode = TextWrappingModes.Normal;
            hint = Text("Hint", bubble, bodyFont, 15f);
            hint.color = new Color(Ink.r, Ink.g, Ink.b, 0.7f);
            hint.alignment = TextAlignmentOptions.MidlineRight;
        }

        void Resize()
        {
            var parent = root.parent as RectTransform;
            float width = parent != null && parent.rect.width > 0f
                ? Mathf.Min(preferredWidth, Mathf.Max(1f, parent.rect.width - 48f)) : preferredWidth;
            float height = Mathf.Clamp(body.GetPreferredValues(body.text, width - 2f * Padding, 0f).y, 32f, 188f);
            root.sizeDelta = new Vector2(width, 58f + height + 44f);
            Place(body.rectTransform, 58f, height);
            Place(hint.rectTransform, 58f + height + 10f, 20f);
        }

        void RefreshHint()
        {
            if (hint != null) hint.text = IsTyping ? "Tıkla: yazıyı tamamla" : "Devam etmek için tıkla";
        }

        IEnumerator Enter()
        {
            for (float t = 0f; t < DeskMotion.Fast; t += Time.deltaTime)
            {
                float k = t / DeskMotion.Fast;
                group.alpha = DeskMotion.EaseOut(k);
                bubble.localScale = Vector3.one * Mathf.LerpUnclamped(0.92f, 1f, DeskMotion.EaseOutBack(k, 1.6f));
                yield return null;
            }
            Rest(1f);
            motion = null;
        }

        IEnumerator Exit()
        {
            float alpha = group.alpha;
            Vector3 scale = bubble.localScale;
            float duration = DeskMotion.Exit(DeskMotion.Fast);
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = DeskMotion.EaseIn(t / duration);
                group.alpha = alpha * (1f - k);
                bubble.localScale = Vector3.Lerp(scale, Vector3.one * 0.96f, k);
                yield return null;
            }
            Rest(0f);
            motion = null;
        }

        void StopMotion()
        {
            if (motion != null) StopCoroutine(motion);
            motion = null;
        }

        void HideInstant()
        {
            shown = false;
            CancelTyping();
            StopMotion();
            if (group == null) return;
            Rest(0f);
            group.interactable = group.blocksRaycasts = false;
        }

        void Rest(float alpha)
        {
            group.alpha = alpha;
            bubble.localScale = Vector3.one;
        }

        void OnDisable() => HideInstant();

        static RectTransform NewRect(string name, Transform parent)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            return rect;
        }

        static void AddPaper(RectTransform rect, bool raycast)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Paper;
            image.raycastTarget = raycast;
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = Ink;
            outline.effectDistance = new Vector2(2f, -2f);
        }

        static TMP_Text Text(string name, Transform parent, TMP_FontAsset font, float size)
        {
            var text = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.fontSize = size;
            text.richText = false;
            text.raycastTarget = false;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
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
        void Reset()
        {
            BindEditorAssets();
            var rect = (RectTransform)transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-360f, 130f);
        }

        void BindEditorAssets()
        {
            if (bodyFont == null) bodyFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/FiraSans-Regular SDF.asset");
            if (labelFont == null) labelFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/FiraSans-Bold SDF.asset");
            if (typingClip == null) typingClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/sfx_typing.ogg");
        }

        [ContextMenu("Önizleme/Denetçi konuşması")]
        void Preview()
        {
            BindEditorAssets();
            Show("Kolon kalınlığı 33 cm; 4-6 katlı binada en az 35 cm olmalı.");
        }
#endif
    }
}
