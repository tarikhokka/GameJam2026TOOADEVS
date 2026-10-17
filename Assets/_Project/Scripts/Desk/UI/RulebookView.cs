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
    // Sayfalı "Denetçi El Kitabı" (el kitabı v2): masadaki EL KİTABI kitabıyla (ya da açık dosyadaki butonla) açılır.
    // Sayfalar: giriş + belge kontrolü → yönetmelik tablosu (5 parametre × kat grubu, altında açıklamalar) → akredite laboratuvarlar.
    // Tablo her hücrede Core'un uyguladığı kuralı gösterir (ctx.Rules.FindRule). Sadece gösterir.
    public class RulebookView : MonoBehaviour
    {
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

            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
            {
                if (rules == null || !IsRelevant(ctx, type) || !HasRules(rules, type)) continue;
                pages.Add(($"Yönetmelik · {DeskFormat.BuildingName(type)}", RulesPage(rules, type)));
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

            // Okuma adımları HandbookIntro'da (A); burada belgelerin kendisine dair kontroller.
            int testCost = ctx.Balance != null ? ctx.Balance.ExtraTestCost : 0;
            sb.AppendLine("<b>Belge kontrolü</b>");
            sb.AppendLine("• Başvuru ile lab raporunda parsel no, tarih ve imza aynı olmalı.");
            sb.AppendLine("• Raporun başlığındaki laboratuvar ile kaşedeki aynı olmalı ve akredite listesinde bulunmalı.");
            sb.AppendLine($"• Beyana güvenmiyorsan ek test iste ({DeskFormat.Money(testCost)}): gerçek değerleri gösterir.");
            sb.AppendLine();
            sb.Append("<i>Sayfaları alttaki oklarla çevir.</i>");
            return sb.ToString();
        }

        // El kitabı v2 (A): tek tablo. Satır = parametre (lab raporuyla aynı sıra), sütun = kat grubu (FloorsLabel),
        // hücre = o kat grubunda kabul edilen en düşük değer (FindRule = Core'un projeye uyguladığı kural). Altında açıklamalar.
        private static string RulesPage(RuleSetSO rules, BuildingType type)
        {
            var groups = FloorGroups(rules, type);
            const float firstCol = 34f; // kitap dar (yan yana modda ekranın sağ kısmı): parametre adı + 3 kısa sütun
            float colWidth = (100f - firstCol) / groups.Count;
            var parameters = ParamsOf(rules, type);

            var sb = new StringBuilder();
            sb.Append("<b>Parametre");
            for (int i = 0; i < groups.Count; i++) sb.Append($"<pos={Pos(firstCol + i * colWidth)}%>{groups[i].FloorsLabel}");
            sb.AppendLine("</b>");

            foreach (var param in parameters)
            {
                sb.Append(DeskFormat.ParamName(rules, type, param));
                for (int i = 0; i < groups.Count; i++)
                {
                    int floors = groups[i].MinFloors > 0 ? groups[i].MinFloors : 1;
                    var rule = rules.FindRule(type, param, floors);
                    string cell = rule != null ? DeskFormat.Value(rules, param, rule.Required) : "—";
                    sb.Append($"<pos={Pos(firstCol + i * colWidth)}%>{cell}");
                }
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("<i>En düşük kabul edilen değerler. Seçenekli parametrelerde sağdaki seçenek daha iyidir; tablodakinden kötüsü uygunsuz.</i>");

            foreach (var param in parameters)
            {
                var rule = FirstRule(rules, type, param);
                if (rule == null || string.IsNullOrWhiteSpace(rule.Description)) continue;
                sb.AppendLine();
                sb.Append($"<size=85%><b>{DeskFormat.ParamName(rules, type, param)}</b>");
                if (rule.IsLabeled) sb.Append($" ({string.Join(" < ", rule.Labels)})");
                sb.Append($": {rule.Description.Trim()}</size>");
            }
            return sb.ToString().TrimEnd();
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

        private static ParamRule FirstRule(RuleSetSO rules, BuildingType type, ParamType param)
        {
            foreach (var r in rules.RulesFor(type))
                if (r.Param == param) return r;
            return null;
        }

        // Parametreler lab raporundaki sırayla (Core belgede ParamType sırasını kullanır).
        private static List<ParamType> ParamsOf(RuleSetSO rules, BuildingType type)
        {
            var list = new List<ParamType>();
            foreach (var r in rules.RulesFor(type))
                if (!list.Contains(r.Param)) list.Add(r.Param);
            list.Sort();
            return list;
        }

        // Kurallarda geçen kat grupları (ör. 1-3, 4-6, 7+), her gruptan bir temsilci kural (başlık: FloorsLabel).
        // Hiç kat koşulu yoksa tek sütun ("Tüm binalar").
        private static List<ParamRule> FloorGroups(RuleSetSO rules, BuildingType type)
        {
            var groups = new List<ParamRule>();
            ParamRule any = null;
            foreach (var r in rules.RulesFor(type))
            {
                any ??= r;
                if (r.MinFloors == 0 && r.MaxFloors == 0) continue;
                if (!groups.Exists(g => g.MinFloors == r.MinFloors && g.MaxFloors == r.MaxFloors)) groups.Add(r);
            }
            if (groups.Count == 0 && any != null) groups.Add(any);
            groups.Sort((a, b) => a.MinFloors.CompareTo(b.MinFloors));
            return groups;
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
