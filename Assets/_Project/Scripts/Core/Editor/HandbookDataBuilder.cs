using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CivilJam.Core.Editor
{
    /// <summary>
    /// Ç1: Denetçi El Kitabı'nın verisini RuleSet.asset'e yazar.
    ///  - 16 mahalle → deprem bölgesi (1 = en riskli, 4 = en az riskli); NamePool.Neighborhoods da bu listeye eşitlenir.
    ///  - Konut için 8 parametre × 4 bölge × 2 kat grubu (1-4 kat / 5+ kat). Perde duvar oranı sadece 5+ kat.
    /// EEE'nin yazdığı DisplayName / Unit / Description / HandbookIntro ve mahalle adları korunur (aynı sıradaki mahalle adı değiştiyse yeni ad kalır).
    /// Menü: CivilJam > Core > Rebuild Handbook Rules
    /// </summary>
    public static class HandbookDataBuilder
    {
        const string RuleSetPath = "Assets/_Project/Data/RuleSet.asset";
        const string NamePoolPath = "Assets/_Project/Data/NamePool.asset";

        // Taslak mahalle adları (kurgusal; EEE değiştirebilir). Sıra = bölge ataması.
        static readonly (string name, int zone)[] DefaultZones =
        {
            ("Yeşiltepe", 1), ("Karaağaç", 1), ("Dereboyu", 1), ("Taşocak", 1),
            ("Bağlarbaşı", 2), ("Kuşdili", 2), ("Gülbahçe", 2), ("Akarsu", 2),
            ("Sarıkaya", 3), ("Esenyurtlu", 3), ("Çamlıdere", 3), ("Kavaklı", 3),
            ("Yenikent", 4), ("Bayırlı", 4), ("Çiftlikönü", 4), ("Sakızlık", 4),
        };

        static readonly List<string> ConcreteLabels = new List<string> { "C16", "C20", "C25", "C30", "C35", "C40" };
        static readonly List<string> SoilLabels = new List<string> { "ZE", "ZD", "ZC", "ZB", "ZA" };

        struct Spec
        {
            public ParamType Param; public string Name; public string Unit; public List<string> Labels;
            public string Description;
            public float[] Low;   // 1-4 kat, bölge 1..4
            public float[] High;  // 5+ kat, bölge 1..4 (null = bu kat grubunda kural yok)
        }

        static readonly Spec[] Specs =
        {
            new Spec { Param = ParamType.ConcreteGrade, Name = "Beton sınıfı", Labels = ConcreteLabels,
                Description = "Taşıyıcı sistemde kullanılan betonun sınıfı. Belirtilen sınıf ve üstü kabul edilir.",
                Low = new float[] { 3, 2, 2, 1 }, High = new float[] { 4, 3, 2, 2 } },           // C30/C25/C25/C20 · C35/C30/C25/C25
            new Spec { Param = ParamType.RebarRatio, Name = "Donatı oranı", Unit = "%",
                Description = "Kolon kesitindeki boyuna donatı alanının kesit alanına oranı.",
                Low = new float[] { 1.2f, 1.0f, 0.9f, 0.8f }, High = new float[] { 1.4f, 1.2f, 1.0f, 0.9f } },
            new Spec { Param = ParamType.BarDiameter, Name = "Donatı çapı", Unit = "mm",
                Description = "Boyuna donatıda kullanılan en küçük çubuk çapı.",
                Low = new float[] { 16, 14, 14, 12 }, High = new float[] { 18, 16, 16, 14 } },
            new Spec { Param = ParamType.ColumnSection, Name = "Kolon kesiti", Unit = "cm²",
                Description = "Zemin kat kolonlarının en küçük kesit alanı.",
                Low = new float[] { 1600, 1400, 1200, 1000 }, High = new float[] { 2500, 2000, 1800, 1600 } },
            new Spec { Param = ParamType.SoilQuality, Name = "Zemin taşıma gücü", Unit = "kPa",
                Description = "Zemin etüdünde ölçülen izin verilebilir taşıma gücü.",
                Low = new float[] { 200, 175, 150, 150 }, High = new float[] { 250, 200, 175, 150 } },
            new Spec { Param = ParamType.SoilClass, Name = "Zemin sınıfı", Labels = SoilLabels,
                Description = "Zemin etüdü raporundaki yerel zemin sınıfı. ZA en sağlam, ZE en zayıftır.",
                Low = new float[] { 3, 2, 2, 1 }, High = new float[] { 3, 3, 2, 2 } },              // ZB/ZC/ZC/ZD · ZB/ZB/ZC/ZC
            new Spec { Param = ParamType.SeismicDesign, Name = "Deprem dayanım katsayısı", Unit = "",
                Description = "Statik hesapta kullanılan deprem dayanım katsayısı.",
                Low = new float[] { 1.2f, 1.0f, 0.8f, 0.6f }, High = new float[] { 1.4f, 1.2f, 1.0f, 0.8f } },
            new Spec { Param = ParamType.ShearWallRatio, Name = "Perde duvar oranı", Unit = "%",
                Description = "Betonarme perde duvar alanının kat alanına oranı. Sadece 5 ve üzeri katlı yapılarda aranır.",
                Low = null, High = new float[] { 2.0f, 1.5f, 1.2f, 1.0f } },
        };

        [MenuItem("CivilJam/Core/Rebuild Handbook Rules")]
        public static void Rebuild()
        {
            var rules = AssetDatabase.LoadAssetAtPath<RuleSetSO>(RuleSetPath);
            var names = AssetDatabase.LoadAssetAtPath<NamePoolSO>(NamePoolPath);
            if (rules == null) { Debug.LogError("[Handbook] RuleSet.asset yok"); return; }

            // EEE metinlerini koru
            var texts = new Dictionary<ParamType, ParamRule>();
            foreach (var r in rules.Rules)
                if (r.BuildingType == BuildingType.Residential && !texts.ContainsKey(r.Param)) texts[r.Param] = r;

            // Mahalleler: var olan adlar sırayla korunur, eksikler taslaktan
            var zones = new List<ZoneEntry>();
            for (int i = 0; i < DefaultZones.Length; i++)
            {
                string name = i < rules.Zones.Count && !string.IsNullOrEmpty(rules.Zones[i].Neighborhood)
                    ? rules.Zones[i].Neighborhood : DefaultZones[i].name;
                zones.Add(new ZoneEntry { Neighborhood = name, Zone = DefaultZones[i].zone });
            }
            for (int i = DefaultZones.Length; i < rules.Zones.Count; i++) zones.Add(rules.Zones[i]); // EEE fazladan eklediyse kalsın

            var newRules = new List<ParamRule>();
            foreach (var r in rules.Rules)
                if (r.BuildingType != BuildingType.Residential) newRules.Add(r); // fabrika kuralları (kapsam dışı) aynen

            foreach (var spec in Specs)
            {
                texts.TryGetValue(spec.Param, out var old);
                for (int zone = 1; zone <= 4; zone++)
                {
                    if (spec.Low != null) newRules.Add(MakeRule(spec, old, zone, 1, 4, spec.Low[zone - 1]));
                    if (spec.High != null) newRules.Add(MakeRule(spec, old, zone, 5, 0, spec.High[zone - 1]));
                }
            }

            Undo.RecordObject(rules, "Rebuild Handbook Rules");
            rules.Zones = zones;
            rules.Rules = newRules;
            if (string.IsNullOrEmpty(rules.HandbookIntro))
                rules.HandbookIntro = "Bu el kitabı, yapı ruhsatı başvurularında aranacak en düşük değerleri içerir.\n\n" +
                                      "1. Başvurudaki adresten mahalleyi bul.\n2. Mahallenin deprem bölgesini tablodan öğren.\n" +
                                      "3. O bölgenin sayfasında yapının kat sayısına uyan satırdaki değerlerle laboratuvar raporunu karşılaştır.\n\n" +
                                      "Tek bir değer bile yetersizse yapı onaylanamaz.";
            EditorUtility.SetDirty(rules);

            if (names != null)
            {
                Undo.RecordObject(names, "Sync neighborhoods");
                names.Neighborhoods = zones.ConvertAll(z => z.Neighborhood);
                EditorUtility.SetDirty(names);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Handbook] {zones.Count} mahalle, {newRules.Count} kural (konut: {newRules.FindAll(r => r.BuildingType == BuildingType.Residential).Count})");
        }

        static ParamRule MakeRule(Spec spec, ParamRule old, int zone, int minFloors, int maxFloors, float required) => new ParamRule
        {
            BuildingType = BuildingType.Residential,
            Param = spec.Param,
            DisplayName = old != null && !string.IsNullOrEmpty(old.DisplayName) && old.Param == spec.Param && old.Zone != 0 ? old.DisplayName : spec.Name,
            Unit = old != null && old.Zone != 0 && old.Unit != null ? old.Unit : spec.Unit ?? "",
            Description = old != null && !string.IsNullOrEmpty(old.Description) ? old.Description : spec.Description,
            Labels = spec.Labels != null ? new List<string>(spec.Labels) : new List<string>(),
            Zone = zone,
            MinFloors = minFloors,
            MaxFloors = maxFloors,
            Required = required,
            FailRatioRange = new Vector2(0.55f, 0.9f)
        };
    }
}
