using System.Collections.Generic;
using System.Text;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    /// <summary>
    /// Gazete ekranı (GamePhase.Result). Sadece afet olan gün açılır.
    /// Veri ctx.LastQuake'ten okunur; hesap yapılmaz. Şablon: newspaper_bg.png (1200x900).
    /// </summary>
    public class ResultScreen : FlowScreen
    {
        [SerializeField] Sprite background;     // newspaper_bg
        [SerializeField] int maxVictimNames = 6;

        public override GamePhase Phase => GamePhase.Result;

        // newspaper_bg.png içindeki kutular (piksel, 1200x900)
        const float W = 1200f, H = 900f, Pad = 12f;
        static readonly float[] HeadlineBox = { 56, 216, 1144, 346 };
        static readonly float[] StoryBox = { 56, 370, 396, 854 };                 // sol uzun kutu
        static readonly float[][] BuildingBoxes =
        {
            new float[] { 432, 370, 768, 677 },    // orta üst
            new float[] { 804, 370, 1144, 658 },   // sağ üst
            new float[] { 432, 706, 768, 854 },    // orta alt
            new float[] { 804, 699, 1144, 854 },   // sağ alt
        };

        TextMeshProUGUI _headline, _story;
        readonly List<TextMeshProUGUI> _buildingTexts = new List<TextMeshProUGUI>();
        Button _continue;

        protected override void Build()
        {
            var root = UiKit.Stretch(transform, "Root");
            UiKit.Image(root, "Table", null, UiKit.Dark, Vector2.zero, Vector2.one);

            var paper = UiKit.Stretch(root, "Paper");
            var fitter = paper.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = W / H;
            var bg = paper.gameObject.AddComponent<Image>();
            bg.sprite = background;
            bg.color = background != null ? Color.white : UiKit.Paper;
            bg.raycastTarget = false;

            _headline = BoxText(paper, "Headline", HeadlineBox, 64, TextAlignmentOptions.Center, 28);
            _headline.fontStyle = FontStyles.Bold;
            _story = BoxText(paper, "Story", StoryBox, 28, TextAlignmentOptions.TopLeft, 14);
            for (int i = 0; i < BuildingBoxes.Length; i++)
                _buildingTexts.Add(BoxText(paper, "Building" + i, BuildingBoxes[i], 26, TextAlignmentOptions.TopLeft, 11));

            _continue = UiKit.Button(root, "Continue", "Hesaplaşmaya geç  →", Finish,
                new Vector2(0.74f, 0.02f), new Vector2(0.98f, 0.09f), 30f);
        }

        static TextMeshProUGUI BoxText(Transform parent, string name, float[] box, float size,
            TextAlignmentOptions align, float minSize)
        {
            var min = new Vector2((box[0] + Pad) / W, 1f - (box[3] - Pad) / H);
            var max = new Vector2((box[2] - Pad) / W, 1f - (box[1] + Pad) / H);
            return UiKit.Text(parent, name, "", size, UiKit.Ink, align, min, max, true, minSize);
        }

        protected override void Refresh()
        {
            var quake = Context.LastQuake;
            _headline.text = PickHeadline(quake);
            _story.text = BuildStory(quake);

            // Yıkılanlar önce, sonra ayakta kalanlar
            var outcomes = new List<BuildingOutcome>();
            if (quake != null)
            {
                foreach (var o in quake.Outcomes) if (o.Collapsed) outcomes.Add(o);
                foreach (var o in quake.Outcomes) if (!o.Collapsed) outcomes.Add(o);
            }

            int slots = _buildingTexts.Count;
            for (int i = 0; i < slots; i++)
            {
                var t = _buildingTexts[i];
                bool overflowSlot = outcomes.Count > slots && i == slots - 1;
                if (overflowSlot)
                {
                    t.text = $"<b>Ve {outcomes.Count - (slots - 1)} bina daha</b>\n" + SummarizeRest(outcomes, slots - 1);
                }
                else if (i < outcomes.Count)
                {
                    t.text = BuildBuilding(outcomes[i]);
                }
                else
                {
                    t.text = "";
                }
            }
        }

        // ---------- manşet ----------

        string PickHeadline(QuakeResult quake)
        {
            var h = Context.Headlines;
            if (quake == null) return "";

            int collapsed = quake.CollapsedCount;
            bool bribed = quake.Outcomes.Exists(o => o.Collapsed && o.WasBribed);
            List<string> pool = null;
            if (h != null)
            {
                if (collapsed == 0) pool = h.NoCollapse;
                else if (bribed && h.BribedCollapse.Count > 0) pool = h.BribedCollapse;
                else if (collapsed == 1) pool = h.SingleCollapse;
                else pool = h.MultiCollapse;
            }

            string template;
            if (pool != null && pool.Count > 0)
            {
                // Aynı gün/seed için hep aynı manşet (ekran tekrar açılırsa değişmesin)
                var rng = new System.Random(unchecked(Context.State.Seed * 31 + quake.Day));
                template = pool[rng.Next(pool.Count)];
            }
            else
            {
                template = collapsed == 0 ? "Deprem korkuttu, binalar ayakta kaldı" : "Depremde {count} bina yıkıldı";
            }

            string building = "";
            int count = collapsed;
            var first = quake.Outcomes.Find(o => o.Collapsed);
            if (first != null) building = first.BuildingName;   // {count} her zaman yıkılan bina sayısı
            return template
                .Replace("{building}", building)
                .Replace("{count}", count.ToString())
                .Replace("{day}", quake.Day.ToString());
        }

        // ---------- sol kutu: toplam + sorumluluk ----------

        string BuildStory(QuakeResult quake)
        {
            if (quake == null) return "";
            var sb = new StringBuilder();
            sb.AppendLine("<b>GÜNÜN ÖZETİ</b>");

            if (quake.Outcomes.Count == 0)
            {
                // (A, Core) Son afetten beri onaylanan bina yoksa "incelenen yapılar" demeyelim
                sb.AppendLine("Şehirde sarsıntı hissedildi. Son depremden bu yana imzaladığın bir bina yoktu.");
            }
            else if (quake.CollapsedCount == 0)
            {
                sb.AppendLine("Şehirde sarsıntı hissedildi. İncelenen yapıların hepsi ayakta kaldı.");
            }
            else
            {
                sb.AppendLine($"Yıkılan bina: {quake.CollapsedCount}");
                sb.AppendLine($"Can kaybı: {quake.TotalDeaths}");
                sb.AppendLine($"Yaralı: {quake.TotalInjured}");
            }

            foreach (var o in quake.Outcomes)
            {
                if (!o.Collapsed || string.IsNullOrEmpty(o.ResponsibilityText)) continue;
                sb.AppendLine();
                sb.AppendLine($"<i>{o.BuildingName}:</i> {o.ResponsibilityText}");
            }

            if (quake.Outcomes.Exists(o => o.Collapsed && o.WasBribed))
            {
                sb.AppendLine();
                sb.AppendLine("Yıkılan yapının onayında usulsüzlük şüphesi var.");
            }
            if (quake.Jailed)
            {
                sb.AppendLine();
                sb.AppendLine("<b>Savcılık, imzanızın bulunduğu dosyalar hakkında soruşturma başlattı.</b>");
            }
            return sb.ToString();
        }

        // ---------- bina kutuları ----------

        string BuildBuilding(BuildingOutcome o)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<b>{o.BuildingName}</b>");
            if (!string.IsNullOrEmpty(o.Address)) sb.AppendLine(o.Address);

            if (!o.Collapsed)
            {
                // (A, Core) Karar: ayakta kalan binada da oyuncunun imzası/sorumluluğu yazılır
                sb.AppendLine(string.IsNullOrEmpty(o.ResponsibilityText) ? "Ayakta kaldı." : o.ResponsibilityText);
                return sb.ToString();
            }

            sb.AppendLine("<b>YIKILDI</b>");
            sb.AppendLine($"Can kaybı: {o.Deaths} · Yaralı: {o.Injured}");

            if (o.VictimNames != null && o.VictimNames.Count > 0)
            {
                int shown = Mathf.Min(maxVictimNames, o.VictimNames.Count);
                var names = string.Join(", ", o.VictimNames.GetRange(0, shown));
                if (o.VictimNames.Count > shown) names += $" ve {o.VictimNames.Count - shown} kişi daha";
                sb.AppendLine("Hayatını kaybedenler: " + names);
            }

            string reason = BuildReason(o);
            if (reason.Length > 0) sb.AppendLine(reason);
            return sb.ToString();
        }

        string SummarizeRest(List<BuildingOutcome> outcomes, int from)
        {
            int collapsed = 0, standing = 0;
            for (int i = from; i < outcomes.Count; i++)
            {
                if (outcomes[i].Collapsed) collapsed++; else standing++;
            }
            return $"Yıkılan: {collapsed} · Ayakta: {standing}";
        }

        // Yıkım nedeni: FailedParams'tan şablon cümle (elle yazılmaz)
        string BuildReason(BuildingOutcome o)
        {
            if (o.FailedParams == null || o.FailedParams.Count == 0) return "";
            var names = new List<string>();
            for (int i = 0; i < o.FailedParams.Count && i < 2; i++)
                names.Add(ParamName(o.FailedParams[i].Type));
            return "Neden: " + string.Join(" ve ", names) + " yetersizdi.";
        }

        string ParamName(ParamType type)
        {
            var rules = Context.Rules;
            if (rules != null)
            {
                foreach (var r in rules.Rules)
                    if (r.Param == type && !string.IsNullOrEmpty(r.DisplayName)) return r.DisplayName.ToLowerInvariant();
            }
            switch (type)
            {
                case ParamType.ConcreteStrength: return "beton dayanımı";
                case ParamType.RebarRatio: return "donatı oranı";
                case ParamType.SoilQuality: return "zemin etüdü";
                case ParamType.SeismicDesign: return "deprem yönetmeliği uyumu";
                case ParamType.FireSafety: return "yangın güvenliği";
                default: return type.ToString();
            }
        }
    }
}
