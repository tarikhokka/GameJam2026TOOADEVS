using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Günün projelerini üretir (v0). Aynı seed + gün → aynı projeler.
    /// Uygunsuz proje türleri (DayConfig'e göre):
    ///  - Düz uygunsuz: belgede gerçek (kötü) değerler yazar, dikkatli okuyan yakalar.
    ///  - Rüşvetli: rüşvet teklif eder (her rüşvetçi uygunsuzdur).
    ///  - Sahte belgeli: rüşvetsiz, belge temiz görünür; bedava ipucu (tutarsızlık) + ek testle yakalanır.
    /// </summary>
    public static class ProjectGenerator
    {
        const float CompliantMarginMin = 1.0f;
        const float CompliantMarginMax = 1.3f;

        public static List<ProjectData> Generate(int day, DayConfigSO config, RuleSetSO rules, BalanceSO balance,
            NamePoolSO names, IReadOnlyList<ClientSO> clients, Random rng)
        {
            int count = config.ProjectsPerDay;
            int badCount = (int)Math.Round(count * config.NonCompliantRatio);

            var compliance = new List<bool>();
            for (int i = 0; i < count; i++) compliance.Add(i >= badCount);
            rng.Shuffle(compliance);

            var result = new List<ProjectData>();
            for (int i = 0; i < count; i++)
                result.Add(CreateProject(day, i, compliance[i], config, rules, balance, names, clients, rng));
            return result;
        }

        static ProjectData CreateProject(int day, int index, bool compliant, DayConfigSO config, RuleSetSO rules,
            BalanceSO balance, NamePoolSO names, IReadOnlyList<ClientSO> clients, Random rng)
        {
            var type = config.AllowFactories && rng.Chance(0.3f) ? BuildingType.Factory : BuildingType.Residential;
            string baseName = rng.Pick(names != null ? names.BuildingNames : null, "Yeni");

            var p = new ProjectData
            {
                Id = $"D{day:00}-P{index + 1}",
                BuildingType = type,
                Name = type == BuildingType.Factory ? $"{baseName} Fabrikası" : $"{baseName} Apartmanı",
                Address = MakeAddress(names, rng),
                Floors = type == BuildingType.Factory ? rng.RangeInclusive(1, 3) : rng.RangeInclusive(3, 8),
                Fee = rng.RangeInclusive(balance.ApprovalFeeRange.x, balance.ApprovalFeeRange.y)
            };

            // Uygunsuz projenin türü
            bool bribe = false, fake = false;
            if (!compliant)
            {
                if (config.AllowBribes && config.AllowFakeDocuments) { bribe = rng.Chance(0.5f); fake = !bribe; }
                else if (config.AllowBribes) bribe = true;
                else if (config.AllowFakeDocuments) fake = true;
            }

            // Parametreler
            var typeRules = new List<ParamRule>(rules.RulesFor(type));
            var failing = new HashSet<int>();
            if (!compliant && typeRules.Count > 0)
            {
                int failCount = Math.Min(typeRules.Count, rng.RangeInclusive(1, 2));
                while (failing.Count < failCount) failing.Add(rng.Next(typeRules.Count));
            }

            for (int i = 0; i < typeRules.Count; i++)
            {
                var r = typeRules[i];
                float actual = failing.Contains(i)
                    ? r.Required * rng.Range(r.FailRatioRange.x, r.FailRatioRange.y)
                    : r.Required * rng.Range(CompliantMarginMin, CompliantMarginMax);
                actual = Round1(actual);
                // Yuvarlama uygun/uygunsuz ayrımını bozmasın
                if (failing.Contains(i)) actual = Math.Min(actual, Round1(r.Required * 0.95f));
                else actual = Math.Max(actual, r.Required);

                // Sahte belge: kötü değer yönetmeliği geçiyor gibi yazılır
                float reported = fake && failing.Contains(i)
                    ? Round1(r.Required * rng.Range(1.02f, 1.15f))
                    : actual;

                p.Params.Add(new ParamEntry { Type = r.Param, Required = r.Required, Reported = reported, Actual = actual });
            }

            // Müşteri
            var client = PickClient(clients, compliant, bribe, config.AllowThugs, rng);
            if (client != null) p.ClientId = client.Id;

            if (bribe)
            {
                p.OffersBribe = true;
                var range = client != null ? client.BribeRange : new UnityEngine.Vector2Int(500, 2000);
                p.BribeAmount = RoundTo(rng.RangeInclusive(range.x, range.y), 50);
            }

            CreateDocuments(p, day, fake, names, rng);
            return p;
        }

        /// <summary>
        /// Müteahhidin rolü ClientSO alanlarından çıkar:
        ///  - Uygun proje      → IsHonest (Rıfat, Sevim)
        ///  - Rüşvetli proje   → OffersBribes (Kemal, Nuri; kabadayı günlerinde Rüstem)
        ///  - Diğer uygunsuz   → dürüst değil + rüşvet vermez (Selim: sahte belge / düz uygunsuz)
        /// Kabadayılar sadece DayConfig.AllowThugs günlerinde gelir. Uygun müteahhit yoksa null (yanlış rol atanmaz).
        /// </summary>
        static ClientSO PickClient(IReadOnlyList<ClientSO> clients, bool compliant, bool bribe, bool allowThugs, Random rng)
        {
            if (clients == null || clients.Count == 0) return null;
            var pool = new List<ClientSO>();
            foreach (var c in clients)
            {
                if (c == null || (c.IsThug && !allowThugs)) continue;
                if (compliant) { if (c.IsHonest) pool.Add(c); }
                else if (bribe) { if (c.OffersBribes) pool.Add(c); }
                else if (!c.IsHonest && !c.OffersBribes) pool.Add(c);
            }
            return pool.Count > 0 ? rng.Pick(pool) : null;
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
                Title = "Yapı Ruhsatı Başvurusu",
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

        static string LabStamp(string labName) => $"{UpperTr(labName)} · AKREDİTE";

        static void ApplyClue(DocumentData doc, ClueType clue, NamePoolSO names, Random rng)
        {
            switch (clue)
            {
                case ClueType.ParcelMismatch:
                    doc.ParcelNo = $"{rng.RangeInclusive(100, 999)}/{rng.RangeInclusive(1, 40)}";
                    break;
                case ClueType.DateMismatch:
                    doc.Date = $"{rng.RangeInclusive(1, 28):00}.{rng.RangeInclusive(10, 12):00}.2026";
                    break;
                case ClueType.UnknownLab:
                    // Başlık bilinmeyen lab; kaşe hâlâ eski lab adını taşır → başlık/kaşe uyuşmaz
                    doc.LabName = "Hızlı Sonuç Lab. Ltd.";
                    break;
                case ClueType.WrongStamp:
                    // Kaşe başka bir laboratuvarın → başlık/kaşe uyuşmaz
                    doc.StampId = LabStamp(PickOther(names != null ? names.LabNames : null, doc.LabName, "Kale Test Lab.", rng));
                    break;
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
        static string UpperTr(string s) => s.Replace('i', 'İ').Replace('ı', 'I').ToUpperInvariant();

        static string MakeAddress(NamePoolSO names, Random rng)
        {
            string hood = rng.Pick(names != null ? names.Neighborhoods : null, "Yeşiltepe");
            string street = rng.Pick(names != null ? names.Streets : null, "Lale");
            return $"{hood} Mah. {street} Sok. No: {rng.RangeInclusive(1, 60)}";
        }

        static float Round1(float v) => (float)Math.Round(v, 1);
        static int RoundTo(int v, int step) => (v / step) * step;
    }
}
