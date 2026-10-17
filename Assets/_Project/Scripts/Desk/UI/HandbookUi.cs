using System;
using System.Collections.Generic;
using System.Globalization;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // El Kitabı "Hızlı Bakış" (Ahmet'in tasarımı, Docs/Tasarim/el_kitabi_hizli_bakis.html) için çalışırken kurulan UI parçaları.
    // Değerler RuleSet'ten gelir (çip sayısı = rule.Labels, eşik = FindRule(..).Required); kodda kural değeri yazılmaz.
    // Fontlarda ✓/✕ yok (EEE): yetersiz kademe renk + üstü çizili yazıyla, yeterli olan renkle, en az olan "en az" etiketiyle ayrılır.
    internal static class HandbookUi
    {
        public static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

        // Tasarım renkleri
        public static readonly Color Paper = Hex(0xf4ead6), Paper2 = Hex(0xeadcc0), Line = Hex(0xcdb994);
        public static readonly Color Ink = Hex(0x3b2f22), InkSoft = Hex(0x7a6650), NoteBg = Hex(0xfffaf0);
        public static readonly Color Ok = Hex(0x3f8a4f), OkBg = Hex(0xdcebd3), Bad = Hex(0xc2412f), BadBg = Hex(0xf3d3cb);
        public static readonly Color General = Hex(0x7a5ea8);

        public static Color SectionColor(string section)
        {
            switch ((section ?? "").ToUpper(Tr))
            {
                case "BETON": return Hex(0xc4553f);
                case "DONATI": return Hex(0x3f7cb3);
                case "YAPI": return Hex(0xd18a25);
                case "ZEMİN": return Hex(0x5d9152);
                default: return General;
            }
        }

        public static Color Hex(int rgb) =>
            new Color(((rgb >> 16) & 0xff) / 255f, ((rgb >> 8) & 0xff) / 255f, (rgb & 0xff) / 255f);

        // ---------- Temel parçalar ----------

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static RectTransform Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Img(Component target, Color color, bool raycast = false)
        {
            var image = target.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, TMP_FontAsset font, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft, bool wrap = false)
        {
            var t = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            return t;
        }

        public static HorizontalLayoutGroup HL(Component target, float spacing, RectOffset pad = null, bool expandWidth = false,
            TextAnchor align = TextAnchor.MiddleLeft)
        {
            var l = target.gameObject.AddComponent<HorizontalLayoutGroup>();
            l.spacing = spacing;
            l.padding = pad ?? new RectOffset(0, 0, 0, 0);
            l.childAlignment = align;
            l.childControlWidth = l.childControlHeight = true;
            l.childForceExpandWidth = expandWidth;
            l.childForceExpandHeight = false;
            return l;
        }

        public static VerticalLayoutGroup VL(Component target, float spacing, RectOffset pad = null)
        {
            var l = target.gameObject.AddComponent<VerticalLayoutGroup>();
            l.spacing = spacing;
            l.padding = pad ?? new RectOffset(0, 0, 0, 0);
            l.childControlWidth = l.childControlHeight = true;
            l.childForceExpandWidth = true;
            l.childForceExpandHeight = false;
            return l;
        }

        public static LayoutElement Size(Component target, float prefW = -1, float prefH = -1, float flexW = -1, float flexH = -1,
            float minH = -1)
        {
            var e = target.GetComponent<LayoutElement>();
            if (e == null) e = target.gameObject.AddComponent<LayoutElement>();
            e.preferredWidth = prefW;
            e.preferredHeight = prefH;
            e.flexibleWidth = flexW;
            e.flexibleHeight = flexH;
            e.minHeight = minH;
            return e;
        }

        public static void Border(Graphic g, Color color, float width)
        {
            var o = g.gameObject.AddComponent<Outline>();
            o.effectColor = color;
            o.effectDistance = new Vector2(width, -width);
        }

        // ---------- Parametre gösterimi (çip / cetvel) ----------

        // Bir parametrenin kat gruplarına göre gösterimi. Paint(-1) = nötr (grup seçilmedi), Paint(i) = i. grubun eşiğine göre boyar.
        public abstract class Scale
        {
            public abstract void Paint(int group);
        }

        // Seçenekli parametre: Labels soldan sağa (kötüden iyiye) çipler; eşiğin altı kırmızı + üstü çizili, eşik "en az" + yeşil çerçeve.
        public sealed class Chips : Scale
        {
            private readonly Image[] backs;
            private readonly TextMeshProUGUI[] texts;
            private readonly Outline[] outlines;
            private readonly GameObject[] minTags;
            private readonly string[] labels;
            private readonly float[] required; // grup başına eşik (Labels içindeki sıra)

            public Chips(Transform parent, ParamRule anyRule, float[] required, TMP_FontAsset bold, float height)
            {
                labels = anyRule.Labels.ToArray();
                this.required = required;
                var row = Rect("Chips", parent);
                HL(row, 4, null, expandWidth: true);
                Size(row, prefH: height, flexW: 1);

                backs = new Image[labels.Length];
                texts = new TextMeshProUGUI[labels.Length];
                outlines = new Outline[labels.Length];
                minTags = new GameObject[labels.Length];
                for (int i = 0; i < labels.Length; i++)
                {
                    var chip = Rect("Chip" + i, row);
                    Size(chip, prefW: 0, flexW: 1, prefH: height);
                    backs[i] = Img(chip, Paper2);
                    outlines[i] = chip.gameObject.AddComponent<Outline>();
                    outlines[i].effectColor = Ok;
                    outlines[i].effectDistance = new Vector2(2, -2);
                    texts[i] = Text("Label", chip, labels[i], 14, bold, Ink, TextAlignmentOptions.Center);
                    texts[i].enableAutoSizing = true;
                    texts[i].fontSizeMin = 9;
                    texts[i].fontSizeMax = 14;
                    Stretch(texts[i].rectTransform).offsetMin = new Vector2(2, 0);
                    texts[i].rectTransform.offsetMax = new Vector2(-2, 0);

                    // "en az" etiketi çipin üst kenarında (yerleşim dışı)
                    var tag = Rect("MinTag", chip);
                    tag.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                    tag.anchorMin = tag.anchorMax = new Vector2(0.5f, 1f);
                    tag.pivot = new Vector2(0.5f, 0.5f);
                    tag.sizeDelta = new Vector2(40, 13);
                    Img(tag, Ok);
                    var tagText = Text("Text", tag, "en az", 9, bold, Color.white, TextAlignmentOptions.Center);
                    Stretch(tagText.rectTransform);
                    minTags[i] = tag.gameObject;
                }
            }

            public override void Paint(int group)
            {
                int req = group >= 0 ? Mathf.RoundToInt(required[group]) : -1;
                for (int i = 0; i < labels.Length; i++)
                {
                    bool neutral = group < 0, bad = !neutral && i < req, atMin = !neutral && i == req;
                    backs[i].color = neutral ? Paper2 : bad ? BadBg : OkBg;
                    texts[i].color = neutral ? Ink : bad ? Bad : Ok;
                    texts[i].text = bad ? $"<s>{labels[i]}</s>" : labels[i];
                    outlines[i].enabled = atMin;
                    minTags[i].SetActive(atMin);
                }
            }
        }

        // Sayısal parametre: yatay cetvel; eşiğin solu kırmızı "YETERSİZ", sağı yeşil "UYGUN", eşikte "en az 35 cm".
        public sealed class Ruler : Scale
        {
            private readonly float lo, hi;
            private readonly float[] required;
            private readonly RectTransform badBar, okBar, marker;
            private readonly Image badImage, okImage;
            private readonly TextMeshProUGUI markerLabel, badZone, okZone;
            private readonly Func<float, string> format;

            public Ruler(Transform parent, float[] required, Func<float, string> format, TMP_FontAsset text, TMP_FontAsset bold, float height)
            {
                this.required = required;
                this.format = format;
                float min = Mathf.Min(required), max = Mathf.Max(required);
                lo = min * 0.6f;
                hi = max * 1.3f;
                float step = NiceStep((hi - lo) / 6f);

                var root = Rect("Ruler", parent);
                Size(root, prefH: height, flexW: 1);

                // Çubuk: üstte işaret etiketi için boşluk, altta çentik yazıları
                var bar = Rect("Bar", root);
                bar.anchorMin = new Vector2(0, 1);
                bar.anchorMax = new Vector2(1, 1);
                bar.pivot = new Vector2(0.5f, 1f);
                bar.sizeDelta = new Vector2(0, 14);
                bar.anchoredPosition = new Vector2(0, -14);

                badBar = Rect("Bad", bar);
                badImage = Img(badBar, BadBg);
                okBar = Rect("Ok", bar);
                okImage = Img(okBar, OkBg);

                for (float v = Mathf.Ceil(lo / step) * step; v <= hi + 1e-4f; v += step)
                {
                    float t = (v - lo) / (hi - lo);
                    var tick = Rect("Tick", bar);
                    tick.anchorMin = tick.anchorMax = new Vector2(t, 0.5f);
                    tick.sizeDelta = new Vector2(1, 14);
                    Img(tick, new Color(Ink.r, Ink.g, Ink.b, 0.25f));
                    var lbl = Text("TickLabel", bar, v.ToString("0.#", Tr), 10, text, InkSoft, TextAlignmentOptions.Top);
                    lbl.rectTransform.anchorMin = lbl.rectTransform.anchorMax = new Vector2(t, 0f);
                    lbl.rectTransform.pivot = new Vector2(0.5f, 1f);
                    lbl.rectTransform.sizeDelta = new Vector2(36, 12);
                    lbl.rectTransform.anchoredPosition = new Vector2(0, -1);
                }

                badZone = Text("BadZone", bar, "YETERSİZ", 9, bold, Bad, TextAlignmentOptions.MidlineLeft);
                Stretch(badZone.rectTransform).offsetMin = new Vector2(6, 0);
                okZone = Text("OkZone", bar, "UYGUN", 9, bold, Ok, TextAlignmentOptions.MidlineRight);
                Stretch(okZone.rectTransform).offsetMax = new Vector2(-6, 0);

                // Eşik işareti: dikey çizgi + üstte "en az …"
                marker = Rect("Marker", bar);
                marker.anchorMin = new Vector2(0, 0);
                marker.anchorMax = new Vector2(0, 1);
                marker.sizeDelta = new Vector2(3, 8);
                Img(marker, Ok);
                var tagRect = Rect("Label", marker);
                tagRect.anchorMin = tagRect.anchorMax = new Vector2(0.5f, 1f);
                tagRect.pivot = new Vector2(0.5f, 0f);
                tagRect.sizeDelta = new Vector2(84, 14);
                tagRect.anchoredPosition = new Vector2(0, 2);
                Img(tagRect, Ok);
                markerLabel = Text("Text", tagRect, "en az", 10, bold, Color.white, TextAlignmentOptions.Center);
                Stretch(markerLabel.rectTransform);
            }

            public override void Paint(int group)
            {
                bool neutral = group < 0;
                float t = neutral ? 0.5f : Mathf.Clamp01((required[group] - lo) / (hi - lo));
                SetSpan(badBar, 0, t);
                SetSpan(okBar, t, 1);
                badImage.color = neutral ? Paper2 : BadBg;
                okImage.color = neutral ? Paper2 : OkBg;
                badZone.gameObject.SetActive(!neutral);
                okZone.gameObject.SetActive(!neutral);
                marker.gameObject.SetActive(!neutral);
                if (neutral) return;
                marker.anchorMin = new Vector2(t, 0);
                marker.anchorMax = new Vector2(t, 1);
                marker.anchoredPosition = Vector2.zero;
                markerLabel.text = $"en az {format(required[group])}";
            }

            private static void SetSpan(RectTransform rt, float from, float to)
            {
                rt.anchorMin = new Vector2(from, 0);
                rt.anchorMax = new Vector2(to, 1);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }

            private static float NiceStep(float raw)
            {
                float pow = Mathf.Pow(10, Mathf.Floor(Mathf.Log10(raw)));
                float n = raw / pow;
                return (n < 1.5f ? 1 : n < 3.5f ? 2.5f : n < 7.5f ? 5 : 10) * pow;
            }
        }

        // Kural verisinden ölçek: seçenekli → çip, sayısal → cetvel. required[i] = i. kat grubunun eşiği.
        public static Scale BuildScale(Transform parent, RuleSetSO rules, ParamType param, ParamRule anyRule, float[] required,
            TMP_FontAsset text, TMP_FontAsset bold, float height)
        {
            if (anyRule.IsLabeled) return new Chips(parent, anyRule, required, bold, height);
            return new Ruler(parent, required, v => rules.FormatValue(param, v), text, bold, height);
        }

        // Kat grubu: kurallardaki aralıklar (1-3 / 4-6 / 7+). temsilci kat = grubun ortası (ör. 1-3 → 2, 7+ → 8).
        public static List<(ParamRule rep, int floors, string label)> FloorGroups(RuleSetSO rules, BuildingType type)
        {
            var groups = new List<(ParamRule, int, string)>();
            var seen = new HashSet<(int, int)>();
            var list = new List<ParamRule>();
            foreach (var r in rules.RulesFor(type))
                if ((r.MinFloors != 0 || r.MaxFloors != 0) && seen.Add((r.MinFloors, r.MaxFloors))) list.Add(r);
            list.Sort((a, b) => a.MinFloors.CompareTo(b.MinFloors));
            foreach (var r in list)
            {
                int floors = r.MaxFloors > 0 ? (Math.Max(1, r.MinFloors) + r.MaxFloors) / 2 : r.MinFloors + 1;
                groups.Add((r, floors, r.FloorsLabel.ToUpper(Tr)));
            }
            return groups;
        }
    }
}
