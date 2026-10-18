using System;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    /// <summary>
    /// (Tarık, 18 Ekim) İlk açılış rehberi: bu bilgisayarda oyun ilk kez oynanırken 5 kartlık öğreticinin yerine masadaki
    /// gerçek öğeleri sırayla gösterir (parlayan çerçeve + ok + kısa metin). Bilgi adımları "Anladım" ile, eylem adımları
    /// (belgeyi aç, İNCELE, karşılaştır, kaşe, iade) oyuncu yapınca ilerler. "Rehberi atla" her an var (takılma yok).
    /// Masa olaylarını sadece dinler (DeskScreen / InspectController / StampTrayView / DraggableSheet / PaperWindow).
    /// Her yeni oyunun 1. gününde çıkar; sonraki günlerde NASIL OYNANIR eski kartları açar.
    /// Ayrıca ilk zarfta bir kez rüşvet kartı (BribeHint).
    /// </summary>
    public sealed class FirstRunCoach : MonoBehaviour
    {
        static bool _bribeShownThisGame;

        class Step
        {
            public Func<Transform> Target; public string Title, Body; public Func<bool> Done;
            public Step(Func<Transform> target, string title, string body, Func<bool> done = null)
            { Target = target; Title = title; Body = body; Done = done; }
        }

        Step[] Steps;
        const string DropText = "Şimdi kaşeyi formdaki sarı <b>KAŞE</b> alanının üstüne getir ve bırak.";
        bool _bribeMode;

        // Masa bileşenleri (dinlenir, değiştirilmez)
        DeskScreen _screen;
        InspectController _inspect;
        StampTrayView _tray;
        bool _compared;
        int _handedBack;

        void BindDesk()
        {
            _screen = _desk.GetComponent<DeskScreen>();
            _inspect = _desk.GetComponentInChildren<InspectController>(true);
            _tray = _desk.GetComponentInChildren<StampTrayView>(true);
            if (_inspect != null) _inspect.Compared += OnCompared;
            foreach (var sheet in _desk.GetComponentsInChildren<DraggableSheet>(true)) sheet.HandBackDropped += OnHandBack;
        }

        void OnCompared(InspectOutcome _) => _compared = true;
        void OnHandBack() => _handedBack++;

        void OnDestroy()
        {
            if (_inspect != null) _inspect.Compared -= OnCompared;
            if (_desk != null)
                foreach (var sheet in _desk.GetComponentsInChildren<DraggableSheet>(true)) sheet.HandBackDropped -= OnHandBack;
        }

        Transform Named(string name) => Find(name);
        Transform Under(string parent, string child) { var p = Find(parent); return p != null ? FindDeep(p, child) : null; }
        bool WindowOpen(string name) { var w = Find(name); var pw = w != null ? w.GetComponent<PaperWindow>() : null; return pw != null && pw.IsOpen; }

        void MakeTutorialSteps()
        {
            Steps = new[]
            {
                new Step(() => Named("FormIcon"), "Belgeler burada",
                    "Müteahhit belgelerini masanın soluna bıraktı. <b>Yapı Denetim Formu</b>'nun ikonuna tıkla, aç.",
                    () => WindowOpen("FormWindow")),
                new Step(() => Named("LabIcon"), "Lab raporu",
                    "Şimdi <b>Laboratuvar Raporu</b>'nu aç. İki belge aynı binayı anlatmalı.",
                    () => WindowOpen("LabWindow")),
                new Step(() => Named("RulebookWindow"), "Denetçi El Kitabı",
                    "El Kitabı hep açık. <b>HIZLI BAKIŞ</b> binanın kat grubunu kendisi seçer: formdaki her değer <b>yeşil</b> alanda olmalı. Tek kırmızı varsa bina güvensizdir."),
                new Step(() => Named("InspectButton"), "İncele",
                    "Belgeleri karşılaştırmak için <b>İNCELE</b>'ye bas.",
                    () => _inspect != null && _inspect.IsOn),
                new Step(() => Named("FormWindow"), "Karşılaştır",
                    "İki belgede aynı bilgiye tıkla: önce formdaki <b>parsel</b>, sonra lab raporundaki parsel. Oyun <b>SORUN YOK</b> ya da <b>UYUŞMAZLIK</b> der.",
                    () => _compared),
                new Step(() => Named("ExtraTestButton"), "Ek test",
                    "Şüphelendiğin projede <b>EK TEST</b> iste: laboratuvar gerçek değerleri ölçer. Sahte belgedeki yalanı çoğu zaman sadece bu ortaya çıkarır. Ama her test <b>kasadan para</b> götürür, dikkatli kullan."),
                new Step(() => Under("StampTray", "Toggle"), "Kaşe çekmecesi",
                    "Kararını kaşeyle verirsin. <b>KAŞE</b> düğmesine bas, çekmece açılsın.",
                    () => _tray != null && _tray.IsOpen),
                new Step(() => Under("StampTray", "Tray"), "Kaşeyi bas",
                    "Yeşil <b>ONAY</b> ya da kırmızı <b>RED</b> kaşesini tut, formdaki kaşe alanına sürükleyip bırak. Basılan kaşe geri alınmaz.",
                    () => _screen != null && _screen.IsStamped),
                new Step(() => Named("Portrait"), "Belgeleri geri ver",
                    "Şimdi iki belgeyi de tutup müteahhidin üstüne sürükle. İkisi de verilince karar kesinleşir, sıradaki müteahhit gelir.",
                    () => _handedBack >= 2),
            };
        }

        void MakeBribeSteps()
        {
            Steps = new[]
            {
                new Step(EnvelopeOnDesk, "Masada bir zarf var",
                    "Uyuşmazlığı bulunca müteahhit <b>rüşvet</b> teklif etti. Zarfa tıklayıp içindeki parayı görebilirsin.\n" +
                    "Kabul edersen ya da zarf masadayken yeşil <b>ONAY</b> kaşesini basarsan para <b>kasana girer</b>. " +
                    "Ama rüşvetle onayladığın bina yıkılırsa hapse girebilirsin. Kırmızı <b>RED</b> zarfı da reddeder."),
            };
        }

        // Masadaki zarf (aynı adlı başka nesne de var: tıklanabilir olanı seç)
        Transform EnvelopeOnDesk()
        {
            foreach (var b in _desk.GetComponentsInChildren<Button>(false))
                if (b.name == "Envelope") return b.transform;
            return null;
        }

        /// <summary>İlk zarf masaya düştüğünde bir kez (bilgisayar başına) rüşvet kartı.</summary>
        public static void BribeHint(Transform deskRoot)
        {
            if (_bribeShownThisGame) return;
            if (deskRoot == null || FindAnyObjectByType<FirstRunCoach>() != null) return; // ana rehber sürerken üst üste binmesin
            var go = new GameObject("BribeHint");
            var coach = go.AddComponent<FirstRunCoach>();
            coach._desk = deskRoot;
            coach._bribeMode = true;
            coach.Build();
        }

        static readonly Color Gold = new Color(0.98f, 0.74f, 0.36f);
        static readonly Color Card = new Color(0.08f, 0.06f, 0.045f, 0.96f);
        static readonly Color Cream = new Color(1f, 0.96f, 0.88f);

        Transform _desk;
        RectTransform _root, _frame, _arrow, _card;
        TextMeshProUGUI _title, _body, _counter, _next;
        int _step = -1;
        GameObject _skip;
        float _t;

        /// <summary>İlk açılışsa rehberi başlatır ve true döner (o zaman eski kartlar açılmaz).</summary>
        public static bool TryStart(Transform deskRoot)
        {
            // (Tarık) Her yeni oyunun 1. gününde çıkar (bilgisayar başına bir kez değil). Rüşvet kartı da oyun başına bir kez.
            if (deskRoot == null) return false;
            _bribeShownThisGame = false;
            if (FindAnyObjectByType<FirstRunCoach>() != null) return true;
            var go = new GameObject("FirstRunCoach");
            var coach = go.AddComponent<FirstRunCoach>();
            coach._desk = deskRoot;
            coach.Build();
            return true;
        }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 31000; // jüri menüsü (32000) ve Esc menüsü (32760) üstte kalsın
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            _root = (RectTransform)transform;
            BindDesk();
            if (_bribeMode) MakeBribeSteps(); else MakeTutorialSteps();

            TMP_FontAsset title = null, bold = null, body = null;
            foreach (var f in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (f.name.StartsWith("PaytoneOne")) title = f;
                else if (f.name.StartsWith("FiraSans-Bold")) bold = f;
                else if (f.name.StartsWith("FiraSans-Regular")) body = f;
            }

            // Parlayan çerçeve: dört ince kenar (ortası boş, hedef görünür ve tıklanabilir kalır)
            _frame = NewRect("Frame", _root);
            foreach (var (min, max) in new[] {
                (new Vector2(0, 1), new Vector2(1, 1)), (new Vector2(0, 0), new Vector2(1, 0)),
                (new Vector2(0, 0), new Vector2(0, 1)), (new Vector2(1, 0), new Vector2(1, 1)) })
            {
                var edge = Img("Edge", _frame, Gold);
                var rt = edge.rectTransform;
                rt.anchorMin = min; rt.anchorMax = max;
                rt.sizeDelta = new Vector2(min.x == max.x ? 6f : 12f, min.y == max.y ? 6f : 12f);
                rt.anchoredPosition = Vector2.zero;
            }

            // Ok: kodla çizilen üçgen + gövde (yazı tiplerinde ok işareti yok)
            _arrow = NewRect("Arrow", _root);
            _arrow.sizeDelta = new Vector2(70f, 90f);
            var arrowImg = _arrow.gameObject.AddComponent<Image>();
            arrowImg.sprite = ArrowSprite();
            arrowImg.color = Gold;
            arrowImg.raycastTarget = false;
            _arrow.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(3f, -4f);

            // Bilgi kartı
            _card = NewRect("Card", _root);
            _card.sizeDelta = new Vector2(600f, 250f); // yükseklik Show'da metne göre
            var border = _card.gameObject.AddComponent<Image>();
            border.color = Gold;
            border.raycastTarget = false; // kartın altındaki belgeye tıklanabilsin (düğmeler hariç)
            _card.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(6f, -6f);
            var inner = Img("Inner", _card, Card);
            Stretch(inner.rectTransform, 4f);
            _title = Text(inner.transform, "Title", 34f, Cream, title, TextAlignmentOptions.TopLeft);
            Place(_title.rectTransform, 22f, -16f, -90f, 46f);
            _counter = Text(inner.transform, "Counter", 20f, new Color(Cream.r, Cream.g, Cream.b, 0.6f), bold, TextAlignmentOptions.TopRight);
            Place(_counter.rectTransform, -22f, -20f, 80f, 30f, true);
            _body = Text(inner.transform, "Body", 24f, Cream, body, TextAlignmentOptions.TopLeft);
            _body.textWrappingMode = TextWrappingModes.Normal;
            _body.rectTransform.anchorMin = new Vector2(0f, 0f);
            _body.rectTransform.anchorMax = new Vector2(1f, 1f);
            _body.rectTransform.offsetMin = new Vector2(22f, 64f);
            _body.rectTransform.offsetMax = new Vector2(-22f, -66f);

            _next = Button(inner.transform, "Next", "Anladım", bold, Next, new Vector2(1f, 0f), new Vector2(-18f, 14f), new Vector2(170f, 44f), Gold, Card);
            _skip = Button(inner.transform, "Skip", "Rehberi atla", body, Finish, new Vector2(0f, 0f), new Vector2(18f, 14f), new Vector2(170f, 44f),
                new Color(1f, 1f, 1f, 0.08f), new Color(Cream.r, Cream.g, Cream.b, 0.75f)).transform.parent.gameObject;

            _root.gameObject.SetActive(true);
            SetVisible(false); // ilk hedef (belge ikonları) müteahhit oturunca görünür
        }

        void Update()
        {
            _t += Time.unscaledDeltaTime;
            if (_desk == null) { Destroy(gameObject); return; }
            bool deskOn = _desk.gameObject.activeInHierarchy;
            if (_card.gameObject.activeSelf != (deskOn && _step >= 0)) SetVisible(deskOn && _step >= 0);
            if (!deskOn) return; // masa kapalıyken (gün sonu ekranları) gizli bekler
            if (_step < 0)
            {
                // Müteahhit oturup ikonlar gelince başla
                var first = Steps[0].Target();
                if (first != null && first.gameObject.activeInHierarchy) Show(0);
                return;
            }

            var step = Steps[_step];
            if (step.Done != null && step.Done()) { Next(); return; } // oyuncu adımı yaptı
            var target = step.Target();
            // Kaşe eldeyken (çekmece kaşeyi DragLayer'a alır) ok formdaki kaşe alanına geçer
            if (step.Title == "Kaşeyi bas")
            {
                var drag = Under("StampTray", "DragLayer");
                bool holding = drag != null && drag.childCount > 0;
                if (holding) { var stampArea = Named("StampArea"); if (stampArea != null) target = stampArea; }
                string want = holding ? DropText : step.Body;
                if (_body.text != want) _body.text = want;
            }
            bool has = target != null && target.gameObject.activeInHierarchy;
            _frame.gameObject.SetActive(has);
            _arrow.gameObject.SetActive(has);

            Rect area = has ? ScreenRect(target) : new Rect(_root.rect.width * 0.5f - 1f, _root.rect.height * 0.5f, 2f, 2f);
            // çerçeve
            float pad = 10f + 3f * Mathf.Sin(_t * 5f);
            _frame.anchoredPosition = area.center;
            _frame.sizeDelta = area.size + Vector2.one * pad * 2f;
            foreach (var img in _frame.GetComponentsInChildren<Image>())
                img.color = new Color(Gold.r, Gold.g, Gold.b, 0.65f + 0.35f * Mathf.Sin(_t * 5f));

            // kart: hedefin altında yer varsa altta, yoksa üstte; yatayda ekrana sığdır
            var size = _root.rect.size;
            bool below = area.yMin - _card.sizeDelta.y - 120f > 0f;
            float cy = below ? area.yMin - 110f - _card.sizeDelta.y * 0.5f : area.yMax + 110f + _card.sizeDelta.y * 0.5f;
            cy = Mathf.Clamp(cy, _card.sizeDelta.y * 0.5f + 20f, size.y - _card.sizeDelta.y * 0.5f - 90f);
            float cx = Mathf.Clamp(area.center.x, _card.sizeDelta.x * 0.5f + 20f, size.x - _card.sizeDelta.x * 0.5f - 20f);
            _card.anchoredPosition = new Vector2(cx, cy);

            // ok: kartla hedef arasında, hedefe doğru, hafifçe zıplar
            float bob = 8f * Mathf.Sin(_t * 6f);
            _arrow.localRotation = Quaternion.Euler(0f, 0f, below ? 0f : 180f); // sprite yukarıyı gösterir
            float ay = below ? area.yMin - 60f - bob : area.yMax + 60f + bob;
            _arrow.anchoredPosition = new Vector2(area.center.x, ay);
        }

        void Show(int i)
        {
            _step = i;
            var s = Steps[i];
            _title.text = s.Title;
            _body.text = s.Body;
            _counter.text = $"{i + 1} / {Steps.Length}";
            _next.text = i == Steps.Length - 1 && !_bribeMode ? "Başla" : "Anladım";
            _next.transform.parent.gameObject.SetActive(s.Done == null); // eylem adımında düğme yok: oyuncu yapınca geçer
            _counter.gameObject.SetActive(!_bribeMode);
            // Kart yüksekliği metne göre (sabit yükseklikte uzun metin taşıyordu): başlık + metin + düğme şeridi
            float bodyH = _body.GetPreferredValues(s.Body, _card.sizeDelta.x - 44f - 8f, 0f).y;
            _card.sizeDelta = new Vector2(_card.sizeDelta.x, Mathf.Max(200f, 66f + bodyH + 18f + (s.Done == null || !_bribeMode ? 64f : 0f)));
            // Ek test adımı: İNCELE modu açıksa EK TEST pasif görünür (kural); rehber modu kapatır ki düğme canlı görünsün
            if (s.Title == "Ek test" && _inspect != null && _inspect.IsOn) _inspect.SetMode(false);
            _skip.SetActive(!_bribeMode);
            SetVisible(true);
        }

        void Next()
        {
            if (_step + 1 < Steps.Length) Show(_step + 1);
            else Finish();
        }

        void Finish()
        {
            if (_bribeMode) _bribeShownThisGame = true;
            if (!_bribeMode) JuryHooks.RaiseTutorialFinished(); // jüri ipucu, eski kartlardaki gibi rehber bitince gelir
            Destroy(gameObject);
        }

        void SetVisible(bool on)
        {
            _frame.gameObject.SetActive(on);
            _arrow.gameObject.SetActive(on);
            _card.gameObject.SetActive(on);
        }

        // ---------- yardımcılar ----------

        Transform Find(string name) => _desk != null ? FindDeep(_desk, name) : null;

        static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var r = FindDeep(t.GetChild(i), name);
                if (r != null) return r;
            }
            return null;
        }

        // Hedefin ekrandaki dikdörtgeni, rehber tuvalinin (sol alt köşe 0,0) koordinatlarında
        Rect ScreenRect(Transform target)
        {
            var rt = target as RectTransform;
            if (rt == null) return new Rect(target.position, Vector2.one);
            var canvas = rt.GetComponentInParent<Canvas>();
            Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
            foreach (var c in corners)
            {
                Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam, c);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, sp, null, out var local);
                local += _root.rect.size * 0.5f; // merkezden sol alta
                min = Vector2.Min(min, local); max = Vector2.Max(max, local);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = Vector2.zero; // konumlar sol alttan
            rt.pivot = new Vector2(0.5f, 0.5f);
            return rt;
        }

        static Image Img(string name, Transform parent, Color c)
        {
            var img = NewRect(name, parent).gameObject.AddComponent<Image>();
            img.color = c;
            img.raycastTarget = false;
            return img;
        }

        static void Stretch(RectTransform rt, float inset)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.one * inset; rt.offsetMax = -Vector2.one * inset;
        }

        static TextMeshProUGUI Text(Transform parent, string name, float size, Color c, TMP_FontAsset font, TextAlignmentOptions align)
        {
            var t = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.fontSize = size; t.color = c; t.alignment = align;
            t.raycastTarget = false;
            return t;
        }

        // Üst köşeye göre yerleşim (sol üst ya da sağ üst)
        static void Place(RectTransform rt, float x, float y, float w, float h, bool right = false)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(right ? 1f : 0f, 1f);
            rt.pivot = new Vector2(right ? 1f : 0f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w < 0 ? 560f + w - 44f : w, h);
        }

        static TextMeshProUGUI Button(Transform parent, string name, string label, TMP_FontAsset font, Action onClick,
            Vector2 corner, Vector2 offset, Vector2 size, Color bg, Color fg)
        {
            var rt = NewRect(name, parent);
            rt.anchorMin = rt.anchorMax = rt.pivot = corner;
            rt.anchoredPosition = offset;
            rt.sizeDelta = size;
            var img = rt.gameObject.AddComponent<Image>();
            img.color = bg;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick());
            var t = Text(rt, "Label", 22f, fg, font, TextAlignmentOptions.Center);
            Stretch(t.rectTransform, 0f);
            t.text = label;
            return t;
        }

        // Yukarıyı gösteren kalın ok (üçgen baş + gövde), 64x80
        static Sprite ArrowSprite()
        {
            const int w = 64, h = 80;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool head = y >= 40 && Mathf.Abs(x - 31.5f) <= (h - 1 - y) * 0.8f;
                    bool shaft = y < 42 && Mathf.Abs(x - 31.5f) <= 10f;
                    px[y * w + x] = head || shaft ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
        }
    }
}
