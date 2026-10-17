using System;
using System.Collections.Generic;
using System.Text;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Denetçi El Kitabı (Ahmet'in tasarımı Docs/Tasarim/denetci_el_kitabi_GORUNUM.html, içerik El Kitabı v2).
    // Masadaki EL KİTABI kitabıyla açılır/kapanır; pencere işleri PaperWindow'da (yan yana yerleşim, sürükleme).
    // Sayfa 1: bölüm kutuları (ParamRule.Section), numaralı kurallar "1.1 Kolon kalınlığı en az: 1-3 kat 30 cm · 4-6 kat 35 cm · 7+ kat 40 cm"
    //          (değerler FindRule = Core'un uyguladığı kural), seçenek sırası + açıklama, GENEL KURAL kutusu.
    // Sayfa 2: nasıl kullanılır (HandbookIntro), belge kontrolü, akredite laboratuvarlar. Sadece gösterir.
    public class RulebookView : MonoBehaviour
    {
        [SerializeField] private Button openButton; // masadaki EL KİTABI kitabı
        [SerializeField] private PaperWindow window;

        [Header("Sayfa 1: kurallar")]
        [SerializeField] private GameObject rulesPage;
        [SerializeField] private Transform sectionsContainer;
        [SerializeField] private SectionBlockView sectionTemplate;

        [Header("Sayfa 2: kontrol + laboratuvarlar")]
        [SerializeField] private GameObject notesPage;
        [SerializeField] private TMP_Text notesText;

        [Header("Sayfa çevirme")]
        [SerializeField] private TMP_Text pageText;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;

        private const int PageCount = 2;
        private const string NoteColor = "#5b4a36";

        public event Action Toggled; // açma/kapama ve sayfa çevirme (kağıt sesi)

        private readonly List<SectionBlockView> sections = new List<SectionBlockView>();
        private GameContext context;
        private int page;

        public PaperWindow Window => window;

        private void Awake()
        {
            openButton.onClick.AddListener(Toggle);
            window.CloseClicked += () => { if (window.IsOpen) Toggle(); };
            prevButton.onClick.AddListener(() => Turn(-1));
            nextButton.onClick.AddListener(() => Turn(1));
        }

        public void Bind(GameContext gameContext)
        {
            context = gameContext;
            page = 0;
            window.Close(instant: true);
        }

        public void Close(bool instant = false) => window.Close(instant);

        // Kitap kapanınca kalınan sayfa hatırlanır (aynı gün içinde aynı kurala dönmek için).
        private void Toggle()
        {
            Toggled?.Invoke();
            if (window.IsOpen)
            {
                window.Close();
                return;
            }
            if (context != null) Build(context);
            ShowPage();
            window.Open();
        }

        private void Turn(int delta)
        {
            int next = Mathf.Clamp(page + delta, 0, PageCount - 1);
            if (next == page) return;
            page = next;
            ShowPage();
            Toggled?.Invoke();
        }

        private void ShowPage()
        {
            rulesPage.SetActive(page == 0);
            notesPage.SetActive(page == 1);
            pageText.text = $"{page + 1} / {PageCount}";
            prevButton.interactable = page > 0;
            nextButton.interactable = page < PageCount - 1;
        }

        private void Build(GameContext ctx)
        {
            BuildRules(ctx);
            notesText.text = Notes(ctx);
        }

        // Bölüm → kurallar. Numara masada üretilir (bölüm sırası.kural sırası).
        private void BuildRules(GameContext ctx)
        {
            var rules = ctx.Rules;
            var order = new List<string>();
            var lines = new Dictionary<string, List<(string, string)>>();

            if (rules != null)
            {
                foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
                {
                    if (!IsRelevant(ctx, type)) continue;
                    var groups = FloorGroups(rules, type);
                    foreach (var param in ParamsOf(rules, type))
                    {
                        var first = FirstRule(rules, type, param);
                        string section = first != null && !string.IsNullOrWhiteSpace(first.Section) ? first.Section : "A · YAPI";
                        if (!lines.TryGetValue(section, out var list))
                        {
                            list = new List<(string, string)>();
                            lines[section] = list;
                            order.Add(section);
                        }
                        list.Add(("", RuleText(rules, type, param, first, groups)));
                    }
                }
            }
            order.Sort(StringComparer.Ordinal);

            UiPool.Resize(sections, sectionTemplate, sectionsContainer, order.Count);
            for (int s = 0; s < order.Count; s++)
            {
                var list = lines[order[s]];
                for (int i = 0; i < list.Count; i++) list[i] = ($"{s + 1}.{i + 1}", list[i].Item2);
                sections[s].Bind(order[s], list);
            }
        }

        // "Kolon kalınlığı en az: 1-3 kat 30 cm · 4-6 kat 35 cm · 7+ kat 40 cm" + (seçenekliyse sıra) + açıklama.
        private static string RuleText(RuleSetSO rules, BuildingType type, ParamType param, ParamRule first,
            List<ParamRule> groups)
        {
            var sb = new StringBuilder();
            sb.Append(DeskFormat.ParamName(rules, type, param)).Append(" en az: ");
            for (int i = 0; i < groups.Count; i++)
            {
                int floors = groups[i].MinFloors > 0 ? groups[i].MinFloors : 1;
                var rule = rules.FindRule(type, param, floors);
                if (i > 0) sb.Append(" · ");
                sb.Append(groups[i].FloorsLabel).Append(' ')
                  .Append(rule != null ? DeskFormat.Value(rules, param, rule.Required) : "—");
            }

            var note = new StringBuilder();
            if (first != null && first.IsLabeled) note.Append("Sıra (kötüden iyiye): ").Append(string.Join(" < ", first.Labels)).Append(". ");
            if (first != null && !string.IsNullOrWhiteSpace(first.Description)) note.Append(first.Description.Trim());
            if (note.Length > 0) sb.Append($"\n<size=78%><color={NoteColor}>{note}</color></size>");
            return sb.ToString();
        }

        private static string Notes(GameContext ctx)
        {
            var sb = new StringBuilder();
            string intro = ctx.Rules != null ? ctx.Rules.HandbookIntro : null;
            if (!string.IsNullOrWhiteSpace(intro))
            {
                sb.AppendLine("<b>NASIL KULLANILIR</b>");
                sb.AppendLine(intro.Trim());
                sb.AppendLine();
            }

            int testCost = ctx.Balance != null ? ctx.Balance.ExtraTestCost : 0;
            sb.AppendLine("<b>BELGE KONTROLÜ</b>");
            sb.AppendLine("• Formdaki parsel no, tarih ve imza lab raporundakiyle aynı olmalı.");
            sb.AppendLine("• Raporun başlığındaki laboratuvar ile kaşedeki aynı olmalı ve aşağıdaki listede bulunmalı.");
            sb.AppendLine($"• Beyana güvenmiyorsan lab raporundan ek test iste ({DeskFormat.Money(testCost)}): gerçek değerleri gösterir.");
            sb.AppendLine();

            sb.AppendLine("<b>AKREDİTE LABORATUVARLAR</b>");
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

        // Kurallarda geçen kat grupları (ör. 1-3, 4-6, 7+), her gruptan bir temsilci kural (FloorsLabel).
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

        // Konut her gün; diğer türler o gün masada varsa ya da günün ayarı izin veriyorsa.
        private static bool IsRelevant(GameContext context, BuildingType type)
        {
            if (type == BuildingType.Residential) return true;
            if (type == BuildingType.Factory && context.DayConfig != null && context.DayConfig.AllowFactories) return true;
            return context.TodayProjects.Exists(p => p.BuildingType == type);
        }
    }
}
