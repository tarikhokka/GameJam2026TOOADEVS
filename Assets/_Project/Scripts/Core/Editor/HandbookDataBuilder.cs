using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CivilJam.Core.Editor
{
    /// <summary>
    /// Denetçi El Kitabı v2 (17 Ekim öğle, Ahmet kararı): konut için 5 parametre × 3 kat grubu (1-3 / 4-6 / 7+ kat) = 15 kural.
    /// 2 sayısal (kolon kalınlığı, temel derinliği) + 3 seçenekli (beton sınıfı, demir tipi, zemin türü). Mahalle/bölge yok.
    /// EEE'nin yazdığı DisplayName / Description / HandbookIntro korunur (aynı parametre için). Değerler her çalıştırmada tablodan yazılır.
    /// Menü: CivilJam > Core > Rebuild Handbook Rules
    /// </summary>
    public static class HandbookDataBuilder
    {
        const string RuleSetPath = "Assets/_Project/Data/RuleSet.asset";

        static readonly (int min, int max)[] FloorGroups = { (1, 3), (4, 6), (7, 0) };

        struct Spec
        {
            public ParamType Param; public string Name; public string Unit; public List<string> Labels; public string Section;
            public string Description;
            public float[] Required;   // kat grubu sırasıyla: 1-3, 4-6, 7+
        }

        // Sıra = el kitabı sekme sırası (yeni hedef arayüz, Ahmet kararı 17 Ekim): BETON, DONATI, YAPI, ZEMİN.
        public static readonly string[] SectionOrder = { "BETON", "DONATI", "YAPI", "ZEMİN" };

        static readonly Spec[] Specs =
        {
            new Spec { Param = ParamType.ConcreteGrade, Section = "BETON", Name = "Beton sınıfı",
                Labels = new List<string> { "C16", "C20", "C25", "C30", "C35" },
                Description = "Betonun dayanımı. Sayı büyüdükçe beton sağlamlaşır; belirtilen sınıf ve üstü kabul edilir.",
                Required = new float[] { 1, 2, 3 } },                 // C20 / C25 / C30
            new Spec { Param = ParamType.RebarType, Section = "DONATI", Name = "Demir tipi",
                Labels = new List<string> { "B220 (düz)", "B420", "B500", "B500S" },
                Description = "Betonun içindeki demirin sınıfı. B220 düz demirdir, hiçbir binada kabul edilmez. " +
                              "Nervürlü demirlerde sayı büyüdükçe demir güçlenir; S harfi depreme dayanıklı sınıf demektir.",
                Required = new float[] { 1, 2, 3 } },                 // B420 / B500 / B500S (Ahmet kararı: kat grubuna göre 3 farklı tür)
            new Spec { Param = ParamType.ColumnThickness, Section = "YAPI", Name = "Kolon kalınlığı", Unit = "cm",
                Description = "Binayı taşıyan kolonların en ince kenarı. Kat arttıkça kolon kalınlaşmalı.",
                Required = new float[] { 30, 35, 40 } },
            new Spec { Param = ParamType.FoundationDepth, Section = "ZEMİN", Name = "Temel derinliği", Unit = "m",
                Description = "Temelin toprağın içine ne kadar indiği. Yüksek bina daha derin temel ister.",
                Required = new float[] { 1.5f, 2.0f, 2.5f } },
            new Spec { Param = ParamType.SoilType, Section = "ZEMİN", Name = "Zemin türü",
                Labels = new List<string> { "Dolgu", "Yumuşak kil", "Sert kil", "Kaya" },
                Description = "Binanın oturduğu toprak. Dolgu en zayıf, kaya en sağlam zemindir.",
                Required = new float[] { 1, 2, 3 } },                 // Yumuşak kil / Sert kil / Kaya
        };

        [MenuItem("CivilJam/Core/Rebuild Handbook Rules")]
        public static void Rebuild() => Build(preserveTexts: true);

        /// <summary>v1 → v2 geçişi için: eski (v1) açıklamaları taşımadan metinleri tablodan yazar. Batchmode.</summary>
        public static void RebuildFresh() => Build(preserveTexts: false);

        static void Build(bool preserveTexts)
        {
            var rules = AssetDatabase.LoadAssetAtPath<RuleSetSO>(RuleSetPath);
            if (rules == null) { Debug.LogError("[Handbook] RuleSet.asset yok"); return; }

            // EEE metinlerini koru (v2 parametreleri için yazılmışsa)
            var texts = new Dictionary<ParamType, ParamRule>();
            foreach (var r in rules.Rules)
                if (preserveTexts && r.BuildingType == BuildingType.Residential && !texts.ContainsKey(r.Param)) texts[r.Param] = r;

            var newRules = new List<ParamRule>();
            foreach (var r in rules.Rules)
                if (r.BuildingType != BuildingType.Residential) newRules.Add(r); // fabrika kuralları (kapsam dışı) aynen

            foreach (var spec in Specs)
            {
                texts.TryGetValue(spec.Param, out var old);
                for (int g = 0; g < FloorGroups.Length; g++)
                {
                    newRules.Add(new ParamRule
                    {
                        BuildingType = BuildingType.Residential,
                        Param = spec.Param,
                        DisplayName = old != null && !string.IsNullOrEmpty(old.DisplayName) ? old.DisplayName : spec.Name,
                        Unit = spec.Unit ?? "",
                        Description = old != null && !string.IsNullOrEmpty(old.Description) ? old.Description : spec.Description,
                        Labels = spec.Labels != null ? new List<string>(spec.Labels) : new List<string>(),
                        Section = spec.Section,
                        MinFloors = FloorGroups[g].min,
                        MaxFloors = FloorGroups[g].max,
                        Required = spec.Required[g],
                        FailRatioRange = new Vector2(0.6f, 0.9f)
                    });
                }
            }

            Undo.RecordObject(rules, "Rebuild Handbook Rules");
            rules.Rules = newRules;
            rules.HandbookIntro =
                "Bu el kitabı, Yapı Denetim Formu'nda aranacak en düşük değerleri içerir.\n\n" +
                "1. Formdan binanın kat adedini oku.\n" +
                "2. Sekmelerde (BETON, DONATI, YAPI, ZEMİN) o kat grubunun sütununu bul.\n" +
                "3. Formdaki beş değeri o sütunla karşılaştır; şüphelenirsen laboratuvardan ek test iste.\n" +
                "4. Form ile laboratuvar raporunun parsel, tarih, laboratuvar, kaşe ve imzası birbirini tutmalı.\n\n" +
                "Tek bir değer bile yetersizse yapı onaylanamaz.";
            EditorUtility.SetDirty(rules);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Handbook] v2: {newRules.FindAll(r => r.BuildingType == BuildingType.Residential).Count} konut kuralı (5 parametre × 3 kat grubu)");
        }
    }
}
