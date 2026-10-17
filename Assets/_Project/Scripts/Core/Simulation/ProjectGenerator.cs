using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Günün projelerini üretir. Aynı seed + gün → aynı projeler.
    /// Uygunsuz proje türleri (ProjectKind; DayConfig'in açtıklarından eşit ağırlıkla seçilir):
    ///  - Sloppy: rüşvetsiz, bir değeri gerekenin biraz altında (%85-95); belgeler tutarlı. Her gün gelebilir.
    ///  - Bribe: rüşvet teklif eder, değerleri belirgin yetersiz. (AllowBribes)
    ///  - Fake: rüşvetsiz, sahte belge: kötü değer yönetmeliği geçiyor gibi yazılır, belgelerde ipucu var. (AllowFakeDocuments)
    ///  - BribeFake: rüşvetli projelerin %25'i, sahte belge açıksa, aynı zamanda sahte belgelidir.
    /// Müteahhit seçimi türü ağırlıkla etkiler ama ele vermez (Ç3): dürüst usta da dikkatsiz olabilir, rüşvetçi de temiz proje getirebilir.
    /// </summary>
    public static class ProjectGenerator
    {
        const float CompliantMarginMin = 1.0f;
        const float CompliantMarginMax = 1.3f;
        const float SloppyRatioMin = 0.85f;
        const float SloppyRatioMax = 0.95f;
        const float BribeAlsoFakeChance = 0.25f;

        static readonly string[] BlockLetters = { "A", "B", "C", "D" };

        /// <param name="previousClientId">Dünün son müteahhidi; bugünün ilki o olmasın (null olabilir).</param>
        public static List<ProjectData> Generate(int day, DayConfigSO config, RuleSetSO rules, BalanceSO balance,
            NamePoolSO names, IReadOnlyList<ClientSO> clients, Random rng, ICollection<string> usedNames = null,
            string previousClientId = null)
        {
            int count = config.ProjectsPerDay;
            int badCount = (int)Math.Round(count * config.NonCompliantRatio);

            var compliance = new List<bool>();
            for (int i = 0; i < count; i++) compliance.Add(i >= badCount);
            rng.Shuffle(compliance);

            usedNames = usedNames ?? new HashSet<string>();
            var result = new List<ProjectData>();
            string previous = previousClientId;
            for (int i = 0; i < count; i++)
            {
                var p = CreateProject(day, i, compliance[i] ? ProjectKind.Compliant : PickBadKind(config, rng),
                    config, rules, balance, names, clients, rng, usedNames, previous);
                previous = p.ClientId;
                result.Add(p);
            }
            return result;
        }

        static ProjectKind PickBadKind(DayConfigSO config, Random rng)
        {
            var kinds = new List<ProjectKind> { ProjectKind.Sloppy };
            if (config.AllowBribes) kinds.Add(ProjectKind.Bribe);
            if (config.AllowFakeDocuments) kinds.Add(ProjectKind.Fake);
            var kind = kinds[rng.Next(kinds.Count)];
            if (kind == ProjectKind.Bribe && config.AllowFakeDocuments && rng.Chance(BribeAlsoFakeChance))
                kind = ProjectKind.BribeFake;
            return kind;
        }

        static ProjectData CreateProject(int day, int index, ProjectKind kind, DayConfigSO config, RuleSetSO rules,
            BalanceSO balance, NamePoolSO names, IReadOnlyList<ClientSO> clients, Random rng, ICollection<string> usedNames,
            string previousClientId)
        {
            var type = config.AllowFactories && rng.Chance(0.3f) ? BuildingType.Factory : BuildingType.Residential;
            bool compliant = kind == ProjectKind.Compliant;
            bool bribe = kind == ProjectKind.Bribe || kind == ProjectKind.BribeFake;
            bool fake = kind == ProjectKind.Fake || kind == ProjectKind.BribeFake;

            // Kat sayısı el kitabındaki sütunu belirler (1-3 / 4-6 / 7+); üç grup da gelsin diye 2-9 kat.
            int floors = type == BuildingType.Factory ? rng.RangeInclusive(1, 3) : rng.RangeInclusive(2, 9);

            var p = new ProjectData
            {
                Id = $"D{day:00}-P{index + 1}",
                BuildingType = type,
                Kind = kind,
                Name = MakeBuildingName(type, names, rng, usedNames),
                Address = MakeAddress(names, rng),
                Floors = floors,
                FileNo = $"2026-{rng.RangeInclusive(100, 999):0000}",
                PeoplePerFloor = rng.RangeInclusive(8, 24),
                Fee = rng.RangeInclusive(balance.ApprovalFeeRange.x, balance.ApprovalFeeRange.y)
            };

            // Parametreler: bu kat grubuna uyan kuralların hepsi (el kitabı v2: 5 parametre). Sıra = RuleSet'teki sıra
            // = El Kitabı sekme sırası (beton, demir, kolon, temel, zemin); form, lab raporu ve El Kitabı satırları aynı sırada.
            var selected = rules.RulesFor(type, floors);
            selected.Sort((a, b) => FirstIndexOf(rules, a.Param).CompareTo(FirstIndexOf(rules, b.Param)));

            var failing = new HashSet<int>();
            if (!compliant && selected.Count > 0)
            {
                int failCount = Math.Min(selected.Count, kind == ProjectKind.Sloppy ? 1 : rng.RangeInclusive(1, 2));
                while (failing.Count < failCount) failing.Add(rng.Next(selected.Count));
            }

            for (int i = 0; i < selected.Count; i++)
            {
                var r = selected[i];
                bool fails = failing.Contains(i);
                var entry = r.IsLabeled
                    ? LabeledValues(r, fails, kind, fake, rng)
                    : NumericValues(r, fails, kind, fake, rng);
                p.Params.Add(entry);
            }

            // Müşteri
            var client = PickClient(clients, kind, config.AllowThugs, rng, previousClientId);
            if (client != null) p.ClientId = client.Id;

            if (bribe)
            {
                p.OffersBribe = true;
                var range = client != null && client.OffersBribes ? client.BribeRange : new UnityEngine.Vector2Int(500, 2000);
                p.BribeAmount = Math.Max(50, RoundTo(rng.RangeInclusive(range.x, range.y), 50));
            }

            CreateDocuments(p, day, fake, names, rng);
            return p;
        }

        /// <summary>
        /// Müteahhit, türü ağırlıkla etkiler ama ele vermez (Ç3):
        ///  - Uygun:        %80 dürüst usta, %20 dürüst olmayan (zarf vermeden temiz proje getirir)
        ///  - Dikkatsiz:    %50 dürüst usta (dikkatsizlik), %50 dürüst olmayan
        ///  - Rüşvetli:     rüşvet veren müteahhit (OffersBribes); kabadayı sadece AllowThugs günlerinde
        ///  - Sahte belge:  %70 rüşvet vermeyen dürüst olmayan (Selim), %30 başka bir dürüst olmayan
        /// Dürüst usta asla rüşvet teklif etmez ve sahte belge getirmez. Uygun müteahhit yoksa null.
        /// (EEE, 17 Ekim) Aynı müteahhit arka arkaya gelmez: bir önceki (previousClientId) havuzdan çıkarılır; havuz boşalırsa
        /// bu türe uyan diğer havuza geçilir (ör. Selim az önce geldiyse sahte belgeyi başka bir dürüst olmayan getirir).
        /// Türe uyan başka kimse yoksa tekrar etmesine izin verilir.
        /// </summary>
        static ClientSO PickClient(IReadOnlyList<ClientSO> clients, ProjectKind kind, bool allowThugs, Random rng,
            string previousClientId = null)
        {
            if (clients == null || clients.Count == 0) return null;
            var honest = new List<ClientSO>();
            var dishonest = new List<ClientSO>();      // dürüst değil, kabadayı değil
            var bribers = new List<ClientSO>();        // rüşvet verir (kabadayı günlerinde kabadayı dahil)
            var forgers = new List<ClientSO>();        // dürüst değil, rüşvet vermez
            foreach (var c in clients)
            {
                if (c == null || (c.IsThug && !allowThugs)) continue;
                if (c.IsHonest) { honest.Add(c); continue; }
                if (c.OffersBribes) bribers.Add(c);
                if (!c.IsThug) dishonest.Add(c);
                if (!c.OffersBribes) forgers.Add(c);
            }

            bool bribeKind = kind == ProjectKind.Bribe || kind == ProjectKind.BribeFake;
            List<ClientSO> pool;
            switch (kind)
            {
                case ProjectKind.Compliant: pool = rng.Chance(0.8f) ? honest : dishonest; break;
                case ProjectKind.Sloppy: pool = rng.Chance(0.5f) ? honest : dishonest; break;
                case ProjectKind.Bribe:
                case ProjectKind.BribeFake: pool = bribers; break;
                default: pool = rng.Chance(0.7f) ? forgers : dishonest; break;
            }
            if (pool.Count == 0) pool = bribeKind ? bribers : kind == ProjectKind.Compliant ? honest : dishonest;

            if (!string.IsNullOrEmpty(previousClientId))
            {
                var fresh = pool.FindAll(c => c.Id != previousClientId);
                if (fresh.Count == 0)
                {
                    // Bu türe uyan diğer havuzlar (sözleşme korunur: dürüst usta rüşvet/sahte almaz, rüşvet sadece rüşvetçiye)
                    var alternatives = bribeKind ? bribers
                        : kind == ProjectKind.Compliant || kind == ProjectKind.Sloppy ? Merge(honest, dishonest)
                        : dishonest;
                    fresh = alternatives.FindAll(c => c.Id != previousClientId);
                }
                if (fresh.Count > 0) pool = fresh;
            }
            return pool.Count > 0 ? rng.Pick(pool) : null;
        }

        static List<ClientSO> Merge(List<ClientSO> a, List<ClientSO> b)
        {
            var all = new List<ClientSO>(a);
            all.AddRange(b);
            return all;
        }

        /// <summary>Ç5: bir oyunda aynı bina adı iki kez gelmez; ad kalıpları çeşitlenir.</summary>
        static string MakeBuildingName(BuildingType type, NamePoolSO names, Random rng, ICollection<string> used)
        {
            for (int attempt = 0; attempt < 40; attempt++)
            {
                string baseName = rng.Pick(names != null ? names.BuildingNames : null, "Yeni");
                string name;
                if (type == BuildingType.Factory) name = $"{baseName} Fabrikası";
                else
                {
                    switch (rng.Next(5))
                    {
                        case 0: name = $"{baseName} Sitesi {BlockLetters[rng.Next(BlockLetters.Length)]} Blok"; break;
                        case 1: name = $"{baseName} Konutları"; break;
                        case 2: name = $"{baseName} Evleri"; break;
                        default: name = $"{baseName} Apartmanı"; break;
                    }
                }
                if (!used.Contains(name)) { used.Add(name); return name; }
            }
            // Havuz tükendiyse numaralandır
            string fallback = $"{rng.Pick(names != null ? names.BuildingNames : null, "Yeni")} Apartmanı {used.Count + 1}";
            used.Add(fallback);
            return fallback;
        }

        static void CreateDocuments(ProjectData p, int day, bool fake, NamePoolSO names, Random rng)
        {
            string parcel = $"{rng.RangeInclusive(100, 999)}/{rng.RangeInclusive(1, 40)}";
            string date = $"{rng.RangeInclusive(1, 28):00}.{rng.RangeInclusive(1, 9):00}.2026";
            string engineer = rng.Pick(names != null ? names.EngineerNames : null, "Müh. A. Yılmaz");
            string labName = rng.Pick(names != null ? names.LabNames : null, "Merkez Yapı Lab.");

            // StampId = kaşenin üstünde okunan metin (Desk doğrudan gösterir). İçinde gerçek/sahte bilgisi OLMAZ.
            var application = new DocumentData
            {
                Kind = DocumentKind.Application,
                Title = "Yapı Denetim Formu",
                ParcelNo = parcel,
                Date = date,
                StampId = MunicipalityStamp,
                SignatureName = engineer
            };
            var lab = new DocumentData
            {
                Kind = DocumentKind.LabReport,
                Title = "Laboratuvar Raporu",
                ParcelNo = parcel,
                Date = date,
                LabName = labName,
                StampId = LabStamp(labName),
                StampLabName = labName,
                SignatureName = engineer
            };

            if (fake)
            {
                // 1-2 bedava ipucu: lab raporu başvuruyla ya da kendi başlığıyla tutarsız
                var clueTypes = new List<ClueType>((ClueType[])Enum.GetValues(typeof(ClueType)));
                rng.Shuffle(clueTypes);
                int clueCount = rng.RangeInclusive(1, 2);
                for (int i = 0; i < clueCount; i++) ApplyClue(lab, clueTypes[i], names, rng);
            }

            p.Documents.Add(application);
            p.Documents.Add(lab);
        }

        const string MunicipalityStamp = "BELEDİYE · İMAR MÜDÜRLÜĞÜ";

        /// <summary>Sahte kaşelerde kullanılan, akredite listede olmayan laboratuvarlar (kurgusal).</summary>
        static readonly string[] UnaccreditedLabs = { "Kale Test Lab.", "Güneş Deney Lab.", "Yıldız Malzeme Lab." };

        static string LabStamp(string labName) => $"{UpperTr(labName)} · AKREDİTE";

        static void ApplyClue(DocumentData doc, ClueType clue, NamePoolSO names, Random rng)
        {
            switch (clue)
            {
                case ClueType.ParcelMismatch:
                {
                    // Gerçek parselle asla aynı olmasın (inceleme modu "uyuşmazlık" diyebilmeli)
                    string original = doc.ParcelNo, other;
                    do other = $"{rng.RangeInclusive(100, 999)}/{rng.RangeInclusive(1, 40)}"; while (other == original);
                    doc.ParcelNo = other;
                    break;
                }
                case ClueType.DateMismatch:
                    doc.Date = $"{rng.RangeInclusive(1, 28):00}.{rng.RangeInclusive(10, 12):00}.2026";
                    break;
                case ClueType.UnknownLab:
                    // Başlık bilinmeyen lab; kaşe hâlâ eski lab adını taşır → başlık/kaşe uyuşmaz
                    doc.LabName = "Hızlı Sonuç Lab. Ltd.";
                    break;
                case ClueType.WrongStamp:
                {
                    // Kaşe akredite OLMAYAN başka bir laboratuvarın → başlıkla uyuşmaz ve inceleme modunda
                    // "kaşe ↔ akredite listesi" uyuşmazlık verir (Ahmet kararı, 17 Ekim akşam).
                    var accredited = names != null ? names.LabNames : null;
                    var pool = new List<string>();
                    foreach (var n in UnaccreditedLabs)
                        if (n != doc.LabName && (accredited == null || !accredited.Contains(n))) pool.Add(n);
                    string stampLab = rng.Pick(pool, "Kale Test Lab.");
                    doc.StampId = LabStamp(stampLab);
                    doc.StampLabName = stampLab;
                    break;
                }
                case ClueType.WrongSignature:
                    // Lab raporunu başvurudakinden farklı bir mühendis imzalamış
                    doc.SignatureName = PickOther(names != null ? names.EngineerNames : null, doc.SignatureName, "Müh. K. Durmaz", rng);
                    break;
            }
            doc.Clues.Add(clue);
        }

        static string PickOther(List<string> pool, string current, string fallback, Random rng)
        {
            var others = pool == null ? new List<string>() : pool.FindAll(s => s != current);
            return others.Count > 0 ? rng.Pick(others) : fallback;
        }

        /// <summary>Kültürden bağımsız Türkçe büyük harf (i → İ, ı → I).</summary>
        internal static string UpperTr(string s) => s.Replace('i', 'İ').Replace('ı', 'I').ToUpperInvariant();

        /// <summary>Görsel adres (sokak + kapı no). Mahalle kaldırıldı: oyun kuralını etkileyen bir bilgi taşımaz.</summary>
        static string MakeAddress(NamePoolSO names, Random rng)
        {
            string street = rng.Pick(names != null ? names.Streets : null, "Lale");
            return $"{street} Sokak No: {rng.RangeInclusive(1, 60)}";
        }

        // ---------- Değer üretimi ----------

        /// <summary>Sayısal parametre: uygun %100-130, dikkatsiz %85-95, rüşvetli/sahte kuralın FailRatioRange'i.</summary>
        static ParamEntry NumericValues(ParamRule r, bool fails, ProjectKind kind, bool fake, Random rng)
        {
            float step = StepFor(r.Required);
            float actual;
            if (!fails) actual = r.Required * rng.Range(CompliantMarginMin, CompliantMarginMax);
            else if (kind == ProjectKind.Sloppy) actual = r.Required * rng.Range(SloppyRatioMin, SloppyRatioMax);
            else actual = r.Required * rng.Range(r.FailRatioRange.x, r.FailRatioRange.y);
            actual = Snap(actual, step);
            // Yuvarlama uygun/uygunsuz ayrımını bozmasın
            if (fails) actual = Math.Min(actual, Snap(r.Required - step, step));
            else actual = Math.Max(actual, r.Required);

            // Sahte belge: kötü değer yönetmeliği geçiyor gibi yazılır
            float reported = fake && fails ? Math.Max(r.Required, Snap(r.Required * rng.Range(1.02f, 1.15f), step)) : actual;
            return new ParamEntry { Type = r.Param, Required = r.Required, Reported = reported, Actual = actual };
        }

        /// <summary>Harfli parametre (beton sınıfı, zemin sınıfı): değerler etiket sıra numarası; yetersizlik 1-2 kademe.</summary>
        static ParamEntry LabeledValues(ParamRule r, bool fails, ProjectKind kind, bool fake, Random rng)
        {
            int max = r.Labels.Count - 1;
            int req = Math.Max(0, Math.Min(max, (int)Math.Round(r.Required)));
            int actual;
            if (!fails) actual = Math.Min(max, req + rng.RangeInclusive(0, 1));
            else actual = Math.Max(0, req - (kind == ProjectKind.Sloppy ? 1 : rng.RangeInclusive(1, 2)));
            if (fails && actual >= req) actual = Math.Max(0, req - 1);
            int reported = fake && fails ? Math.Min(max, req + rng.RangeInclusive(0, 1)) : actual;
            return new ParamEntry { Type = r.Param, Required = req, Reported = reported, Actual = actual, Labeled = true };
        }

        /// <summary>Okunur yuvarlama adımı: 100 ve üstü → 10, 10-100 → 1, altı → 0,1.</summary>
        static float StepFor(float required) => required >= 100f ? 10f : required >= 10f ? 1f : 0.1f;

        static float Snap(float v, float step) => (float)Math.Round(Math.Round(v / step) * step, 2);

        static float Round1(float v) => (float)Math.Round(v, 1);

        static int FirstIndexOf(RuleSetSO rules, ParamType param)
        {
            for (int i = 0; i < rules.Rules.Count; i++)
                if (rules.Rules[i].Param == param) return i;
            return int.MaxValue;
        }
        static int RoundTo(int v, int step) => (v / step) * step;
    }
}
