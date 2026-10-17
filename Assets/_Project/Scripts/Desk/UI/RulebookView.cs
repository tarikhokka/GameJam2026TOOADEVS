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
    // Sağ kenarda sekmeler: HIZLI BAKIŞ · LAB. (Poliş, Ahmet kararı 18 Ekim: BETON / DONATI / YAPI / ZEMİN sekmeleri ve sayfaları kalktı.)
    // Hızlı Bakış: kat grubu formdaki kat adedinden kendiliğinden seçilir (ShowProject, Core Inspection.FloorGroupOf; poliş K1);
    // 1-3 / 4-6 / 7+ düğmeleri sadece gösterge, tıklanmaz. 5 parametre kırmızı/yeşil çip ya da cetvel olarak o grubun eşiğine göre
    // boyanır; müteahhit yokken nötr. Alttaki "Tek bir kırmızı yeter" kutusu kalktı, satırlar sayfayı doldurur (poliş madde 5).
    // Sayfalar çalışırken kurallardan kurulur (HandbookUi); kural değeri kodda yazılmaz, hiçbir şey "bu projede yanlış" diye işaretlenmez.
    public class RulebookView : MonoBehaviour
    {
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

        [Tooltip("Sekme görselleri (tab_<sekme>, yazı görselde). Bölüm sekmeleri kalktığı için şu an kullanılan yok (tab_beton/donati/" +
                 "yapi/zemin artık gösterilmez); görseli olmayan sekme (HIZLI BAKIŞ, LAB) kodla çizilir.")]
        [SerializeField] private Sprite[] tabSprites;

        private const BuildingType Type = BuildingType.Residential;
        private const float RowHeight = 44f; // (EEE, 18 Ekim) 30 → 44: yazılar 1,35 kat büyüdü (HandbookUi.FontScale)
        // (Poliş madde 5) RED kutusu kalkınca Hızlı Bakış satırları ve içindeki çip / cetvel / yazılar bu kadar büyür.
        private const float QuickScale = 1.25f;

        public event Action Toggled; // açma/kapama ve sekme değişimi (kağıt sesi)

        public PaperWindow Window => window;

        // Gösterilen kat grubu (0 = 1-3, 1 = 4-6, 2 = 7+; müteahhit yoksa Inspection.NoFloorGroup). Gruplar küçükten büyüğe sıralı
        // (HandbookUi.FloorGroups), Core'un sırasıyla aynı. İnceleme bunu kullanmaz: Core binanın grubunu kendisi bulur (poliş K1).
        public int SelectedFloorGroup => floorGroup < 0 ? Inspection.NoFloorGroup : floorGroup;

        private GameContext context;
        private bool built;
        private int floorGroup = -1; // binanın kat grubu; -1 = müteahhit yok (nötr)
        private int page;
        private readonly List<GameObject> pages = new List<GameObject>();
        private readonly List<(Image image, Color color)> tabs = new List<(Image, Color)>();
        private readonly List<HandbookUi.Scale> quickScales = new List<HandbookUi.Scale>();
        private readonly List<(Image back, TextMeshProUGUI label, Image icon)> floorButtons = new List<(Image, TextMeshProUGUI, Image)>();

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

        // Yeni müteahhit (ya da müteahhit yok: null): Hızlı Bakış binanın kat grubunu kendiliğinden gösterir (poliş K1, Ahmet kararı).
        public void ShowProject(ProjectData project)
        {
            int group = project != null ? Inspection.FloorGroupOf(context, project) : Inspection.NoFloorGroup;
            if (group == floorGroup) return;
            floorGroup = group;
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
            AddPage("LAB", HandbookUi.InkSoft, BuildLabPage(ctx)); // (poliş madde 6) bölüm sayfaları kalktı
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
                label.fontSizeMin = 8 * HandbookUi.FontScale;
                label.fontSizeMax = 12 * HandbookUi.FontScale;
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
            HandbookUi.Size(steps, prefH: 60);
            Step(steps, "1", "Formda <b>KAT ADEDİ</b>'ne bak");
            Step(steps, "2", "Kat grubu <b>otomatik</b>");
            Step(steps, "3", "Her değeri bul: hepsi yeşil mi?");

            // Kat grubu göstergesi (poliş K1): binanın grubu koyu, diğerleri soluk; tıklanmaz.
            var floors = HandbookUi.Rect("Floors", page.transform);
            HandbookUi.HL(floors, 8, null, expandWidth: true);
            HandbookUi.Size(floors, prefH: 60);
            for (int i = 0; i < groups.Count; i++)
            {
                var b = HandbookUi.Rect("Floor" + i, floors);
                HandbookUi.Size(b, prefW: 0, flexW: 1, prefH: 60);
                var back = HandbookUi.Img(b, HandbookUi.NoteBg);
                HandbookUi.Border(back, HandbookUi.Ink, 3);
                HandbookUi.HL(b, 8, new RectOffset(8, 8, 4, 4), align: TextAnchor.MiddleCenter);
                Image img = null;
                if (floorIcons != null && i < floorIcons.Length && floorIcons[i] != null)
                {
                    var icon = HandbookUi.Rect("Icon", b);
                    HandbookUi.Size(icon, prefW: 32, prefH: 44);
                    img = HandbookUi.Img(icon, Color.white);
                    img.sprite = floorIcons[i];
                    img.preserveAspect = true;
                }
                var label = HandbookUi.Text("Label", b, groups[i].label, 19, Title, HandbookUi.Ink, TextAlignmentOptions.Center);
                floorButtons.Add((back, label, img));
            }

            // 5 satır (kural sırasıyla); RED kutusu kalktığı için sayfanın kalanını doldurur (poliş madde 5)
            var rows = HandbookUi.Rect("Rows", page.transform);
            HandbookUi.VL(rows, 8);
            HandbookUi.Size(rows, flexH: 1);
            foreach (var section in sections)
                foreach (var param in ParamsIn(rules, section))
                {
                    var any = FirstRule(rules, param);
                    var color = HandbookUi.SectionColor(section);
                    var row = HandbookUi.Rect("Row_" + param, rows);
                    InspectSpot.Mark(row, InspectTarget.OfParam(InspectDoc.Handbook, param)); // inceleme modunda tıklanabilir
                    HandbookUi.Size(row, prefH: 64 * QuickScale, flexH: 1);
                    HandbookUi.Img(row, new Color(1f, 1f, 1f, 0.35f));
                    HandbookUi.HL(row, 10, new RectOffset(14, 8, 7, 7));
                    var stripe = HandbookUi.Rect("Stripe", row);
                    stripe.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                    stripe.anchorMin = Vector2.zero;
                    stripe.anchorMax = new Vector2(0, 1);
                    stripe.sizeDelta = new Vector2(6, 0);
                    stripe.pivot = new Vector2(0, 0.5f);
                    HandbookUi.Img(stripe, color);

                    NameBlock(row, section, DeskFormat.ParamName(rules, Type, param), color, 128);
                    quickScales.Add(HandbookUi.BuildScale(row, rules, param, any, Required(rules, param, groups), textFont, Bold,
                        RowHeight * QuickScale, QuickScale));
                }
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
            text.fontSizeMin = 11 * HandbookUi.FontScale;
            text.fontSizeMax = 16 * HandbookUi.FontScale;

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
            HandbookUi.Size(dot, prefW: 28, prefH: 28);
            HandbookUi.Img(dot, HandbookUi.Ink);
            var n = HandbookUi.Text("Text", dot, number, 13, Title, HandbookUi.Paper, TextAlignmentOptions.Center);
            HandbookUi.Stretch(n.rectTransform);
            // (EEE) Adım yazıları kalın ve büyük; kutuya sığmazsa biraz küçülür
            var t = HandbookUi.Text("Text", box, text, 15, Bold, HandbookUi.Ink, wrap: true);
            t.enableAutoSizing = true;
            t.fontSizeMin = 11;
            t.fontSizeMax = 15 * HandbookUi.FontScale;
            t.lineSpacing = -10;
            HandbookUi.Size(t, flexW: 1);
        }

        // Sol blok: renkli kare (bölümün baş harfi) + bölüm adı küçük + parametre adı kalın.
        private void NameBlock(Transform row, string section, string name, Color color, float width)
        {
            var block = HandbookUi.Rect("Name", row);
            HandbookUi.HL(block, 7);
            // flexW 0: blok sabit genişlikte kalsın (yoksa içindeki esnek yazı yüzünden kalan yeri çiplerle yarı yarıya paylaşıyor,
            // çipler daralıp yazıları küçülüyordu — EEE)
            HandbookUi.Size(block, prefW: width, flexW: 0);
            var ico = HandbookUi.Rect("Icon", block);
            HandbookUi.Size(ico, prefW: 40, prefH: 40);
            HandbookUi.Img(ico, color);
            var letter = HandbookUi.Text("Letter", ico, section.Substring(0, 1), 17, Title, Color.white, TextAlignmentOptions.Center);
            HandbookUi.Stretch(letter.rectTransform);
            var texts = HandbookUi.Rect("Texts", block);
            HandbookUi.VL(texts, 0);
            HandbookUi.Size(texts, flexW: 1);
            var sec = HandbookUi.Text("Section", texts, section, 10, Bold, color);
            sec.characterSpacing = 3;
            var n = HandbookUi.Text("Param", texts, name, 15, Bold, HandbookUi.Ink, wrap: true);
            n.lineSpacing = -20;
        }

        private void PaintQuick()
        {
            foreach (var scale in quickScales) scale.Paint(floorGroup);
            for (int i = 0; i < floorButtons.Count; i++)
            {
                bool on = i == floorGroup;
                var (back, label, icon) = floorButtons[i];
                back.color = on ? HandbookUi.Ink : new Color(HandbookUi.NoteBg.r, HandbookUi.NoteBg.g, HandbookUi.NoteBg.b, 0.45f);
                label.color = on ? HandbookUi.Paper : new Color(HandbookUi.Ink.r, HandbookUi.Ink.g, HandbookUi.Ink.b, 0.35f);
                if (icon != null) icon.color = new Color(1f, 1f, 1f, on ? 1f : 0.35f);
                var outline = back.GetComponent<Outline>();
                if (outline != null) outline.effectColor = new Color(HandbookUi.Ink.r, HandbookUi.Ink.g, HandbookUi.Ink.b, on ? 1f : 0.3f);
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
    }
}
