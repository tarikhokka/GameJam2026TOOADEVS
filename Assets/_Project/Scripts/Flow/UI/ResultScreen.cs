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
        // (EEE, 18 Ekim) Gazete masaya atılır: ahşap masa, gölge, hafif eğik kağıt; siyah-beyaz eski gazete teması: siyah manşet bandı, SON DAKİKA
        // şeridi, gün mührü, bina kutularında YIKILDI / AYAKTA kaşeleri. Açılış hareketi + kağıt ve kaşe sesi.
        [SerializeField] Sprite table;          // newspaper_table
        [SerializeField] AudioClip paperClip;   // sfx_paper
        [SerializeField] AudioClip stampClip;   // sfx_stamp

        // Eski gazete: sadece mürekkep siyahı ve kağıt kremi (renkli vurgu yok).
        const string InkHex = "#141210";
        static readonly Color NewsInk = new Color(0.08f, 0.07f, 0.06f);
        static readonly Color NewsCream = new Color(0.95f, 0.91f, 0.82f);

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

        TextMeshProUGUI _headline, _story, _dayText;
        readonly List<TextMeshProUGUI> _buildingTexts = new List<TextMeshProUGUI>();
        readonly List<(RectTransform rt, Image img, TextMeshProUGUI text, Image tint)> _badges =
            new List<(RectTransform, Image, TextMeshProUGUI, Image)>();
        Button _continue;
        RectTransform _root, _paper, _ribbon, _dayStamp, _headlineRt;
        CanvasGroup _continueGroup;
        AudioSource _audio;
        Coroutine _intro;

        static readonly Vector2 PaperSize = new Vector2(1180f, 885f); // 1200x900 oranında, masa kenarlardan görünsün
        const float PaperTilt = -1.5f;

        protected override void Build()
        {
            var root = _root = UiKit.Stretch(transform, "Root");
            UiKit.Image(root, "Table", table, table != null ? Color.white : UiKit.Dark, Vector2.zero, Vector2.one);

            // Kağıdın gölgesi ve kendisi: ortada sabit boyut, hafif eğik
            var holder = UiKit.Rect(root, "PaperHolder", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            holder.sizeDelta = PaperSize;
            holder.anchoredPosition = new Vector2(0f, 22f);
            _paper = holder;
            var shadow = UiKit.Image(holder, "Shadow", null, new Color(0f, 0f, 0f, 0.5f), Vector2.zero, Vector2.one);
            shadow.rectTransform.offsetMin = new Vector2(16f, -22f);
            shadow.rectTransform.offsetMax = new Vector2(16f, -22f);
            var paper = UiKit.Stretch(holder, "Paper");
            var bg = paper.gameObject.AddComponent<Image>();
            bg.sprite = background;
            bg.color = background != null ? Color.white : UiKit.Paper;
            bg.raycastTarget = false;

            // Manşet: siyah bant + krem Paytone
            var band = UiKit.Image(paper, "HeadlineBand", null, NewsInk, BoxMin(HeadlineBox, 2f), BoxMax(HeadlineBox, 2f));
            _headlineRt = band.rectTransform;
            _headline = UiKit.Text(band.transform, "Headline", "", 66, UiKit.Paper, TextAlignmentOptions.Center,
                new Vector2(0.02f, 0.06f), new Vector2(0.98f, 0.94f), true, 28);
            UiKit.StyleInk(_headline, titleFont); // krem, düz (altın geçiş yok)
            _headline.color = NewsCream;
            _headline.textWrappingMode = TextWrappingModes.Normal;

            _story = BoxText(paper, "Story", StoryBox, 28, TextAlignmentOptions.TopLeft, 14);
            UiKit.StyleInk(_story, bodyFont);
            for (int i = 0; i < BuildingBoxes.Length; i++)
            {
                var tint = UiKit.Image(paper, "Tint" + i, null, Color.clear, BoxMin(BuildingBoxes[i], 2f), BoxMax(BuildingBoxes[i], 2f));
                var t = BoxText(paper, "Building" + i, BuildingBoxes[i], 26, TextAlignmentOptions.TopLeft, 11);
                UiKit.StyleInk(t, bodyFont);
                t.rectTransform.offsetMax = new Vector2(-70f, 0f); // sağ üstteki kaşeye yer
                _buildingTexts.Add(t);
                _badges.Add(Badge(paper, i, tint));
            }

            // SON DAKİKA şeridi (sol üst köşe, eğik) ve gün mührü (başlığın sağı)
            var ribbon = UiKit.Image(paper, "Ribbon", null, NewsInk, new Vector2(0f, 1f), new Vector2(0f, 1f));
            _ribbon = ribbon.rectTransform;
            _ribbon.sizeDelta = new Vector2(250f, 58f);
            _ribbon.anchoredPosition = new Vector2(80f, -28f); // başlığın Ş'sini örtmeden köşede
            _ribbon.localRotation = Quaternion.Euler(0f, 0f, 14f);
            ribbon.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(4f, -5f);
            var rt = UiKit.Text(_ribbon, "Text", "SON DAKİKA!", 34, NewsCream, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            UiKit.StyleInk(rt, titleFont);
            rt.color = NewsCream;

            var stamp = UiKit.Image(paper, "DayStamp", null, NewsInk, new Vector2(1f, 1f), new Vector2(1f, 1f));
            _dayStamp = stamp.rectTransform;
            _dayStamp.sizeDelta = new Vector2(124f, 124f);
            _dayStamp.anchoredPosition = new Vector2(-104f, -108f);
            _dayStamp.localRotation = Quaternion.Euler(0f, 0f, -10f);
            stamp.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(3f, -4f);
            _dayText = UiKit.Text(_dayStamp, "Text", "", 30, UiKit.Paper, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            UiKit.StyleInk(_dayText, titleFont);
            _dayText.color = UiKit.Paper;
            _dayText.lineSpacing = -18f;

            holder.localRotation = Quaternion.Euler(0f, 0f, PaperTilt);

            _continue = UiKit.Button(root, "Continue", "Hesaplaşmaya geç  →", Finish,
                new Vector2(0.74f, 0.02f), new Vector2(0.98f, 0.09f), 30f);
            _continueGroup = _continue.gameObject.AddComponent<CanvasGroup>();

            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
        }

        (RectTransform, Image, TextMeshProUGUI, Image) Badge(Transform paper, int i, Image tint)
        {
            var box = BuildingBoxes[i];
            var img = UiKit.Image(paper, "Badge" + i, null, NewsInk, Vector2.zero, Vector2.zero);
            var r = img.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2((box[2] - 18f) / W, 1f - (box[1] + 18f) / H);
            r.pivot = new Vector2(1f, 1f);
            r.sizeDelta = new Vector2(150f, 44f);
            r.localRotation = Quaternion.Euler(0f, 0f, 8f);
            var t = UiKit.Text(r, "Text", "", 26, UiKit.Paper, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            UiKit.StyleInk(t, titleFont);
            t.color = UiKit.Paper;
            t.characterSpacing = 4f;
            return (r, img, t, tint);
        }

        static Vector2 BoxMin(float[] box, float inset) => new Vector2((box[0] + inset) / W, 1f - (box[3] - inset) / H);
        static Vector2 BoxMax(float[] box, float inset) => new Vector2((box[2] - inset) / W, 1f - (box[1] + inset) / H);

        static TextMeshProUGUI BoxText(Transform parent, string name, float[] box, float size,
            TextAlignmentOptions align, float minSize)
        {
            return UiKit.Text(parent, name, "", size, UiKit.Ink, align, BoxMin(box, Pad), BoxMax(box, Pad), true, minSize);
        }

        protected override void Refresh()
        {
            var quake = Context.LastQuake;
            _headline.text = PickHeadline(quake);
            _story.text = BuildStory(quake);
            _dayText.text = quake != null ? $"{quake.Day}.\n<size=70%>GÜN</size>" : "";

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
                var badge = _badges[i];
                bool overflowSlot = outcomes.Count > slots && i == slots - 1;
                if (overflowSlot)
                {
                    t.text = $"<b>Ve {outcomes.Count - (slots - 1)} bina daha</b>\n" + SummarizeRest(outcomes, slots - 1);
                    SetBadge(badge, null);
                }
                else if (i < outcomes.Count)
                {
                    t.text = BuildBuilding(outcomes[i]);
                    SetBadge(badge, outcomes[i]);
                }
                else
                {
                    t.text = "";
                    SetBadge(badge, null);
                }
            }

            if (_intro != null) StopCoroutine(_intro);
            _intro = StartCoroutine(Intro(quake != null && quake.CollapsedCount > 0));
        }

        static void SetBadge((RectTransform rt, Image img, TextMeshProUGUI text, Image tint) b, BuildingOutcome o)
        {
            b.rt.gameObject.SetActive(o != null);
            if (o == null) { b.tint.color = Color.clear; return; }
            b.img.color = o.Collapsed ? NewsInk : NewsCream;                 // YIKILDI: siyah kaşe; AYAKTA: krem kaşe, siyah çerçeve
            b.text.color = o.Collapsed ? NewsCream : NewsInk;
            var frame = b.img.GetComponent<Outline>();
            if (frame == null) { frame = b.img.gameObject.AddComponent<Outline>(); frame.effectColor = NewsInk; frame.effectDistance = new Vector2(3f, -3f); }
            frame.enabled = !o.Collapsed;
            b.text.text = o.Collapsed ? "YIKILDI" : "AYAKTA";
            b.tint.color = o.Collapsed ? new Color(NewsInk.r, NewsInk.g, NewsInk.b, 0.08f) : Color.clear;
        }

        // ---------- açılış hareketi ----------

        // Gazete aşağıdan dönerek masaya atılır (kağıt sesi); yıkım varsa ekran sarsılır; manşet vurgulanır;
        // SON DAKİKA ve gün mührü belirir; kaşeler sırayla basılır (kaşe sesi); en son devam düğmesi gelir.
        System.Collections.IEnumerator Intro(bool collapsed)
        {
            var activeBadges = _badges.FindAll(b => b.rt.gameObject.activeSelf);
            foreach (var b in activeBadges) b.rt.localScale = Vector3.zero;
            _ribbon.localScale = Vector3.zero;
            _dayStamp.localScale = Vector3.zero;
            _continueGroup.alpha = 0f;
            _continueGroup.interactable = false;
            Play(paperClip, 0.9f);

            var rest = new Vector2(0f, 22f);
            var start = new Vector2(-260f, -1250f);
            for (float t = 0f; t < 0.6f; t += Dt())
            {
                float k = EaseOutBack(t / 0.6f, 1.2f);
                _paper.anchoredPosition = Vector2.LerpUnclamped(start, rest, k);
                _paper.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(28f, PaperTilt, k));
                float s = Mathf.LerpUnclamped(1.12f, 1f, k);
                _paper.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            _paper.anchoredPosition = rest;
            _paper.localRotation = Quaternion.Euler(0f, 0f, PaperTilt);
            _paper.localScale = Vector3.one;

            if (collapsed)
                for (float t = 0f; t < 0.35f; t += Dt())
                {
                    float amp = 14f * (1f - t / 0.35f);
                    _root.anchoredPosition = new Vector2(Random.Range(-amp, amp), Random.Range(-amp, amp));
                    yield return null;
                }
            _root.anchoredPosition = Vector2.zero;

            yield return Pop(_headlineRt, 1.12f, 0.28f);
            Play(stampClip, 0.7f);
            yield return Pop(_dayStamp, 0f, 0.22f);
            yield return Pop(_ribbon, 0f, 0.22f);
            foreach (var b in activeBadges)
            {
                Play(stampClip, 0.8f);
                yield return Pop(b.rt, 0f, 0.18f, 1.8f);
            }

            for (float t = 0f; t < 0.3f; t += Dt()) { _continueGroup.alpha = t / 0.3f; yield return null; }
            _continueGroup.alpha = 1f;
            _continueGroup.interactable = true;

            // SON DAKİKA şeridi hafifçe nabız atar
            for (float t = 0f; ; t += Dt())
            {
                float s = 1f + 0.04f * Mathf.Sin(t * 5f);
                _ribbon.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
        }

        // from = 0: yoktan büyüyerek gelir; from > 1: büyükten yerine oturur (kaşe gibi: overshoot = başlangıç ölçeği)
        System.Collections.IEnumerator Pop(RectTransform rt, float from, float duration, float stampFrom = 0f)
        {
            float s0 = stampFrom > 0f ? stampFrom : from;
            for (float t = 0f; t < duration; t += Dt())
            {
                float k = t / duration;
                float s = s0 > 1f ? Mathf.Lerp(s0, 1f, 1f - (1f - k) * (1f - k)) : Mathf.LerpUnclamped(s0, 1f, EaseOutBack(k, 2f));
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        static float EaseOutBack(float k, float c)
        {
            k = Mathf.Clamp01(k) - 1f;
            return 1f + (c + 1f) * k * k * k + c * k * k;
        }

        static float Dt() => Mathf.Min(PauseMenu.AnimationDeltaTime, 0.05f);

        void Play(AudioClip clip, float volume)
        {
            if (clip != null && _audio != null) _audio.PlayOneShot(clip, volume);
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
            sb.AppendLine($"<color={InkHex}><b>GÜNÜN ÖZETİ</b></color>");

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
                sb.AppendLine($"Yıkılan bina: <color={InkHex}><b>{quake.CollapsedCount}</b></color>");
                sb.AppendLine($"Can kaybı: <color={InkHex}><b>{quake.TotalDeaths}</b></color>");
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

            sb.AppendLine($"<color={InkHex}><b>YIKILDI</b></color>");
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
