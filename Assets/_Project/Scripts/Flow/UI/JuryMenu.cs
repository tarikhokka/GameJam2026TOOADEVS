using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    /// <summary>
    /// (EEE, 18 Ekim) Jüri menüsü: oyunda J tuşu istenen sahneye atlar (4 son, rüşvet, hediye, kabadayı).
    /// Sahneye elle konmaz: GameManager olan her sahnede kendiliğinden kurulur (Game.unity'ye dokunmadan).
    /// GameManager'daki demo tuşları (F9/F10) kapalıysa bu da kapalıdır.
    /// İlk gün öğretici kapanınca üstte bir kez "J'ye bas" ipucu çıkar (JuryHooks.TutorialFinished, Desk/TutorialView).
    /// Kendi tuvali en üstte ama Esc menüsünün altında; Esc menüsü açıkken J çalışmaz.
    /// </summary>
    public class JuryMenu : MonoBehaviour
    {
        static readonly (string label, string note, JuryScene scene)[] Items =
        {
            ("İyi son", "Ayakta Kalanlar", JuryScene.EndingGood),
            ("Kötü son", "Hüküm (hapis)", JuryScene.EndingBad),
            ("Enkazın Gölgesi", "Orta son: yıkılan bina", JuryScene.EndingCollapsed),
            ("İflas", "Orta son: Kapanan Kapı", JuryScene.EndingClosed),
            ("Rüşvet sahnesi", "İNCELE ile bir uyuşmazlık bul, zarf gelir", JuryScene.Bribe),
            ("Hediye sahnesi", "Dürüst ustanın projesini ONAYLA", JuryScene.Gift),
            ("Kabadayı sahnesi", "Projeyi REDDET, ofiste bir şey kırılır", JuryScene.Thug),
        };

        GameManager _game;
        TMP_FontAsset _titleFont, _boldFont, _bodyFont;
        Sprite _button, _paper;
        RectTransform _menu;
        CanvasGroup _tag, _hint;
        RectTransform _hintRt;
        float _tagTime = -1f;
        float _hintTime = -1f; // >= 0: büyük ipucu açık (süre), -2: köşeye kayıyor, -1: kapalı
        float _shrink;
        TextMeshProUGUI _status;
        bool _hintShown;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            SceneManager.sceneLoaded += (s, m) => Attach();
            Attach();
        }

        static void Attach()
        {
            var game = FindAnyObjectByType<GameManager>();
            if (game == null || !game.DemoKeys || FindAnyObjectByType<JuryMenu>() != null) return;
            var go = new GameObject("JuryMenu");
            go.AddComponent<JuryMenu>()._game = game;
        }

        void Awake()
        {
            foreach (var f in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (f.name.StartsWith("PaytoneOne")) _titleFont = f;
                else if (f.name.StartsWith("FiraSans-Bold")) _boldFont = f;
                else if (f.name.StartsWith("FiraSans-Regular")) _bodyFont = f;
            }
            foreach (var s in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (s.name == "btn_menu") _button = s;
                else if (s.name == "paper") _paper = s;
            }
            Build();
            JuryHooks.TutorialFinished += ShowHint;
        }

        void OnDestroy() => JuryHooks.TutorialFinished -= ShowHint;

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000; // Esc menüsü (32760) üstte kalsın
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            // İpucu: üst barın (80 px) altında, ortada; altın çerçeveli koyu kart. Süreyle kaybolmaz; köşedeki X ile
            // kapanınca küçülerek sol alt köşeye kayar ve orada küçük etikete dönüşür (Tarık, 18 Ekim).
            var hint = UiKit.Image(transform, "Hint", null, new Color(0.86f, 0.62f, 0.25f, 1f), // altın çerçeve
                new Vector2(0.28f, 0.66f), new Vector2(0.72f, 0.84f));
            _hintRt = hint.rectTransform;
            _hint = hint.gameObject.AddComponent<CanvasGroup>();
            _hint.alpha = 0f;
            var hintShadow = hint.gameObject.AddComponent<Shadow>();
            hintShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            hintShadow.effectDistance = new Vector2(6, -6);
            var inner = UiKit.Image(hint.transform, "Inner", null, new Color(0.08f, 0.06f, 0.045f, 0.97f), Vector2.zero, Vector2.one);
            inner.rectTransform.offsetMin = new Vector2(5, 5);
            inner.rectTransform.offsetMax = new Vector2(-5, -5);
            var hintTitle = UiKit.Text(inner.transform, "Title", "JÜRİ İÇİN", 46, UiKit.Paper, TextAlignmentOptions.Center,
                new Vector2(0f, 0.52f), new Vector2(1f, 0.94f));
            UiKit.StyleTitle(hintTitle, _titleFont, 46);
            var hintBody = UiKit.Text(inner.transform, "Body",
                "İstediğin sahneye gitmek için <color=" + UiKit.GoldHex + "><b>J</b></color> tuşuna bas.", 34, UiKit.Paper,
                TextAlignmentOptions.Center, new Vector2(0f, 0.1f), new Vector2(1f, 0.52f));
            UiKit.StyleInfo(hintBody, _boldFont, 34);
            var hintClose = CloseX(inner.rectTransform, 52f, () => { if (_hintTime >= 0f) _hintTime = -2f; }); // -2: köşeye kayma başlar
            hintClose.anchorMin = hintClose.anchorMax = hintClose.pivot = new Vector2(1f, 1f);
            hintClose.anchoredPosition = new Vector2(-6f, -6f);
            hint.gameObject.SetActive(false);

            // Küçük etiket (sol alt köşe, soluk): tıklayınca menü açılır, X ile tamamen kapanır.
            var tag = UiKit.Image(transform, "Tag", null, new Color(0.08f, 0.06f, 0.045f, 0.75f), Vector2.zero, Vector2.zero);
            var tagRt = tag.rectTransform;
            tagRt.pivot = Vector2.zero;
            tagRt.sizeDelta = new Vector2(196f, 34f);
            tagRt.anchoredPosition = new Vector2(10f, 10f);
            tag.raycastTarget = true;
            var tagButton = tag.gameObject.AddComponent<Button>();
            tagButton.targetGraphic = tag;
            tagButton.onClick.AddListener(() => { if (!PauseMenu.InputBlocked) SetOpen(true); });
            var tagText = UiKit.Text(tagRt, "Text", "Jüri menüsü: <b>J</b>", 18, new Color(1f, 0.96f, 0.88f, 0.85f),
                TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.one);
            if (_boldFont != null) tagText.font = _boldFont;
            tagText.rectTransform.offsetMin = new Vector2(10f, 0f);
            tagText.rectTransform.offsetMax = new Vector2(-30f, 0f);
            CloseX(tagRt, 30f, () => { _tagTime = -1f; _tag.gameObject.SetActive(false); });
            _tag = tag.gameObject.AddComponent<CanvasGroup>();
            _tag.alpha = 0f;
            tag.gameObject.SetActive(false);

            // Menü: tam ekran karartma + ortada kağıt kart.
            _menu = UiKit.Stretch(transform, "Menu");
            var shade = UiKit.Image(_menu, "Shade", null, new Color(0f, 0f, 0f, 0.6f), Vector2.zero, Vector2.one);
            shade.raycastTarget = true; // arkadaki masaya tıklanmasın
            var card = UiKit.Card(_menu, "Card", _paper, new Vector2(0.31f, 0.06f), new Vector2(0.69f, 0.94f));
            card.raycastTarget = true;

            var title = UiKit.Text(card.transform, "Title", "JÜRİ MENÜSÜ", 54, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f));
            UiKit.StyleInk(title, _titleFont); // kağıt üstünde koyu mürekkep (diğer kağıt kartlar gibi)
            var sub = UiKit.Text(card.transform, "Sub", "Bir sahne seç; oyun oraya atlar.", 24, UiKit.Ink,
                TextAlignmentOptions.Center, new Vector2(0.05f, 0.835f), new Vector2(0.95f, 0.88f));
            UiKit.StyleInk(sub, _bodyFont);

            float top = 0.82f, row = 0.098f;
            for (int i = 0; i < Items.Length; i++)
            {
                var item = Items[i];
                float y1 = top - i * row, y0 = y1 - row + 0.012f;
                var btn = UiKit.Button(card.transform, "Item" + i, item.label, () => Jump(item.scene),
                    new Vector2(0.1f, y0 + 0.03f), new Vector2(0.9f, y1), 30f);
                Wood(btn, 30f);
                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                label.rectTransform.anchorMin = new Vector2(0f, 0.3f);
                var note = UiKit.Text(btn.transform, "Note", item.note, 18, new Color(1f, 0.9f, 0.72f, 0.9f),
                    TextAlignmentOptions.Center, new Vector2(0f, 0.04f), new Vector2(1f, 0.38f));
                if (_bodyFont != null) note.font = _bodyFont;
            }

            _status = UiKit.Text(card.transform, "Status", "", 22, UiKit.Accent, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.145f));
            UiKit.StyleInk(_status, _bodyFont);
            _status.color = UiKit.Accent;

            var close = UiKit.Button(card.transform, "Close", "Kapat (J)", () => SetOpen(false),
                new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.1f), 28f);
            Wood(close, 28f);

            _menu.gameObject.SetActive(false);
        }

        void Wood(Button btn, float size)
        {
            var img = btn.targetGraphic as Image;
            if (_button != null && img != null) { img.sprite = _button; img.color = Color.white; }
            foreach (var t in btn.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (_boldFont != null) t.font = _boldFont;
                t.color = UiKit.Paper;
            }
        }

        void ShowHint()
        {
            if (_hintShown) return;
            _hintShown = true;
            _hintRt.gameObject.SetActive(true);
            _hintTime = 0f;
        }

        void ShowTag()
        {
            _tag.gameObject.SetActive(true);
            _tagTime = 0f;
        }

        // Sağ köşede küçük X (yazı tiplerinde ✕ yok: düz X).
        RectTransform CloseX(RectTransform parent, float size, System.Action onClose)
        {
            var img = UiKit.Image(parent, "Close", null, new Color(1f, 1f, 1f, 0.001f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));
            var rt = img.rectTransform;
            rt.pivot = new Vector2(1f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            img.raycastTarget = true;
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClose());
            var x = UiKit.Text(rt, "X", "X", size * 0.55f, new Color(1f, 0.96f, 0.88f, 0.7f), TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            if (_boldFont != null) x.font = _boldFont;
            return rt;
        }

        void SetOpen(bool open)
        {
            _menu.gameObject.SetActive(open);
            _status.text = "";
            if (open) _menu.SetAsLastSibling();
        }

        void Jump(JuryScene scene)
        {
            if (_game == null) _game = FindAnyObjectByType<GameManager>();
            if (_game != null && _game.JuryJump(scene)) SetOpen(false);
            else _status.text = "Bu sahne şu an açılamadı, tekrar dene.";
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.jKey.wasPressedThisFrame && !PauseMenu.InputBlocked)
                SetOpen(!_menu.gameObject.activeSelf);

            // Büyük ipucu: 0,4 sn'de büyüyerek belirir, X'e basılana kadar hafifçe nabız atar (süreyle kaybolmaz).
            if (_hintTime >= 0f)
            {
                _hintTime += Time.unscaledDeltaTime;
                _hint.alpha = Mathf.Clamp01(_hintTime / 0.4f);
                float s = _hintTime < 0.4f ? Mathf.Lerp(0.9f, 1f, _hintTime / 0.4f) : 1f + 0.025f * Mathf.Sin(_hintTime * 4f);
                _hintRt.localScale = new Vector3(s, s, 1f);
                _shrink = 0f;
            }
            // X: 0,45 sn'de küçülüp sol alt köşeye kayar, sonra küçük etiket belirir.
            else if (_hintTime <= -2f)
            {
                _shrink += Time.unscaledDeltaTime / 0.45f;
                float k = Mathf.Clamp01(_shrink);
                float e = k * k;
                var canvas = (RectTransform)transform;
                Vector2 from = Vector2.zero; // ipucu ekranın ortasına göre konumlu (çapa ortası)
                Vector2 to = new Vector2(-canvas.rect.width * 0.5f + 110f, -canvas.rect.height * 0.75f + 30f);
                _hintRt.anchoredPosition = Vector2.Lerp(from, to, e);
                float s = Mathf.Lerp(1f, 0.15f, e);
                _hintRt.localScale = new Vector3(s, s, 1f);
                _hint.alpha = 1f - e;
                if (k >= 1f)
                {
                    _hintTime = -1f;
                    _hintRt.gameObject.SetActive(false);
                    ShowTag();
                }
            }

            // Etiket 0,8 sn'de belirir, sonra sabit kalır.
            if (_tagTime >= 0f)
            {
                _tagTime += Time.unscaledDeltaTime;
                _tag.alpha = Mathf.Clamp01(_tagTime / 0.8f);
                if (_tagTime >= 0.8f) _tagTime = -1f;
            }
        }
    }
}
