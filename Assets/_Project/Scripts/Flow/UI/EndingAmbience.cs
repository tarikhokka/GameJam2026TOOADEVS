using System.Collections.Generic;
using CivilJam.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    /// <summary>
    /// (EEE, 18 Ekim) Son ekranını canlandıran katmanlar (ana menüdeki MenuAmbience gibi; görsel tek parça, hareket üstüne bindirilir).
    /// Arka plan görselinin hemen üstüne, karartma ve yazı panelinin altına kurulur; yazının okunurluğunu bozmaz.
    ///  - İyi: güneş halesi nefes alır, güneş huzmesinde toz, çay bardağından buhar.
    ///  - Kötü (hapis): pencereden gelen ışık bulut geçiyormuş gibi yavaşça değişir, huzmede toz, gözetleme kulesinde kırmızı lamba.
    ///  - İflas: pencerede yağmur, ara sıra şimşek.
    ///  - Enkazın Gölgesi: çok yavaş düşen kül, pencerede soluk nefes alan ışık.
    /// Koordinatlar görselde oran (sol üstten); 1920x1080.
    /// </summary>
    internal class EndingAmbience : MonoBehaviour
    {
        class Mote { public RectTransform Rt; public Image Img; public Vector2 Pos, Vel; public float Life, Age, Size, Phase; }
        class MoteField { public Rect Region; public Color Color; public Vector2 VelMin, VelMax; public Vector2 SizeRange; public float Alpha, Sway, Grow; public bool SpawnBottom, SpawnTop; public List<Mote> Motes = new List<Mote>(); }
        class Breather { public Image Img; public float Min, Max, Speed, Phase; public bool Scale; }
        class Streak { public RectTransform Rt; public float Speed; }

        RectTransform _root;
        Sprite _dot;
        float _t;
        readonly List<MoteField> _fields = new List<MoteField>();
        readonly List<Breather> _breathers = new List<Breather>();
        readonly List<(Image img, float phase)> _blinks = new List<(Image, float)>();
        readonly List<Streak> _rain = new List<Streak>();
        Rect _rainRegion;
        Image _flash;
        float _nextFlash;

        static Vector2 A(float x, float yTop) => new Vector2(x, 1f - yTop);

        public void Play(EndingType type, Transform parent, int siblingIndex, Sprite glow, Sprite dot, Sprite redLight)
        {
            _root = UiKit.Stretch(parent, "Ambience");
            _root.SetSiblingIndex(siblingIndex);
            _dot = dot;
            switch (type)
            {
                case EndingType.Good:
                    Glow(glow, A(0.82f, 0.245f), 420f, new Color(1f, 0.85f, 0.5f), 0.35f, 0.65f, 0.5f, true);
                    Motes(new Rect(0.2f, 0.05f, 0.6f, 0.55f), new Color(1f, 0.9f, 0.65f), 34, new Vector2(-6, -4), new Vector2(6, 8), new Vector2(4, 10), 0.6f, 10f, 0f, false, false);
                    Motes(new Rect(0.30f, 0.52f, 0.03f, 0.14f), new Color(1f, 0.97f, 0.92f), 10, new Vector2(-2, 22), new Vector2(2, 34), new Vector2(14, 22), 0.22f, 9f, 1.8f, true, false); // çay buharı
                    break;
                case EndingType.Bad:
                    Breathe(null, Vector2.zero, Vector2.one, new Color(1f, 0.78f, 0.45f), 0f, 0.12f, 0.18f);       // bulut geçiyor
                    Glow(glow, A(0.80f, 0.11f), 520f, new Color(1f, 0.8f, 0.5f), 0.2f, 0.45f, 0.3f, false);
                    Motes(new Rect(0.30f, 0.05f, 0.55f, 0.6f), new Color(1f, 0.88f, 0.6f), 30, new Vector2(-5, -6), new Vector2(5, 4), new Vector2(4, 9), 0.55f, 8f, 0f, false, false);
                    if (redLight != null) Blink(redLight, A(0.865f, 0.07f), 22f);
                    break;
                case EndingType.MiddleClosed:
                    Rain(new Rect(0.53f, 0.02f, 0.44f, 0.56f), 70);
                    _flash = UiKit.Image(_root, "Lightning", null, new Color(0.85f, 0.9f, 1f, 0f), A(0.53f, 0.58f), A(0.97f, 0.02f));
                    _nextFlash = Random.Range(3f, 7f);
                    break;
                case EndingType.MiddleCollapsed:
                    Breathe(glow, A(0.5f, 0.22f), A(0.5f, 0.22f), new Color(0.85f, 0.9f, 1f), 0.15f, 0.35f, 0.25f, 760f);
                    Motes(new Rect(0f, -0.05f, 1f, 1f), new Color(0.82f, 0.82f, 0.85f), 40, new Vector2(-4, -16), new Vector2(4, -7), new Vector2(3, 8), 0.5f, 12f, 0f, false, true);
                    break;
            }
        }

        // ---------- Kurucular ----------

        void Glow(Sprite sprite, Vector2 anchor, float size, Color color, float min, float max, float speed, bool scale)
        {
            if (sprite == null) return;
            var img = UiKit.Image(_root, "Glow", sprite, color, anchor, anchor);
            img.rectTransform.sizeDelta = new Vector2(size, size);
            _breathers.Add(new Breather { Img = img, Min = min, Max = max, Speed = speed, Phase = Random.value * 6f, Scale = scale });
        }

        void Breathe(Sprite sprite, Vector2 min, Vector2 max, Color color, float aMin, float aMax, float speed, float size = 0f)
        {
            var img = UiKit.Image(_root, "Light", sprite, color, min, max);
            if (size > 0f) img.rectTransform.sizeDelta = new Vector2(size, size);
            _breathers.Add(new Breather { Img = img, Min = aMin, Max = aMax, Speed = speed, Phase = Random.value * 6f });
        }

        void Blink(Sprite sprite, Vector2 anchor, float size)
        {
            var img = UiKit.Image(_root, "Blink", sprite, new Color(1f, 1f, 1f, 0f), anchor, anchor);
            img.rectTransform.sizeDelta = new Vector2(size, size);
            _blinks.Add((img, Random.value));
        }

        void Motes(Rect region, Color color, int count, Vector2 velMin, Vector2 velMax, Vector2 sizeRange, float alpha, float sway, float grow,
            bool spawnBottom, bool spawnTop)
        {
            if (_dot == null) return;
            var f = new MoteField { Region = region, Color = color, VelMin = velMin, VelMax = velMax, SizeRange = sizeRange, Alpha = alpha,
                Sway = sway, Grow = grow, SpawnBottom = spawnBottom, SpawnTop = spawnTop };
            var layer = UiKit.Stretch(_root, "Motes");
            for (int i = 0; i < count; i++)
            {
                var img = UiKit.Image(layer, "Mote", _dot, Color.clear, Vector2.zero, Vector2.zero);
                var m = new Mote { Rt = img.rectTransform, Img = img };
                Respawn(f, m, true);
                f.Motes.Add(m);
            }
            _fields.Add(f);
        }

        void Rain(Rect region, int count)
        {
            _rainRegion = region;
            var layer = UiKit.Stretch(_root, "Rain");
            for (int i = 0; i < count; i++)
            {
                var img = UiKit.Image(layer, "Drop", null, new Color(0.8f, 0.86f, 1f, Random.Range(0.18f, 0.4f)), Vector2.zero, Vector2.zero);
                var rt = img.rectTransform;
                rt.sizeDelta = new Vector2(2f, Random.Range(26f, 48f));
                rt.localRotation = Quaternion.Euler(0, 0, 8f); // rüzgâr
                var s = new Streak { Rt = rt, Speed = Random.Range(700f, 1000f) };
                rt.anchoredPosition = RainSpawn(true);
                _rain.Add(s);
            }
        }

        // ---------- Yardımcılar ----------

        Vector2 Size => _root.rect.size.x > 1f ? _root.rect.size : new Vector2(1920f, 1080f);

        // Bölge (sol üstten oran) → ekran pikseli (sol alttan)
        Vector2 InRegion(Rect r, float u, float v) => new Vector2((r.x + r.width * u) * Size.x, (1f - (r.y + r.height * v)) * Size.y);

        Vector2 RainSpawn(bool anywhere) => InRegion(_rainRegion, Random.value, anywhere ? Random.value : 0f);

        void Respawn(MoteField f, Mote m, bool anywhere)
        {
            float v = anywhere ? Random.value : f.SpawnBottom ? 1f : f.SpawnTop ? 0f : Random.value;
            m.Pos = InRegion(f.Region, Random.value, v);
            m.Vel = new Vector2(Random.Range(f.VelMin.x, f.VelMax.x), Random.Range(f.VelMin.y, f.VelMax.y));
            m.Life = Random.Range(6f, 12f);
            m.Age = anywhere ? Random.Range(0f, m.Life) : 0f;
            m.Size = Random.Range(f.SizeRange.x, f.SizeRange.y);
            m.Phase = Random.value * 6.28f;
            m.Rt.sizeDelta = new Vector2(m.Size, m.Size);
        }

        void Update()
        {
            if (_root == null) return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            _t += dt;

            foreach (var b in _breathers)
            {
                float k = 0.5f + 0.5f * Mathf.Sin(_t * b.Speed * 2f + b.Phase) * (0.8f + 0.2f * Mathf.Sin(_t * 0.37f + b.Phase));
                var c = b.Img.color; c.a = Mathf.Lerp(b.Min, b.Max, k); b.Img.color = c;
                if (b.Scale) { float s = 1f + 0.06f * Mathf.Sin(_t * b.Speed * 1.6f + b.Phase); b.Img.rectTransform.localScale = new Vector3(s, s, 1f); }
            }

            foreach (var (img, phase) in _blinks)
            {
                float cycle = Mathf.Repeat(_t / 1.6f + phase, 1f);
                img.color = new Color(1f, 1f, 1f, cycle < 0.3f ? Mathf.Sin(cycle / 0.3f * Mathf.PI) : 0f);
            }

            foreach (var f in _fields)
                foreach (var m in f.Motes)
                {
                    m.Age += dt;
                    if (m.Age >= m.Life) { Respawn(f, m, false); continue; }
                    m.Pos += new Vector2(m.Vel.x + Mathf.Sin(_t * 0.7f + m.Phase) * f.Sway, m.Vel.y) * dt;
                    m.Rt.anchoredPosition = m.Pos;
                    float k = m.Age / m.Life;
                    if (f.Grow > 0f) { float s = m.Size * (1f + f.Grow * k); m.Rt.sizeDelta = new Vector2(s, s); }
                    m.Img.color = new Color(f.Color.r, f.Color.g, f.Color.b, Mathf.Sin(k * Mathf.PI) * f.Alpha);
                }

            if (_rain.Count > 0)
            {
                float bottom = (1f - (_rainRegion.y + _rainRegion.height)) * Size.y;
                foreach (var s in _rain)
                {
                    var p = s.Rt.anchoredPosition + new Vector2(-0.14f, -1f) * s.Speed * dt;
                    if (p.y < bottom) p = RainSpawn(false);
                    s.Rt.anchoredPosition = p;
                }
            }

            if (_flash != null)
            {
                _nextFlash -= dt;
                float a = 0f;
                if (_nextFlash < 0f)
                {
                    float f = -_nextFlash; // çift çakma: 0-0,08 ve 0,16-0,3 sn
                    a = f < 0.08f ? 0.55f : f < 0.16f ? 0.1f : f < 0.3f ? 0.4f * (1f - (f - 0.16f) / 0.14f) : 0f;
                    if (f > 0.35f) _nextFlash = Random.Range(6f, 12f);
                }
                var c = _flash.color; c.a = a; _flash.color = c;
            }
        }
    }
}
