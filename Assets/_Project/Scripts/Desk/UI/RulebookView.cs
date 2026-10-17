using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Sayfalı "Denetçi El Kitabı" (Ç1): masadaki EL KİTABI kitabıyla (ya da açık dosyadaki butonla) açılır.
    // Sayfalar: giriş + kontrol listesi → mahalle → bölge tablosu → parametreler → bölge 1-4 → akredite laboratuvarlar.
    // Bölge sayfası her parametre ve kat grubu için Core'un uyguladığı kuralı gösterir (ctx.Rules.FindRule). Sadece gösterir.
    public class RulebookView : MonoBehaviour
    {
        private const int ZoneCount = 4;

        // 1920x1080 tuval: tek başına ortada; dosya da açıksa sağ kenara yaslanır (dosya sola kayar).
        private static readonly Vector2 CenterPos = new Vector2(0, -40);
        private static readonly Vector2 DockedPos = new Vector2(560, -40);
        private static readonly Vector2 FromHandbook = new Vector2(500, -900); // masadaki EL KİTABI kitabı sağ altta

        [SerializeField] private Button openButton;        // masadaki EL KİTABI kitabı
        [SerializeField] private Button dossierOpenButton; // açık dosyanın içindeki "El Kitabı'na bak" (aç/kapat)
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private DraggableSheet book;      // kayarak gelir/gider, sürüklenir
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text pageText;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;

        public event Action Toggled;     // açma/kapama ve sayfa çevirme (kağıt sesi)
        public event Action OpenChanged; // sadece açma/kapama (DeskScreen yerleşimi)

        private readonly List<(string title, string body)> pages = new List<(string, string)>();
        private GameContext context;
        private int page;
        private bool open;

        public bool IsOpen => open;
        public DraggableSheet Sheet => book;

        // Perde yok: kitap açıkken masa ve dosya tıklanabilir kalır (yan yana karşılaştırma).
        private void Awake()
        {
            openButton.onClick.AddListener(() => SetOpen(!open));
            if (dossierOpenButton != null) dossierOpenButton.onClick.AddListener(() => SetOpen(!open));
            closeButton.onClick.AddListener(() => SetOpen(false));
            prevButton.onClick.AddListener(() => Turn(-1));
            nextButton.onClick.AddListener(() => Turn(1));
        }

        public void Bind(GameContext gameContext)
        {
            context = gameContext;
            page = 0;
            Close(instant: true);
        }

        // instant: gün başı / ekran gizlenirken (animasyon bekleyemez).
        public void Close(bool instant = false)
        {
            if (!open && !panelRoot.activeSelf) return;
            bool wasOpen = open;
            open = false;
            if (wasOpen) OpenChanged?.Invoke();

            if (instant || !isActiveAndEnabled || !panelRoot.activeSelf) panelRoot.SetActive(false);
            else book.AnimateOut(FromHandbook, () => { if (!open) panelRoot.SetActive(false); });
        }

        // Dosya da açıldı: sağ kenara kay. Ortaya sadece kitap tek başına açılırken gelinir (SetOpen).
        public void Dock()
        {
            if (open) book.SetHome(DockedPos, 1f);
        }

        // Kitap kapanınca kalınan sayfa hatırlanır (aynı gün içinde aynı tabloya dönmek için).
        private void SetOpen(bool value)
        {
            if (open == value) return;
            Toggled?.Invoke();
            if (!value)
            {
                Close();
                return;
            }

            if (context != null)
            {
                Build(context);
                ShowPage();
            }
            open = true;
            panelRoot.SetActive(true);
            book.SetHome(CenterPos, 1f);
            OpenChanged?.Invoke(); // yerleşim önce (dosya açıksa Dock), kitap sonra oraya kayar
            book.AnimateIn(FromHandbook);
        }

        private void Turn(int delta)
        {
            int next = Mathf.Clamp(page + delta, 0, pages.Count - 1);
            if (next == page) return;
            page = next;
            ShowPage();
            Toggled?.Invoke(); // sayfa çevirme sesi
        }

        private void ShowPage()
        {
            if (pages.Count == 0) return;
            page = Mathf.Clamp(page, 0, pages.Count - 1);
            titleText.text = pages[page].title;
            bodyText.text = pages[page].body;
            pageText.text = $"{page + 1} / {pages.Count}";
            prevButton.interactable = page > 0;
            nextButton.interactable = page < pages.Count - 1;
        }

        private void Build(GameContext ctx)
        {
            pages.Clear();
            var rules = ctx.Rules;

            pages.Add(("Denetçi El Kitabı", IntroPage(ctx)));
            if (rules != null && rules.Zones.Count > 0) pages.Add(("Mahalleler ve deprem bölgeleri", ZonesPage(rules)));

            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
            {
                if (rules == null || !IsRelevant(ctx, type) || !HasRules(rules, type)) continue;
                string building = DeskFormat.BuildingName(type);
                pages.Add(($"Parametreler · {building}", ParamsPage(rules, type)));

                if (rules.Zones.Count == 0) pages.Add(($"Yönetmelik · {building}", ZonePage(rules, type, 0)));
                else for (int zone = 1; zone <= ZoneCount; zone++)
                        pages.Add(($"{zone}. Bölge · {building}", ZonePage(rules, type, zone)));
            }

            pages.Add(("Akredite laboratuvarlar", LabsPage(ctx)));
        }

        private static string IntroPage(GameContext ctx)
        {
            var sb = new StringBuilder();
            string intro = ctx.Rules != null ? ctx.Rules.HandbookIntro : null;
            if (!string.IsNullOrWhiteSpace(intro))
            {
                sb.AppendLine(intro.Trim());
                sb.AppendLine();
            }

            int testCost = ctx.Balance != null ? ctx.Balance.ExtraTestCost : 0;
            sb.AppendLine("<b>Kontrol listesi</b>");
            sb.AppendLine("1. Adresteki mahalleyi \"Mahalleler\" sayfasında bul, deprem bölgesine bak.");
            sb.AppendLine("2. O bölgenin sayfasında yapının kat sayısına uyan sütunu bul; beyan edilen değerleri oradaki en düşük değerlerle karşılaştır.");
            sb.AppendLine("3. Başvuru ile lab raporunda parsel no, tarih ve imza aynı olmalı.");
            sb.AppendLine("4. Raporun başlığındaki laboratuvar ile kaşedeki aynı olmalı ve akredite listesinde bulunmalı.");
            sb.AppendLine($"5. Beyana güvenmiyorsan ek test iste ({DeskFormat.Money(testCost)}): gerçek değerleri gösterir.");
            sb.AppendLine();
            sb.Append("<i>Sayfaları alttaki oklarla çevir.</i>");
            return sb.ToString();
        }

        private static string ZonesPage(RuleSetSO rules)
        {
            var zones = new List<ZoneEntry>(rules.Zones);
            var tr = CultureInfo.GetCultureInfo("tr-TR");
            zones.Sort((a, b) => string.Compare(a.Neighborhood, b.Neighborhood, tr, CompareOptions.None));

            var sb = new StringBuilder();
            sb.AppendLine("<b>Mahalle<pos=55%>Deprem bölgesi</b>");
            foreach (var z in zones) sb.AppendLine($"{z.Neighborhood}<pos=55%>{z.Zone}. bölge");
            sb.AppendLine();
            sb.Append("<i>1. bölge en riskli bölgedir.</i>");
            return sb.ToString();
        }

        private static string ParamsPage(RuleSetSO rules, BuildingType type)
        {
            var sb = new StringBuilder();
            foreach (var param in ParamsOf(rules, type))
            {
                string description = null;
                foreach (var r in rules.RulesFor(type))
                    if (r.Param == param && !string.IsNullOrWhiteSpace(r.Description)) { description = r.Description.Trim(); break; }

                sb.AppendLine($"<b>{DeskFormat.ParamName(rules, type, param)}</b>");
                if (description != null) sb.AppendLine($"<size=85%>{description}</size>");
            }
            return sb.ToString().TrimEnd();
        }

        // Tablo: satır = parametre, sütun = kat grubu; hücre = bu bölge ve kat grubunda aranan en düşük değer.
        private static string ZonePage(RuleSetSO rules, BuildingType type, int zone)
        {
            var groups = FloorGroups(rules, type);
            const float firstCol = 54f; // kitap dar (yan yana modda ekranın sağ yarısı): parametre adı + 2 kısa sütun
            float colWidth = (100f - firstCol) / groups.Count;

            var sb = new StringBuilder();
            sb.Append("<b>Parametre");
            for (int i = 0; i < groups.Count; i++) sb.Append($"<pos={Pos(firstCol + i * colWidth)}%>{FloorLabel(groups[i])}");
            sb.AppendLine("</b>");

            foreach (var param in ParamsOf(rules, type))
            {
                sb.Append(DeskFormat.ParamName(rules, type, param));
                for (int i = 0; i < groups.Count; i++)
                {
                    int floors = groups[i].min > 0 ? groups[i].min : 1;
                    var rule = rules.FindRule(type, param, zone, floors);
                    string cell = rule != null ? DeskFormat.Value(rules, param, rule.Required) : "—";
                    sb.Append($"<pos={Pos(firstCol + i * colWidth)}%>{cell}");
                }
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.Append("<i>Değerler kabul edilen en düşük değerlerdir; beyan bunun altındaysa uygunsuz. \"—\": bu kat grubunda aranmaz.</i>");
            return sb.ToString();
        }

        private static string LabsPage(GameContext ctx)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Sadece bu laboratuvarların raporları geçerlidir:");
            sb.AppendLine();
            var labs = ctx.Names != null ? ctx.Names.LabNames : null;
            if (labs == null || labs.Count == 0) sb.AppendLine("• (liste yok)");
            else foreach (var lab in labs) sb.AppendLine($"• {lab}");
            return sb.ToString().TrimEnd();
        }

        // Kural listesinde ilk geçiş sırasıyla parametreler.
        private static List<ParamType> ParamsOf(RuleSetSO rules, BuildingType type)
        {
            var list = new List<ParamType>();
            foreach (var r in rules.RulesFor(type))
                if (!list.Contains(r.Param)) list.Add(r.Param);
            list.Sort();
            return list;
        }

        // Kurallarda geçen kat aralıkları (ör. 1-4, 5+); hiç yoksa tek sütun "Tüm katlar".
        private static List<(int min, int max)> FloorGroups(RuleSetSO rules, BuildingType type)
        {
            var groups = new List<(int min, int max)>();
            foreach (var r in rules.RulesFor(type))
            {
                if (r.MinFloors == 0 && r.MaxFloors == 0) continue;
                var g = (r.MinFloors, r.MaxFloors);
                if (!groups.Contains(g)) groups.Add(g);
            }
            if (groups.Count == 0) groups.Add((0, 0));
            groups.Sort((a, b) => a.min.CompareTo(b.min));
            return groups;
        }

        private static string FloorLabel((int min, int max) g)
        {
            if (g.min == 0 && g.max == 0) return "Tüm katlar";
            if (g.max == 0) return $"{g.min}+ kat";
            return $"{Math.Max(1, g.min)}-{g.max} kat";
        }

        private static string Pos(float percent) => percent.ToString("0.#", CultureInfo.InvariantCulture);

        private static bool HasRules(RuleSetSO rules, BuildingType type)
        {
            foreach (var _ in rules.RulesFor(type)) return true;
            return false;
        }

        // Konut her gün; diğer türler o gün masada varsa ya da günün ayarı izin veriyorsa gösterilir.
        private static bool IsRelevant(GameContext context, BuildingType type)
        {
            if (type == BuildingType.Residential) return true;
            if (type == BuildingType.Factory && context.DayConfig != null && context.DayConfig.AllowFactories) return true;
            return context.TodayProjects.Exists(p => p.BuildingType == type);
        }
    }
}
