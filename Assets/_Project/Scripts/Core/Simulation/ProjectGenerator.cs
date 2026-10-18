using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Günün projelerini üretir. Aynı seed + gün → aynı projeler.
    /// Uygunsuz proje türleri (ProjectKind; DayConfig'in açtıklarından eşit ağırlıkla seçilir):
    ///  - Sloppy: rüşvetsiz, bir değeri gerekenin biraz altında (%85-95); belgeler tutarlı. Her gün gelebilir.
    ///  - Bribe: rüşvet teklif eder, değerleri belirgin yetersiz. (AllowBribes)
    ///  - Fake: rüşvetsiz, sahte belge: kötü değer yönetmeliği geçiyor gibi yazılır; belgelerde künye ipucu ve/veya form ↔ lab
    ///    değer farkı var (değer farkı olanların yarısında künye ipucu yok: sadece ek test kanıtlar). (AllowFakeDocuments)
    ///  - Uygun projelerin %15'inde masum değer farkı (yazım hatası; gerçek değer iyi). Oyuncu farkı ek testle çözer.
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
        // Değer farkı (EEE önerisi, Ahmet onayı, 17 Ekim gece): lab raporunun beyan sütununda bir değer formdan farklı yazar,
        // ikisi de yönetmeliği geçiyor görünür. Sahtede gerçek değer kötü, dürüst projede masum yazım hatası (gerçek iyi).
        // Oyuncu İNCELE ile farkı bulur ama hangisi olduğunu ancak ek testle bilir.
        const float TypoChance = 0.15f;              // uygun projede masum yazım hatası
        const float FakeValueDiffChance = 0.65f;     // sahte belgede değer farkı
        const float FakeOnlyValueDiffChance = 0.5f;  // değer farkı olan sahtede künye ipucu hiç olmasın (sadece ek test kanıtlar)

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
            // Türler önce belirlenir; müteahhitler günün tamamı için birlikte yerleştirilir (AssignClients).
            var kinds = new List<ProjectKind>();
            for (int i = 0; i < count; i++) kinds.Add(compliance[i] ? ProjectKind.Compliant : PickBadKind(config, rng));
            var assigned = AssignClients(kinds, clients, config.AllowThugs, rng, previousClientId);

            var result = new List<ProjectData>();
            for (int i = 0; i < count; i++)
                result.Add(CreateProject(day, i, kinds[i], config, rules, balance, names, assigned[i], rng, usedNames));
            return result;
        }

        /// <summary>Ziyaret programından (VisitPlanner) günün projeleri: kim ve hangi tür programda, gerisi aynı üretim.</summary>
        public static List<ProjectData> GenerateFromSchedule(int day, ScheduledDay plan, DayConfigSO config, RuleSetSO rules,
            BalanceSO balance, NamePoolSO names, IReadOnlyList<ClientSO> clients, Random rng, ICollection<string> usedNames = null)
        {
            usedNames = usedNames ?? new HashSet<string>();
            var result = new List<ProjectData>();
            for (int i = 0; i < plan.Slots.Count; i++)
            {
                var slot = plan.Slots[i];
                ClientSO client = null;
                if (clients != null) foreach (var c in clients) if (c != null && c.Id == slot.ClientId) { client = c; break; }
                var project = CreateProject(day, i, slot.Kind, config, rules, balance, names, client, rng, usedNames);
                project.GiftGuaranteed = slot.Gift;
                result.Add(project);
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
            BalanceSO balance, NamePoolSO names, ClientSO client, Random rng, ICollection<string> usedNames)
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
            if (client != null) p.ClientId = client.Id; // AssignClients seçti

            if (bribe)
            {
                p.OffersBribe = true;
                var range = client != null && client.OffersBribes ? client.BribeRange
                    : client != null && client.IsGeneric ? balance.GenericBribeRange : new UnityEngine.Vector2Int(500, 2000);
                p.BribeAmount = Math.Max(50, RoundTo(rng.RangeInclusive(range.x, range.y), 50));
            }

            // Değer farkı: sahtede kötü (yalan yazılmış) parametrelerden birinde, uygun projede rastgele bir parametrede.
            bool kunyeClues = fake;
            if (fake && rng.Chance(FakeValueDiffChance) && AddLabDifference(p, selected, failing, rng))
                kunyeClues = !rng.Chance(FakeOnlyValueDiffChance);
            else if (compliant && rng.Chance(TypoChance))
                AddLabDifference(p, selected, null, rng);

            CreateDocuments(p, day, fake, kunyeClues, names, rng);
            return p;
        }

        /// <summary>
        /// Adaylardan (null: hepsi) birinde lab raporunun beyanını formdan farklı ama yönetmeliği geçen bir değere çevirir.
        /// Uygun değer bulunamazsa (ör. seçenekli parametre zaten en üst kademede) false.
        /// </summary>
        static bool AddLabDifference(ProjectData p, List<ParamRule> rules, HashSet<int> candidates, Random rng)
        {
            var order = new List<int>();
            for (int i = 0; i < p.Params.Count; i++) if (candidates == null || candidates.Contains(i)) order.Add(i);
            rng.Shuffle(order);
            foreach (int i in order)
            {
                var e = p.Params[i];
                var options = new List<float>();
                if (e.Labeled)
                {
                    int max = rules[i].Labels.Count - 1;
                    foreach (int d in new[] { -1, 1 })
                    {
                        int v = (int)Math.Round(e.Reported) + d;
                        if (v >= e.Required && v <= max) options.Add(v);
                    }
                }
                else
                {
                    float step = StepFor(e.Required);
                    foreach (int k in new[] { -2, -1, 1, 2 })
                    {
                        float v = Snap(e.Reported + k * step, step);
                        if (v >= e.Required && Math.Abs(v - e.Reported) > 1e-4f) options.Add(v);
                    }
                }
                if (options.Count == 0) continue;
                e.LabDiffers = true;
                e.LabReported = options[rng.Next(options.Count)];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Müteahhit, türü ağırlıkla etkiler ama ele vermez (Ç3):
        ///  - Uygun:        %80 dürüst usta, %20 dürüst olmayan (zarf vermeden temiz proje getirir)
        ///  - Dikkatsiz:    %50 dürüst usta (dikkatsizlik), %50 dürüst olmayan
        ///  - Rüşvetli:     rüşvet veren müteahhit (OffersBribes); kabadayı sadece AllowThugs günlerinde
        ///  - Sahte belge:  %70 rüşvet vermeyen dürüst olmayan (Selim), %30 başka bir dürüst olmayan
        /// Dürüst usta asla rüşvet teklif etmez ve sahte belge getirmez.
        /// Bir projenin aday sırası: olasılıkla seçilen havuz (karışık) → türe uyan diğerleri (karışık) → son çare olarak
        /// temiz / dikkatsiz projede kabadayı (rüşvetçi bazen temiz proje getirir).
        /// </summary>
        static List<ClientSO> Candidates(IReadOnlyList<ClientSO> clients, ProjectKind kind, bool allowThugs, Random rng)
        {
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
            bool cleanKind = kind == ProjectKind.Compliant || kind == ProjectKind.Sloppy;
            List<ClientSO> pool;
            switch (kind)
            {
                case ProjectKind.Compliant: pool = rng.Chance(0.8f) ? honest : dishonest; break;
                case ProjectKind.Sloppy: pool = rng.Chance(0.5f) ? honest : dishonest; break;
                case ProjectKind.Bribe:
                case ProjectKind.BribeFake: pool = bribers; break;
                default: pool = rng.Chance(0.7f) ? forgers : dishonest; break;
            }
            // Türe uyan herkes (sözleşme: dürüst usta rüşvet / sahte almaz, rüşvet sadece rüşvetçiye)
            var legal = bribeKind ? bribers : cleanKind ? Merge(honest, dishonest) : dishonest;
            var thugs = bribers.FindAll(c => c.IsThug);

            var order = new List<ClientSO>(pool);
            rng.Shuffle(order);
            foreach (var group in new[] { legal, cleanKind ? thugs : new List<ClientSO>() })
            {
                var rest = group.FindAll(c => !order.Contains(c));
                rng.Shuffle(rest);
                order.AddRange(rest);
            }
            return order;
        }

        /// <summary>
        /// (EEE, 17 Ekim gece) Günün müteahhitleri birlikte yerleştirilir: her projeye aday sırasıyla biri denenir; bir
        /// müteahhit günde bir kez gelir ve arka arkaya (dünün sonuncusu dahil) gelmez. Çıkmaza girilirse geri dönülüp başka
        /// aday denenir (en çok 6 proje × 6 müteahhit, anında biter). Hiç mümkün değilse (ör. 4 sahte belge, 3 sahteci) tekrara
        /// izin verilir ama yine arka arkaya değil.
        /// </summary>
        static ClientSO[] AssignClients(List<ProjectKind> kinds, IReadOnlyList<ClientSO> clients, bool allowThugs, Random rng,
            string previousClientId)
        {
            int n = kinds.Count;
            var result = new ClientSO[n];
            if (clients == null || clients.Count == 0) return result;

            var options = new List<List<ClientSO>>();
            foreach (var k in kinds) options.Add(Candidates(clients, k, allowThugs, rng));

            var used = new HashSet<string>();
            bool Place(int i, string prev)
            {
                if (i == n) return true;
                foreach (var c in options[i])
                {
                    if (c.Id == prev || used.Contains(c.Id)) continue;
                    used.Add(c.Id);
                    result[i] = c;
                    if (Place(i + 1, c.Id)) return true;
                    used.Remove(c.Id);
                }
                return false;
            }
            if (Place(0, previousClientId)) return result;

            // Herkesi bir kez getirmek mümkün değil: sırayla, arka arkaya olmadan, mümkünse bugün gelmemiş olandan
            used.Clear();
            string last = previousClientId;
            for (int i = 0; i < n; i++)
            {
                var opts = options[i];
                ClientSO pick = opts.Find(c => c.Id != last && !used.Contains(c.Id))
                                ?? opts.Find(c => c.Id != last)
                                ?? (opts.Count > 0 ? opts[0] : null);
                result[i] = pick;
                if (pick != null) { used.Add(pick.Id); last = pick.Id; }
            }
            return result;
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

        static void CreateDocuments(ProjectData p, int day, bool fake, bool kunyeClues, NamePoolSO names, Random rng)
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

            if (fake && kunyeClues)
            {
                // 1-2 bedava ipucu: lab raporu başvuruyla ya da kendi başlığıyla tutarsız (sadece değer farkı olan sahtede yok)
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
