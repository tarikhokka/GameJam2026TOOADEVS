using System;
using System.Collections.Generic;
using System.Text;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Denetçi El Kitabı — "Hızlı Bakış" tasarımı (Ahmet onayı, Docs/Tasarim/el_kitabi_hizli_bakis.html; içerik El Kitabı v2).
    // Sağ kenarda sekmeler: HIZLI BAKIŞ · BETON · DONATI · YAPI · ZEMİN (ctx.Rules.SectionsFor) · LAB.
    // Hızlı Bakış: oyuncu kat grubunu seçer (formdan otomatik seçilmez, Ahmet kararı); 5 parametre kırmızı/yeşil çip ya da cetvel
    // olarak o grubun eşiğine göre boyanır. Yeni müteahhit gelince seçim sıfırlanır (ResetFloorGroup); seçim yokken nötr.
    // Bölüm sekmeleri: parametre başına üç kat grubu alt alta + Description + afet bilgisi cümlesi (EEE, Inspector'dan).
    // Sayfalar çalışırken kurallardan kurulur (HandbookUi); kural değeri kodda yazılmaz, hiçbir şey "bu projede yanlış" diye işaretlenmez.
    public class RulebookView : MonoBehaviour
    {
        [Serializable]
        private class SectionNote
        {
            public string Section;
            [TextArea(2, 4)] public string Text;
        }

        [SerializeField] private Button openButton; // masadaki EL KİTABI
        [SerializeField] private PaperWindow window;
        [SerializeField] private RectTransform pageRoot; // kağıdın içi: sayfalar burada (biri açık)
        [SerializeField] private RectTransform tabsRoot; // sağ kenar sekmeleri

        [Header("Yazı tipi (EEE: Paytone One başlık, Fira Sans metin)")]
        [SerializeField] private TMP_FontAsset titleFont;
        [SerializeField] private TMP_FontAsset textFont;
        [SerializeField] private TMP_FontAsset boldFont;

        [Tooltip("Kat grubu düğmelerindeki bina simgeleri: alçak, orta, yüksek (Art/bld_low, bld_mid, bld_high)")]
        [SerializeField] private Sprite[] floorIcons;

        [Tooltip("Sekme görselleri (EEE: tab_beton/donati/yapi/zemin, yazı görselde). Adı tab_<bölüm> olan sekme bunu kullanır; " +
                 "görseli olmayan sekme (HIZLI BAKIŞ, LAB) aynı renkte kodla çizilir.")]
        [SerializeField] private Sprite[] tabSprites;

        [Tooltip("Bölüm sekmelerinin afet farkındalık cümlesi (EEE yazar, ICERIK_TASLAK.md). Boşsa gösterilmez.")]
        [SerializeField] private List<SectionNote> sectionNotes = new List<SectionNote>
        {
            new SectionNote { Section = "BETON" }, new SectionNote { Section = "DONATI" },
            new SectionNote { Section = "YAPI" }, new SectionNote { Section = "ZEMİN" },
        };

        private const BuildingType Type = BuildingType.Residential;
        private const float RowHeight = 30f;

        public event Action Toggled; // açma/kapama ve sekme değişimi (kağıt sesi)

        public PaperWindow Window => window;

        // İnceleme modu: El Kitabı eşiği oyuncunun Hızlı Bakış'ta seçtiği kat grubundan (0 = 1-3, 1 = 4-6, 2 = 7+; seçim yoksa
        // Inspection.NoFloorGroup). Gruplar küçükten büyüğe sıralı (HandbookUi.FloorGroups), Core'un sırasıyla aynı.
        public int SelectedFloorGroup => floorGroup < 0 ? Inspection.NoFloorGroup : floorGroup;

        private GameContext context;
        private bool built;
        private int floorGroup = -1; // seçili kat grubu; -1 = seçilmedi (nötr)
        private int page;
        private readonly List<GameObject> pages = new List<GameObject>();
        private readonly List<(Image image, Color color)> tabs = new List<(Image, Color)>();
        private readonly List<HandbookUi.Scale> quickScales = new List<HandbookUi.Scale>();
        private readonly List<(Image back, TextMeshProUGUI label)> floorButtons = new List<(Image, TextMeshProUGUI)>();

        private TMP_FontAsset Title => titleFont != null ? titleFont : textFont;
        private TMP_FontAsset Bold => boldFont != null ? boldFont : textFont;

        private void Awake()
        {
            if (openButton != null) openButton.onClick.AddListener(Toggle); // yeni düzende masada kitap yok: El Kitabı hep açık
            window.CloseClicked += () => { if (window.IsOpen) Toggle(); };
        }

        public void Bind(GameContext gameContext)
        {
            context = gameContext;
            built = false;
            floorGroup = -1;
            page = 0;
            window.Close(instant: true);
        }

        public void Close(bool instant = false) => window.Close(instant);

        // Yeni müteahhit: oyuncu kat grubunu yeniden seçer (zorluk korunur).
        public void ResetFloorGroup()
        {
            if (floorGroup < 0) return;
            floorGroup = -1;
            PaintQuick();
        }

        // Yeni düzen: El Kitabı masada hep açık (DeskScreen.Show çağırır).
        public void Open()
        {
            if (window.IsOpen) return;
            if (!built && context != null) Build(context);
            ShowPage(page);
            window.Open();
        }

        private void Toggle()
        {
            Toggled?.Invoke();
            if (window.IsOpen)
            {
                window.Close();
                return;
            }
            Open();
        }

        private void ShowPage(int index)
        {
            if (pages.Count == 0) return;
            page = Mathf.Clamp(index, 0, pages.Count - 1);
            for (int i = 0; i < pages.Count; i++) pages[i].SetActive(i == page);
            for (int i = 0; i < tabs.Count; i++)
                tabs[i].image.color = new Color(tabs[i].color.r, tabs[i].color.g, tabs[i].color.b, i == page ? 1f : 0.7f);
        }

        // ---------- Kurulum ----------

        private void Build(GameContext ctx)
        {
            foreach (Transform child in pageRoot) Destroy(child.gameObject);
            foreach (Transform child in tabsRoot) Destroy(child.gameObject);
            pages.Clear();
            tabs.Clear();
            quickScales.Clear();
            floorButtons.Clear();
            built = true;

            var rules = ctx.Rules;
            if (rules == null) return;
            var groups = HandbookUi.FloorGroups(rules, Type);
            var sections = rules.SectionsFor(Type);

            AddPage("HIZLI BAKIŞ", HandbookUi.General, BuildQuickPage(rules, groups, sections));
            foreach (var section in sections)
                AddPage(section, HandbookUi.SectionColor(section), BuildSectionPage(rules, groups, section));
            AddPage("LAB", HandbookUi.InkSoft, BuildLabPage(ctx));
            PaintQuick();
        }

        private void AddPage(string tabLabel, Color color, GameObject pageObject)
        {
            int index = pages.Count;
            pages.Add(pageObject);

            // Sekme: EEE görseli varsa o (yazı görselde), yoksa aynı boyutta renkli şerit + dikey yazı.
            var tab = HandbookUi.Rect("Tab_" + tabLabel, tabsRoot);
            HandbookUi.Size(tab, prefW: TabWidth, prefH: TabHeight);
            var sprite = TabSprite(tabLabel);
            var image = HandbookUi.Img(tab, sprite != null ? Color.white : color, raycast: true);
            image.sprite = sprite;
            var button = tab.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() =>
            {
                if (page == index) return;
                ShowPage(index);
                Toggled?.Invoke();
            });
            if (sprite == null)
            {
                var label = HandbookUi.Text("Label", tab, tabLabel, 12, Title, Color.white, TextAlignmentOptions.Center);
                var lr = label.rectTransform;
                lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 0.5f);
                lr.sizeDelta = new Vector2(TabHeight - 8, TabWidth);
                lr.localRotation = Quaternion.Euler(0, 0, -90); // dikey yazı
                label.enableAutoSizing = true;
                label.fontSizeMin = 8;
                label.fontSizeMax = 12;
                label.characterSpacing = 2;
            }
            tabs.Add((image, sprite != null ? Color.white : color));
        }

        private const float TabWidth = 40f, TabHeight = 88f; // EEE sekme görseli 120x300 oranına yakın; 6 sekme deftere sığar

        // "ZEMİN" → tab_zemin
        private Sprite TabSprite(string label)
        {
            if (tabSprites == null) return null;
            string key = "tab_" + label.Replace("İ", "I").Replace("Ş", "S").ToLowerInvariant();
            foreach (var s in tabSprites)
                if (s != null && s.name == key) return s;
            return null;
        }

        private GameObject NewPage(string name)
        {
            var p = HandbookUi.Rect(name, pageRoot);
            HandbookUi.VL(p, 8);
            HandbookUi.Size(p, flexH: 1);
            p.gameObject.SetActive(false);
            return p.gameObject;
        }

        // Başlık: "AFET KONTROL BÜROSU / DENETÇİ EL KİTABI" + sağda baskı bilgisi; altında 3 px çizgi.
        private void Header(Transform page, string title, string side)
        {
            var row = HandbookUi.Rect("Header", page);
            HandbookUi.HL(row, 10, new RectOffset(0, 0, 0, 6), align: TextAnchor.LowerLeft);
            var left = HandbookUi.Rect("Titles", row);
            HandbookUi.VL(left, 0);
            HandbookUi.Size(left, flexW: 1);
            var small = HandbookUi.Text("Small", left, "AFET KONTROL BÜROSU", 11, Bold, HandbookUi.InkSoft);
            small.characterSpacing = 6;
            HandbookUi.Text("Title", left, title, 26, Title, HandbookUi.Ink);
            HandbookUi.Text("Side", row, side, 11, textFont, HandbookUi.InkSoft, TextAlignmentOptions.BottomRight);
            var line = HandbookUi.Rect("Divider", page);
            HandbookUi.Img(line, HandbookUi.Ink);
            HandbookUi.Size(line, prefH: 3);
        }

        private GameObject BuildQuickPage(RuleSetSO rules, List<(ParamRule rep, int floors, string label)> groups, List<string> sections)
        {
            var page = NewPage("QuickLook");
            Header(page.transform, "DENETÇİ EL KİTABI", "Baskı 3\nHızlı Bakış");

            // 3 adım
            var steps = HandbookUi.Rect("Steps", page.transform);
            HandbookUi.HL(steps, 6, null, expandWidth: true);
            HandbookUi.Size(steps, prefH: 36);
            Step(steps, "1", "Formda <b>KAT ADEDİ</b>'ni oku");
            Step(steps, "2", "Aşağıdan kat grubunu seç");
            Step(steps, "3", "Her değeri bul: hepsi yeşil mi?");

            // Kat grubu düğmeleri (oyuncu seçer)
            var floors = HandbookUi.Rect("Floors", page.transform);
            HandbookUi.HL(floors, 8, null, expandWidth: true);
            HandbookUi.Size(floors, prefH: 42);
            for (int i = 0; i < groups.Count; i++)
            {
                int index = i;
                var b = HandbookUi.Rect("Floor" + i, floors);
                HandbookUi.Size(b, prefW: 0, flexW: 1, prefH: 42);
                var back = HandbookUi.Img(b, HandbookUi.NoteBg, raycast: true);
                HandbookUi.Border(back, HandbookUi.Ink, 3);
                var button = b.gameObject.AddComponent<Button>();
                button.targetGraphic = back;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => SelectFloorGroup(index));
                HandbookUi.HL(b, 8, new RectOffset(8, 8, 4, 4), align: TextAnchor.MiddleCenter);
                if (floorIcons != null && i < floorIcons.Length && floorIcons[i] != null)
                {
                    var icon = HandbookUi.Rect("Icon", b);
                    HandbookUi.Size(icon, prefW: 24, prefH: 30);
                    var img = HandbookUi.Img(icon, Color.white);
                    img.sprite = floorIcons[i];
                    img.preserveAspect = true;
                }
                var label = HandbookUi.Text("Label", b, groups[i].label, 17, Title, HandbookUi.Ink, TextAlignmentOptions.Center);
                floorButtons.Add((back, label));
            }

            // 5 satır (sekme sırasıyla)
            var rows = HandbookUi.Rect("Rows", page.transform);
            HandbookUi.VL(rows, 8);
            foreach (var section in sections)
                foreach (var param in ParamsIn(rules, section))
                {
                    var any = FirstRule(rules, param);
                    var color = HandbookUi.SectionColor(section);
                    var row = HandbookUi.Rect("Row_" + param, rows);
                    InspectSpot.Mark(row, InspectTarget.OfParam(InspectDoc.Handbook, param)); // inceleme modunda tıklanabilir
                    HandbookUi.Size(row, prefH: 46);
                    HandbookUi.Img(row, new Color(1f, 1f, 1f, 0.35f));
                    HandbookUi.HL(row, 10, new RectOffset(14, 8, 7, 7));
                    var stripe = HandbookUi.Rect("Stripe", row);
                    stripe.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                    stripe.anchorMin = Vector2.zero;
                    stripe.anchorMax = new Vector2(0, 1);
                    stripe.sizeDelta = new Vector2(6, 0);
                    stripe.pivot = new Vector2(0, 0.5f);
                    HandbookUi.Img(stripe, color);

                    NameBlock(row, section, DeskFormat.ParamName(rules, Type, param), color, 118);
                    quickScales.Add(HandbookUi.BuildScale(row, rules, param, any, Required(rules, param, groups), textFont, Bold, RowHeight));
                }

            HandbookUi.Size(HandbookUi.Rect("Spacer", page.transform), flexH: 1);

            // "Tek bir kırmızı yeter" kutusu
            var foot = HandbookUi.Rect("Foot", page.transform);
            var footBack = HandbookUi.Img(foot, HandbookUi.NoteBg);
            HandbookUi.Border(footBack, HandbookUi.Ink, 3);
            HandbookUi.HL(foot, 12, new RectOffset(12, 12, 8, 8));
            var stamp = HandbookUi.Text("Stamp", foot, "RED", 20, Title, HandbookUi.Bad, TextAlignmentOptions.Center);
            HandbookUi.Size(stamp, prefW: 58, prefH: 30);
            stamp.rectTransform.localRotation = Quaternion.Euler(0, 0, 6);
            var p = HandbookUi.Text("Text", foot,
                "<b>Tek bir kırmızı yeter.</b> Değer tutuyor ama belge şüpheliyse: laboratuvardan <b>ek test</b> iste, " +
                "form ile raporun parsel, tarih, kaşe ve imzasını karşılaştır.", 13, textFont, HandbookUi.Ink, TextAlignmentOptions.MidlineLeft, wrap: true);
            HandbookUi.Size(p, flexW: 1);
            return page;
        }

        private GameObject BuildSectionPage(RuleSetSO rules, List<(ParamRule rep, int floors, string label)> groups, string section)
        {
            var page = NewPage("Section_" + section);
            var color = HandbookUi.SectionColor(section);
            Header(page.transform, section, "En az değerler\nkat grubuna göre");

            foreach (var param in ParamsIn(rules, section))
            {
                var any = FirstRule(rules, param);
                var block = HandbookUi.Rect("Param_" + param, page.transform);
                HandbookUi.VL(block, 6, new RectOffset(14, 6, 8, 8));
                HandbookUi.Img(block, new Color(1f, 1f, 1f, 0.35f));
                var stripe = HandbookUi.Rect("Stripe", block);
                stripe.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                stripe.anchorMin = Vector2.zero;
                stripe.anchorMax = new Vector2(0, 1);
                stripe.sizeDelta = new Vector2(6, 0);
                stripe.pivot = new Vector2(0, 0.5f);
                HandbookUi.Img(stripe, color);

                HandbookUi.Text("Name", block, DeskFormat.ParamName(rules, Type, param), 17, Bold, HandbookUi.Ink);
                var required = Required(rules, param, groups);
                for (int g = 0; g < groups.Count; g++)
                {
                    var line = HandbookUi.Rect("Group" + g, block);
                    HandbookUi.HL(line, 10);
                    HandbookUi.Size(line, prefH: RowHeight + 4);
                    var lbl = HandbookUi.Text("Floors", line, groups[g].label, 13, Title, HandbookUi.Ink);
                    HandbookUi.Size(lbl, prefW: 70);
                    HandbookUi.BuildScale(line, rules, param, any, required, textFont, Bold, RowHeight).Paint(g);
                }
                if (any != null && !string.IsNullOrWhiteSpace(any.Description))
                    HandbookUi.Text("Description", block, any.Description.Trim(), 13, textFont, HandbookUi.InkSoft, wrap: true);
            }

            string note = NoteFor(section);
            if (!string.IsNullOrWhiteSpace(note))
            {
                var box = HandbookUi.Rect("Note", page.transform);
                var back = HandbookUi.Img(box, HandbookUi.NoteBg);
                HandbookUi.Border(back, color, 3);
                HandbookUi.VL(box, 2, new RectOffset(12, 12, 8, 8));
                var head = HandbookUi.Text("Head", box, "AFETE HAZIRLIK", 11, Bold, color);
                head.characterSpacing = 4;
                HandbookUi.Text("Text", box, note.Trim(), 14, textFont, HandbookUi.Ink, wrap: true);
            }
            HandbookUi.Size(HandbookUi.Rect("Spacer", page.transform), flexH: 1);
            return page;
        }

        private GameObject BuildLabPage(GameContext ctx)
        {
            var page = NewPage("Lab");
            Header(page.transform, "BELGE KONTROLÜ", "Laboratuvarlar\nve kaşeler");

            int testCost = ctx.Balance != null ? ctx.Balance.ExtraTestCost : 0;
            var sb = new StringBuilder();
            sb.AppendLine("• Formdaki parsel no, tarih ve imza lab raporundakiyle aynı olmalı.");
            sb.AppendLine("• Raporun başlığındaki laboratuvar ile kaşedeki aynı olmalı ve aşağıdaki listede bulunmalı.");
            sb.AppendLine($"• Beyana güvenmiyorsan lab raporundan ek test iste ({DeskFormat.Money(testCost)}): gerçek değerleri gösterir.");

            var text = HandbookUi.Text("Text", page.transform, sb.ToString().TrimEnd(), 16, textFont, HandbookUi.Ink,
                TextAlignmentOptions.TopLeft, wrap: true);
            text.enableAutoSizing = true;
            text.fontSizeMin = 11;
            text.fontSizeMax = 16;

            // Akredite liste ayrı blok: inceleme modunda tek tıklanabilir alan (lab adı / kaşe bununla karşılaştırılır).
            var list = new StringBuilder();
            list.AppendLine("<b>AKREDİTE LABORATUVARLAR</b>");
            var labs = ctx.Names != null ? ctx.Names.LabNames : null;
            if (labs == null || labs.Count == 0) list.AppendLine("• (liste yok)");
            else foreach (var lab in labs) list.AppendLine($"• {lab}");
            var labList = HandbookUi.Text("Labs", page.transform, list.ToString().TrimEnd(), 16, textFont, HandbookUi.Ink,
                TextAlignmentOptions.TopLeft, wrap: true);
            InspectSpot.Mark(labList, InspectTarget.Of(InspectDoc.Handbook, InspectField.AccreditedLabs));

            HandbookUi.Size(HandbookUi.Rect("Spacer", page.transform), flexH: 1);
            return page;
        }

        // ---------- Parçalar ----------

        private void Step(Transform parent, string number, string text)
        {
            var box = HandbookUi.Rect("Step" + number, parent);
            HandbookUi.Img(box, HandbookUi.Paper2);
            HandbookUi.HL(box, 7, new RectOffset(7, 7, 4, 4));
            HandbookUi.Size(box, prefW: 0, flexW: 1);
            var dot = HandbookUi.Rect("Number", box);
            HandbookUi.Size(dot, prefW: 22, prefH: 22);
            HandbookUi.Img(dot, HandbookUi.Ink);
            var n = HandbookUi.Text("Text", dot, number, 13, Title, HandbookUi.Paper, TextAlignmentOptions.Center);
            HandbookUi.Stretch(n.rectTransform);
            var t = HandbookUi.Text("Text", box, text, 12, textFont, HandbookUi.Ink, wrap: true);
            HandbookUi.Size(t, flexW: 1);
        }

        // Sol blok: renkli kare (bölümün baş harfi) + bölüm adı küçük + parametre adı kalın.
        private void NameBlock(Transform row, string section, string name, Color color, float width)
        {
            var block = HandbookUi.Rect("Name", row);
            HandbookUi.HL(block, 7);
            HandbookUi.Size(block, prefW: width);
            var ico = HandbookUi.Rect("Icon", block);
            HandbookUi.Size(ico, prefW: 28, prefH: 28);
            HandbookUi.Img(ico, color);
            var letter = HandbookUi.Text("Letter", ico, section.Substring(0, 1), 15, Title, Color.white, TextAlignmentOptions.Center);
            HandbookUi.Stretch(letter.rectTransform);
            var texts = HandbookUi.Rect("Texts", block);
            HandbookUi.VL(texts, 0);
            HandbookUi.Size(texts, flexW: 1);
            var sec = HandbookUi.Text("Section", texts, section, 9, Bold, color);
            sec.characterSpacing = 3;
            var n = HandbookUi.Text("Param", texts, name, 13, Bold, HandbookUi.Ink, wrap: true);
            n.lineSpacing = -20;
        }

        private void SelectFloorGroup(int index)
        {
            floorGroup = floorGroup == index ? -1 : index; // aynı düğme seçimi kaldırır
            PaintQuick();
            Toggled?.Invoke();
        }

        private void PaintQuick()
        {
            foreach (var scale in quickScales) scale.Paint(floorGroup);
            for (int i = 0; i < floorButtons.Count; i++)
            {
                bool on = i == floorGroup;
                floorButtons[i].back.color = on ? HandbookUi.Ink : HandbookUi.NoteBg;
                floorButtons[i].label.color = on ? HandbookUi.Paper : HandbookUi.Ink;
            }
        }

        // ---------- Veri ----------

        private static List<ParamType> ParamsIn(RuleSetSO rules, string section)
        {
            var list = new List<ParamType>();
            foreach (var r in rules.RulesFor(Type))
                if (r.Section == section && !list.Contains(r.Param)) list.Add(r.Param);
            list.Sort();
            return list;
        }

        private static ParamRule FirstRule(RuleSetSO rules, ParamType param)
        {
            foreach (var r in rules.RulesFor(Type))
                if (r.Param == param) return r;
            return null;
        }

        // Grup başına eşik: Core'un o kat sayısına uyguladığı kural (FindRule, temsilci kat).
        private static float[] Required(RuleSetSO rules, ParamType param, List<(ParamRule rep, int floors, string label)> groups)
        {
            var values = new float[groups.Count];
            for (int i = 0; i < groups.Count; i++)
            {
                var rule = rules.FindRule(Type, param, groups[i].floors);
                values[i] = rule != null ? rule.Required : 0f;
            }
            return values;
        }

        private string NoteFor(string section)
        {
            foreach (var n in sectionNotes)
                if (n != null && string.Equals(n.Section, section, StringComparison.OrdinalIgnoreCase)) return n.Text;
            return null;
        }
    }
}
