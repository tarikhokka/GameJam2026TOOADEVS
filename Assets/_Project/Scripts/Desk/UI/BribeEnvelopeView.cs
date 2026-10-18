using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    /// <summary>
    /// (A, 17 Ekim gece — Ahmet isteği, memory.md "💰") Masadaki zarfa tıklanınca açılan rüşvet ekranı.
    /// Zarf masadaki yerinden oyuncunun önüne gelip büyür, içindeki para ve tutar görünür; iki seçenek:
    ///  - RÜŞVETİ KABUL ET → onAccept (masa: ctx.Actions.Approve(id, acceptBribe: true))
    ///  - RÜŞVETİ REDDET ya da karartılmış boş alana tıklamak → onDecline (zarf masadaki yerine döner; proje karar bekler;
    ///    zarf masadayken formdaki ONAY zarfla onaydır, RED zarfı da reddeder — Ahmet kararı 18 Ekim ~03:10)
    /// Kendi arayüzünü kurar. Görsel: envelopeArt (Tarık, Art/bribe_envelope_open.png) atanmışsa o, yoksa kodla çizilmiş
    /// geçici zarf + banknotlar. Tutar her durumda kodla yazılır. Kurgusal banknot; gerçek lira tasarımı yok.
    /// Kural bilmez: sadece gösterir ve seçimi bildirir. Tam ekran perde arkadaki masayı kilitler.
    /// Kullanım: masa Canvas'ında tam ekran (stretch) boş bir RectTransform'a ekle; alanları Inspector / builder bağlar.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class BribeEnvelopeView : MonoBehaviour
    {
        [Header("Görsel (boşsa kodla çizilen geçici zarf)")]
        [SerializeField] Sprite envelopeArt;

        [Header("Yazı tipleri")]
        [SerializeField] TMP_FontAsset titleFont;   // Paytone One
        [SerializeField] TMP_FontAsset bodyFont;    // Fira Sans Bold

        [Header("Ses (isteğe bağlı)")]
        [SerializeField] AudioClip openClip;         // sfx_paper
        [SerializeField, Range(0f, 1f)] float volume = 0.8f;

        static readonly Color Ink = new Color(0.23f, 0.18f, 0.13f);
        static readonly Color Paper = new Color(0.96f, 0.92f, 0.84f);
        static readonly Color Kraft = new Color(0.79f, 0.64f, 0.42f);
        static readonly Color KraftDark = new Color(0.68f, 0.52f, 0.32f);
        static readonly Color Note = new Color(0.84f, 0.89f, 0.77f);
        static readonly Color NoteInk = new Color(0.28f, 0.40f, 0.27f);
        const float CardWidth = 760f, CardHeight = 800f;
        // Tarık'ın görselinde ön banknotun boş ovali (görselin oranı olarak): tutar buraya yazılır
        static readonly Vector2 OvalCenter = new Vector2(0.485f, 0.56f);
        static readonly Vector2 OvalSize = new Vector2(0.30f, 0.16f);

        RectTransform root, card, art;
        Image dim;
        CanvasGroup cardGroup;
        TMP_Text amountText;
        Button acceptButton, declineButton;
        AudioSource audioSource;
        Action onAccept, onDecline;
        Vector2 from;
        Coroutine anim;

        public bool IsOpen { get; private set; }

        void Awake()
        {
            EnsureBuilt();
            if (!IsOpen) HideInstant(); // Open nesneyi etkinleştirirken Awake çalışırsa kapanmasın
        }

        /// <summary>Zarfı aç. fromWorld: masadaki zarfın dünya konumu (kart oradan gelir, reddedilince oraya döner).</summary>
        public void Open(int amount, Vector3 fromWorld, Action accept, Action decline)
        {
            EnsureBuilt();
            onAccept = accept;
            onDecline = decline;
            amountText.text = FormatMoney(amount);
            from = root.InverseTransformPoint(fromWorld);
            IsOpen = true;
            gameObject.SetActive(true);
            SetButtons(true);
            Play(openClip);
            Run(Animate(from, Vector2.zero, 0.25f, 1f, 0f, 1f, DeskMotion.Slow, true, null));
        }

        /// <summary>Seçim yapılmadan kapat (yeni müteahhit, gün sonu). Geri çağrı yapılmaz.</summary>
        public void Close(bool instant = false)
        {
            if (!IsOpen && !instant) return;
            IsOpen = false;
            onAccept = onDecline = null;
            if (instant || !isActiveAndEnabled) { HideInstant(); return; }
            Run(Animate(card.anchoredPosition, from, 1f, 0.25f, dim.color.a / DimAlpha, 0f, DeskMotion.Exit(DeskMotion.Base), false, HideInstant));
        }

        void Choose(bool accepted)
        {
            if (!IsOpen) return;
            IsOpen = false;
            SetButtons(false);
            var callback = accepted ? onAccept : onDecline;
            onAccept = onDecline = null;
            // Kabul: zarf oyuncuya (aşağı) kayar; ret: masadaki yerine döner
            Vector2 target = accepted ? new Vector2(0f, -root.rect.height * 0.6f) : from;
            Run(Animate(card.anchoredPosition, target, 1f, accepted ? 0.6f : 0.25f, 1f, 0f, DeskMotion.Exit(DeskMotion.Slow), false, () =>
            {
                HideInstant();
                callback?.Invoke();
            }));
        }

        // ---------- Hareket ----------

        const float DimAlpha = 0.75f;

        IEnumerator Animate(Vector2 p0, Vector2 p1, float s0, float s1, float d0, float d1, float duration, bool arriving, Action done)
        {
            for (float t = 0f; t < duration; t += DeskMotion.Dt)
            {
                float k = t / duration;
                float move = arriving ? DeskMotion.EaseOutBack(k) : DeskMotion.EaseIn(k);
                card.anchoredPosition = Vector2.LerpUnclamped(p0, p1, move);
                card.localScale = Vector3.one * Mathf.LerpUnclamped(s0, s1, move);
                float fade = arriving ? DeskMotion.EaseOut(k) : k;
                SetDim(Mathf.Lerp(d0, d1, fade));
                cardGroup.alpha = arriving ? Mathf.Clamp01(k * 3f) : 1f - DeskMotion.EaseIn(k);
                yield return null;
            }
            card.anchoredPosition = p1;
            card.localScale = Vector3.one * s1;
            SetDim(d1);
            cardGroup.alpha = arriving ? 1f : 0f;
            anim = null;
            done?.Invoke();
        }

        void Run(IEnumerator routine)
        {
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(routine);
        }

        void SetDim(float amount) => dim.color = new Color(0f, 0f, 0f, DimAlpha * Mathf.Clamp01(amount));

        void HideInstant()
        {
            if (anim != null) { StopCoroutine(anim); anim = null; }
            IsOpen = false;
            if (dim != null) SetDim(0f);
            if (cardGroup != null) cardGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        void SetButtons(bool on)
        {
            acceptButton.interactable = on;
            declineButton.interactable = on;
            dim.raycastTarget = true; // açıkken ve kapanırken masayı kilitler
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

        static string FormatMoney(int amount) =>
            amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) + " TL";

        // ---------- Kurulum ----------

        void EnsureBuilt()
        {
            if (card != null) return;
            root = (RectTransform)transform;

            dim = NewImage("Dim", root, new Color(0f, 0f, 0f, 0f));
            Stretch(dim.rectTransform);
            dim.raycastTarget = true;
            // (Tarık, Ahmet kararı 18 Ekim ~03:10) Karartılmış boş alana tıklamak = RÜŞVETİ REDDET.
            var dimButton = dim.gameObject.AddComponent<Button>();
            dimButton.transition = Selectable.Transition.None;
            dimButton.onClick.AddListener(() => Choose(false));

            card = NewRect("Card", root);
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(CardWidth, CardHeight);
            cardGroup = card.gameObject.AddComponent<CanvasGroup>();
            // Kartın üstüne tıklamak boş alan sayılmasın (görünmez tutucu; düğmeler bunun üstünde)
            var cardHit = card.gameObject.AddComponent<Image>();
            cardHit.color = Color.clear;
            cardHit.raycastTarget = true;

            art = NewRect("Art", card);
            if (envelopeArt != null)
            {
                // Görselin oranıyla 640x580'e sığdır; tutar ön banknotun boş ovaline
                float aspect = envelopeArt.rect.width / envelopeArt.rect.height;
                var size = aspect >= 640f / 580f ? new Vector2(640f, 640f / aspect) : new Vector2(580f * aspect, 580f);
                Place(art, new Vector2(0.5f, 1f), new Vector2(0f, -10f - size.y * 0.5f), size);
                var img = NewImage("Envelope", art, Color.white);
                img.sprite = envelopeArt;
                Stretch(img.rectTransform);
                amountText = NewText("Amount", art, "", 52f, titleFont, NoteInk);
                var rt = amountText.rectTransform;
                rt.anchorMin = rt.anchorMax = OvalCenter;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.Scale(size, OvalSize);
                rt.localRotation = Quaternion.Euler(0f, 0f, -3f); // banknot hafif eğik
            }
            else
            {
                Place(art, new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(680f, 420f));
                BuildPlaceholder(art);
                amountText = NewText("Amount", art, "", 64f, titleFont, NoteInk);
                Place(amountText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(460f, 90f));
            }
            amountText.fontStyle = FontStyles.Bold;

            var caption = NewText("Caption", card, "Masaya bırakılan zarf", 24f, bodyFont, Paper);
            Place(caption.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(700f, 36f));

            acceptButton = NewButton("Accept", card, "RÜŞVETİ KABUL ET", new Vector2(-180f, 70f), Ink, Paper);
            declineButton = NewButton("Decline", card, "RÜŞVETİ REDDET", new Vector2(180f, 70f), Paper, Ink);
            acceptButton.onClick.AddListener(() => Choose(true));
            declineButton.onClick.AddListener(() => Choose(false));

            var hint = NewText("Hint", card, "Reddedersen dosya masada kalır; kararı yine ONAY / RED ile verirsin.", 20f, bodyFont,
                new Color(Paper.r, Paper.g, Paper.b, 0.8f));
            Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(720f, 30f));
        }

        // Geçici görsel: arkada kraft zarf, içinden çıkan üç banknot, önde zarfın cebi
        void BuildPlaceholder(RectTransform parent)
        {
            var back = NewImage("EnvelopeBack", parent, KraftDark);
            Place(back.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(600f, 250f));
            AddOutline(back);

            float[] tilts = { -7f, 4f, -1.5f };
            float[] offsets = { -24f, 22f, 0f };
            for (int i = 0; i < tilts.Length; i++)
            {
                var note = NewImage($"Note{i}", parent, Note);
                Place(note.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(offsets[i], 60f + i * 6f), new Vector2(520f, 230f));
                note.rectTransform.localRotation = Quaternion.Euler(0f, 0f, tilts[i]);
                AddOutline(note);
                var inner = NewImage("Inner", note.rectTransform, new Color(NoteInk.r, NoteInk.g, NoteInk.b, 0.18f));
                Stretch(inner.rectTransform, 14f);
                var corner = NewText("TL", note.rectTransform, "TL", 28f, titleFont, NoteInk);
                Place(corner.rectTransform, new Vector2(0f, 1f), new Vector2(46f, -32f), new Vector2(70f, 40f));
                var corner2 = NewText("TL2", note.rectTransform, "TL", 28f, titleFont, NoteInk);
                Place(corner2.rectTransform, new Vector2(1f, 0f), new Vector2(-46f, 32f), new Vector2(70f, 40f));
            }

            var pocket = NewImage("EnvelopeFront", parent, Kraft);
            Place(pocket.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 95f), new Vector2(620f, 190f));
            AddOutline(pocket);
            var flap = NewImage("Fold", pocket.rectTransform, new Color(KraftDark.r, KraftDark.g, KraftDark.b, 0.6f));
            Place(flap.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(620f, 8f));
        }

        static void AddOutline(Graphic g)
        {
            var o = g.gameObject.AddComponent<Outline>();
            o.effectColor = Ink;
            o.effectDistance = new Vector2(3f, -3f);
        }

        Button NewButton(string name, RectTransform parent, string label, Vector2 position, Color fill, Color text)
        {
            var img = NewImage(name, parent, fill);
            Place(img.rectTransform, new Vector2(0.5f, 0f), position, new Vector2(330f, 72f));
            img.raycastTarget = true;
            AddOutline(img);
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.93f, 0.8f);
            colors.pressedColor = new Color(0.8f, 0.75f, 0.65f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.6f);
            button.colors = colors;
            var t = NewText("Label", img.rectTransform, label, 26f, bodyFont, text);
            Stretch(t.rectTransform);
            t.fontStyle = FontStyles.Bold;
            return button;
        }

        TMP_Text NewText(string name, RectTransform parent, string text, float size, TMP_FontAsset font, Color color)
        {
            var t = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.enableAutoSizing = true;
            t.fontSizeMin = size * 0.6f;
            t.fontSizeMax = size;
            t.raycastTarget = false;
            return t;
        }

        static Image NewImage(string name, RectTransform parent, Color color)
        {
            var img = NewRect(name, parent).gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            return rect;
        }

        static void Stretch(RectTransform rt, float inset = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }

        static void Place(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

#if UNITY_EDITOR
        void Reset()
        {
            if (titleFont == null) titleFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/PaytoneOne-Regular SDF.asset");
            if (bodyFont == null) bodyFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/FiraSans-Bold SDF.asset");
            if (openClip == null) openClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/sfx_paper.wav");
            if (envelopeArt == null) envelopeArt = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/bribe_envelope_open.png");
        }
#endif
    }
}
