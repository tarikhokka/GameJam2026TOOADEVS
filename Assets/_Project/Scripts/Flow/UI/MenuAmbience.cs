using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    /// <summary>
    /// (EEE, 17 Ekim) Ana menüyü canlandıran katmanlar (menu_bg tek parça resim; hareket üstüne bindirilen katmanlarla):
    ///  - Pencere ışıkları: menu_bg'deki sarı pencerelerden çıkarılmış 4 katman. İlk yarısı "ışık söndü" (pencere ara sıra kararır),
    ///    ikinci yarısı "ışık parladı" (yavaşça nefes alır).
    ///  - Güneş: batan güneşin üstünde nefes alan sıcak hale.
    ///  - Toz: havada yavaşça yükselen sıcak renkli zerreler.
    ///  - Açılış: ekran karanlıktan açılır, düğme yazıları sırayla belirir (tahtalar yerinde kalır; resimdeki çizili yazı görünmesin).
    /// Arka plan resmi kaymaz: gerçek düğmeler resimdeki çizili düğmelerin tam üstünde.
    /// </summary>
    internal class MenuAmbience : MonoBehaviour
    {
        // menu_bg'de güneşin merkezi (oran, sol alt köşeden) ve yarıçapı ~37 px (1920x1080)
        static readonly Vector2 SunAnchor = new Vector2(0.6968f, 1f - 0.4904f);
        const int DustCount = 36;
        // Vinçler (menu_bg 1920x1080 piksel, üstten): kırmızı ikaz lambaları (tepe, kol ucu, karşı ağırlık ucu)
        // ve sol vincin kol ucuna yakın sarkaç gibi sallanan yük (resimdeki çizili kancalardan uzakta).
        static readonly Vector2[] CraneLights =
        {
            new Vector2(450, 352), new Vector2(660, 452), new Vector2(342, 392),     // sol vinç
            new Vector2(1635, 540), new Vector2(1830, 559), new Vector2(1465, 612),  // sağ vinç
        };
        static readonly Vector2 LoadHook = new Vector2(620, 447);
        static Vector2 Anchor(Vector2 px) => new Vector2(px.x / 1920f, 1f - px.y / 1080f);
        const float IntroFade = 1.1f, LabelDelay = 0.35f, LabelStep = 0.18f, LabelFade = 0.35f;

        class Mote { public RectTransform Rt; public Image Img; public Vector2 Pos, Vel; public float Life, Age, Size, Phase; }
        class Flicker { public Image Img; public float Next, Until, Phase, Speed; public bool Dark; }

        RectTransform _root;
        readonly List<Flicker> _flickers = new List<Flicker>();
        readonly List<Mote> _motes = new List<Mote>();
        Image _glow, _cover;
        readonly List<(Image img, float phase)> _beacons = new List<(Image, float)>();
        RectTransform _load;
        // Yazının kendi rengine dokunulmaz (ButtonHover onu kaydediyor); saydamlık ayrı CanvasGroup'tan.
        readonly List<(CanvasGroup group, Transform tr)> _labels = new List<(CanvasGroup, Transform)>();
        Sprite _dust;
        float _t;

        /// <summary>Katmanları kurar. Arka planın hemen üstüne, menü panelinin altına konur.</summary>
        public void Build(RectTransform root, Transform belowThis, Sprite[] lights, Sprite glow, Sprite dust,
            Sprite craneLight = null, Sprite craneLoad = null)
        {
            _root = root;
            _dust = dust;
            int index = belowThis != null ? belowThis.GetSiblingIndex() : root.childCount;

            if (lights != null)
                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] == null) continue;
                    var img = UiKit.Image(root, "Lights" + i, lights[i], new Color(1f, 1f, 1f, 0f), Vector2.zero, Vector2.one);
                    img.transform.SetSiblingIndex(index++);
                    _flickers.Add(new Flicker
                    {
                        Img = img, Dark = i < lights.Length / 2, Phase = Random.value * 10f,
                        Speed = Random.Range(0.35f, 0.7f), Next = Random.Range(1.5f, 5f)
                    });
                }

            if (glow != null)
            {
                _glow = UiKit.Image(root, "SunGlow", glow, new Color(1f, 0.85f, 0.55f, 0.5f), SunAnchor, SunAnchor);
                _glow.rectTransform.sizeDelta = new Vector2(300f, 300f);
                _glow.transform.SetSiblingIndex(index++);
            }

            if (craneLoad != null)
            {
                var a = Anchor(LoadHook);
                var img = UiKit.Image(root, "CraneLoad", craneLoad, Color.white, a, a);
                _load = img.rectTransform;
                _load.pivot = new Vector2(0.5f, 1f); // halatın bağlandığı nokta: sarkaç buradan döner
                _load.sizeDelta = new Vector2(50f, 130f);
                _load.SetSiblingIndex(index++);
            }

            if (craneLight != null)
                foreach (var px in CraneLights)
                {
                    var a = Anchor(px);
                    var img = UiKit.Image(root, "CraneLight", craneLight, new Color(1f, 1f, 1f, 0f), a, a);
                    img.rectTransform.sizeDelta = new Vector2(26f, 26f);
                    img.transform.SetSiblingIndex(index++);
                    _beacons.Add((img, Random.value));
                }

            if (dust != null)
            {
                var layer = UiKit.Stretch(root, "Dust");
                layer.SetSiblingIndex(index++);
                for (int i = 0; i < DustCount; i++)
                {
                    var img = UiKit.Image(layer, "Mote", dust, Color.clear, Vector2.zero, Vector2.zero);
                    var m = new Mote { Rt = img.rectTransform, Img = img };
                    Respawn(m, true);
                    _motes.Add(m);
                }
            }
        }

        /// <summary>Açılış: üstte karanlık örtü + düğme yazıları sırayla.</summary>
        public void PlayIntro(Transform menuPanel)
        {
            _cover = UiKit.Image(_root, "IntroCover", null, new Color(0.04f, 0.03f, 0.03f, 1f), Vector2.zero, Vector2.one);
            _cover.transform.SetAsLastSibling();
            foreach (var b in menuPanel.GetComponentsInChildren<Button>(true))
            {
                var label = b.GetComponentInChildren<TMP_Text>();
                if (label == null) continue;
                var group = label.gameObject.AddComponent<CanvasGroup>();
                group.alpha = 0f;
                group.blocksRaycasts = false;
                _labels.Add((group, label.transform));
            }
        }

        void Respawn(Mote m, bool anywhere)
        {
            var size = _root.rect.size;
            if (size.x < 1f) size = new Vector2(1920f, 1080f);
            m.Pos = new Vector2(Random.Range(0f, size.x), anywhere ? Random.Range(0f, size.y) : Random.Range(-20f, size.y * 0.3f));
            m.Vel = new Vector2(Random.Range(-6f, 6f), Random.Range(10f, 26f));
            m.Life = Random.Range(7f, 14f);
            m.Age = anywhere ? Random.Range(0f, m.Life) : 0f;
            m.Size = Random.Range(5f, 13f);
            m.Phase = Random.value * 6.28f;
            m.Rt.sizeDelta = new Vector2(m.Size, m.Size);
        }

        void Update()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            _t += dt;

            foreach (var f in _flickers)
            {
                float a;
                if (f.Dark)
                {
                    // Ara sıra birkaç pencerenin ışığı söner, sonra geri yanar
                    if (_t >= f.Next && _t > f.Until) { f.Until = _t + Random.Range(1.2f, 3.5f); f.Next = f.Until + Random.Range(3f, 8f); }
                    float target = _t < f.Until ? 0.85f : 0f;
                    a = Mathf.MoveTowards(f.Img.color.a, target, dt * 2.2f);
                }
                else
                {
                    a = 0.35f + 0.3f * Mathf.Sin(_t * f.Speed * 2f + f.Phase) + 0.1f * Mathf.Sin(_t * 3.1f + f.Phase * 2f);
                }
                var c = f.Img.color; c.a = Mathf.Clamp01(a); f.Img.color = c;
            }

            if (_glow != null)
            {
                float s = 1f + 0.07f * Mathf.Sin(_t * 0.8f);
                _glow.rectTransform.localScale = new Vector3(s, s, 1f);
                var c = _glow.color; c.a = 0.45f + 0.15f * Mathf.Sin(_t * 0.6f + 1f); _glow.color = c;
            }

            if (_load != null)
            {
                // Rüzgârda hafif sallanan yük: iki farklı hızın toplamı, tekdüze olmasın
                float angle = 3.2f * Mathf.Sin(_t * 1.3f) + 0.9f * Mathf.Sin(_t * 0.47f + 1.7f);
                _load.localRotation = Quaternion.Euler(0f, 0f, angle);
            }

            foreach (var (img, phase) in _beacons)
            {
                // İkaz lambası: ~1,4 sn'de bir kısa yanıp söner, her lamba kendi zamanında
                float cycle = Mathf.Repeat(_t / 1.4f + phase, 1f);
                float on = cycle < 0.35f ? Mathf.Sin(cycle / 0.35f * Mathf.PI) : 0f;
                img.color = new Color(1f, 1f, 1f, on);
            }

            if (_motes.Count > 0)
            {
                var size = _root.rect.size;
                foreach (var m in _motes)
                {
                    m.Age += dt;
                    if (m.Age >= m.Life || m.Pos.y > size.y + 20f) { Respawn(m, false); continue; }
                    m.Pos += new Vector2(m.Vel.x + Mathf.Sin(_t * 0.7f + m.Phase) * 8f, m.Vel.y) * dt;
                    m.Rt.anchoredPosition = m.Pos;
                    float k = m.Age / m.Life;
                    float alpha = Mathf.Sin(k * Mathf.PI) * 0.55f; // yavaşça belirip kaybolur
                    m.Img.color = new Color(1f, 0.86f, 0.62f, alpha);
                }
            }

            if (_cover != null)
            {
                float k = Mathf.Clamp01(_t / IntroFade);
                _cover.color = new Color(_cover.color.r, _cover.color.g, _cover.color.b, 1f - k * k * (3f - 2f * k));
                if (k >= 1f) { Destroy(_cover.gameObject); _cover = null; }
            }

            for (int i = 0; i < _labels.Count; i++)
            {
                var (group, tr) = _labels[i];
                if (group == null) continue;
                float k = Mathf.Clamp01((_t - LabelDelay - i * LabelStep) / LabelFade);
                group.alpha = k;
                float s = Mathf.Lerp(0.9f, 1f, 1f - (1f - k) * (1f - k));
                tr.localScale = new Vector3(s, s, 1f);
            }
            if (_labels.Count > 0 && _t > LabelDelay + _labels.Count * LabelStep + LabelFade + 0.1f) _labels.Clear();
        }
    }
}
