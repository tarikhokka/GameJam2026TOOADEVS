using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CivilJam.Core.Tests
{
    /// <summary>İnceleme modu kural tablosu (memory.md "🔎"). Gerçek RuleSet ve NamePool ile.</summary>
    public class InspectionTests
    {
        RuleSetSO _rules;
        NamePoolSO _names;
        GameContext _ctx;

        const int Group13 = 0, Group46 = 1, Group7 = 2;

        [SetUp]
        public void SetUp()
        {
            _rules = AssetDatabase.LoadAssetAtPath<RuleSetSO>("Assets/_Project/Data/RuleSet.asset");
            _names = AssetDatabase.LoadAssetAtPath<NamePoolSO>("Assets/_Project/Data/NamePool.asset");
            Assume.That(_rules, Is.Not.Null);
            Assume.That(_names, Is.Not.Null);
            _ctx = new GameContext { Rules = _rules, Names = _names };
        }

        // 5 katlı (4-6 grubu) bina; kolon 33 cm beyan (eşik 35 → uygunsuz), beton C25 (eşik C25 → uygun), gerçek beton C20
        ProjectData Project()
        {
            string lab = _names.LabNames[0];
            var p = new ProjectData { Id = "T1", Floors = 5, BuildingType = BuildingType.Residential };
            p.Params.Add(new ParamEntry { Type = ParamType.ColumnThickness, Required = 35, Reported = 33, Actual = 33 });
            p.Params.Add(new ParamEntry { Type = ParamType.ConcreteGrade, Required = 2, Reported = 2, Actual = 1, Labeled = true });
            p.Documents.Add(new DocumentData { Kind = DocumentKind.Application, ParcelNo = "561/8", Date = "12.05.2026", SignatureName = "Müh. S. Erdem" });
            p.Documents.Add(new DocumentData { Kind = DocumentKind.LabReport, ParcelNo = "561/8", Date = "12.05.2026", SignatureName = "Müh. S. Erdem",
                LabName = lab, StampLabName = lab, StampId = lab.ToUpperInvariant() + " · AKREDİTE" });
            return p;
        }

        InspectVerdict V(ProjectData p, InspectTarget a, InspectTarget b, int group = Group46)
        {
            var r1 = Inspection.Check(_ctx, p, a, b, group);
            var r2 = Inspection.Check(_ctx, p, b, a, group);
            Assert.AreEqual(r1.Verdict, r2.Verdict, "Sıra sonucu değiştirmemeli");
            Assert.IsFalse(string.IsNullOrEmpty(r1.Title));
            Assert.IsFalse(string.IsNullOrEmpty(r1.Detail));
            return r1.Verdict;
        }

        static InspectTarget FormP(ParamType t) => InspectTarget.OfParam(InspectDoc.Form, t);
        static InspectTarget LabP(ParamType t) => InspectTarget.OfParam(InspectDoc.Lab, t);
        static InspectTarget BookP(ParamType t) => InspectTarget.OfParam(InspectDoc.Handbook, t);
        static InspectTarget F(InspectField f) => InspectTarget.Of(InspectDoc.Form, f);
        static InspectTarget L(InspectField f) => InspectTarget.Of(InspectDoc.Lab, f);
        static readonly InspectTarget Labs = InspectTarget.Of(InspectDoc.Handbook, InspectField.AccreditedLabs);

        [Test]
        public void Rows1And2_DeclaredValueVsHandbook_UsesSelectedGroup()
        {
            var p = Project();
            Assert.AreEqual(InspectVerdict.Match, V(p, FormP(ParamType.ConcreteGrade), BookP(ParamType.ConcreteGrade)));
            Assert.AreEqual(InspectVerdict.Mismatch, V(p, FormP(ParamType.ColumnThickness), BookP(ParamType.ColumnThickness)));
            Assert.AreEqual(InspectVerdict.Mismatch, V(p, LabP(ParamType.ColumnThickness), BookP(ParamType.ColumnThickness)));
            // Yanlış ya da seçilmemiş grup → alakasız (Ahmet kararı)
            Assert.AreEqual(InspectVerdict.Unrelated, V(p, FormP(ParamType.ColumnThickness), BookP(ParamType.ColumnThickness), Group13));
            Assert.AreEqual(InspectVerdict.Unrelated, V(p, FormP(ParamType.ColumnThickness), BookP(ParamType.ColumnThickness), Group7));
            Assert.AreEqual(InspectVerdict.Unrelated, V(p, FormP(ParamType.ColumnThickness), BookP(ParamType.ColumnThickness), Inspection.NoFloorGroup));
        }

        [Test]
        public void Rows3And4_ExtraTest_OnlyAfterTest()
        {
            var p = Project();
            var test = InspectTarget.OfTest(ParamType.ConcreteGrade);
            Assert.IsFalse(Inspection.IsAvailable(p, test));
            Assert.AreEqual(InspectVerdict.Unrelated, V(p, FormP(ParamType.ConcreteGrade), test), "Test yapılmadan ek test hücresi alakasız");

            p.GetParam(ParamType.ConcreteGrade).Tested = true;
            Assert.IsTrue(Inspection.IsAvailable(p, test));
            Assert.AreEqual(InspectVerdict.Mismatch, V(p, FormP(ParamType.ConcreteGrade), test), "Beyan C25, gerçek C20");
            Assert.AreEqual(InspectVerdict.Mismatch, V(p, test, BookP(ParamType.ConcreteGrade)), "Gerçek C20 < C25");
        }

        [Test]
        public void Row5_FormVsLabDeclared_MatchUnlessLabDiffers()
        {
            var p = Project();
            Assert.AreEqual(InspectVerdict.Match, V(p, FormP(ParamType.ColumnThickness), LabP(ParamType.ColumnThickness)));

            // Değer farkı: lab raporu formdan farklı beyan yazar → UYUŞMAZLIK; lab ↔ El Kitabı lab'daki değere bakar
            var e = p.GetParam(ParamType.ColumnThickness);
            e.LabDiffers = true;
            e.LabReported = 36;
            Assert.AreEqual(InspectVerdict.Mismatch, V(p, FormP(ParamType.ColumnThickness), LabP(ParamType.ColumnThickness)));
            Assert.AreEqual(InspectVerdict.Match, V(p, LabP(ParamType.ColumnThickness), BookP(ParamType.ColumnThickness)), "Lab 36 ≥ 35");
            Assert.AreEqual(InspectVerdict.Mismatch, V(p, FormP(ParamType.ColumnThickness), BookP(ParamType.ColumnThickness)), "Form 33 < 35");
        }

        [Test]
        public void Rows6To8_Kunye()
        {
            var p = Project();
            Assert.AreEqual(InspectVerdict.Match, V(p, F(InspectField.Parcel), L(InspectField.Parcel)));
            Assert.AreEqual(InspectVerdict.Match, V(p, F(InspectField.Date), L(InspectField.Date)));
            Assert.AreEqual(InspectVerdict.Match, V(p, F(InspectField.Signature), L(InspectField.Signature)));

            p.Documents[1].ParcelNo = "123/4";
            p.Documents[1].Date = "03.11.2026";
            p.Documents[1].SignatureName = "Müh. K. Durmaz";
            Assert.AreEqual(InspectVerdict.Mismatch, V(p, F(InspectField.Parcel), L(InspectField.Parcel)));
            Assert.AreEqual(InspectVerdict.Mismatch, V(p, F(InspectField.Date), L(InspectField.Date)));
            Assert.AreEqual(InspectVerdict.Mismatch, V(p, F(InspectField.Signature), L(InspectField.Signature)));
        }

        [Test]
        public void Rows9And10_AccreditedList()
        {
            var p = Project();
            Assert.AreEqual(InspectVerdict.Match, V(p, L(InspectField.LabName), Labs));
            Assert.AreEqual(InspectVerdict.Match, V(p, L(InspectField.LabStamp), Labs));

            p.Documents[1].LabName = "Hızlı Sonuç Lab. Ltd.";
            p.Documents[1].StampLabName = "Kale Test Lab.";
            Assert.AreEqual(InspectVerdict.Mismatch, V(p, L(InspectField.LabName), Labs));
            Assert.AreEqual(InspectVerdict.Mismatch, V(p, L(InspectField.LabStamp), Labs));

            // Eski veri: StampLabName boşsa kaşe metninden okunur
            p.Documents[1].StampLabName = null;
            p.Documents[1].StampId = _names.LabNames[1].ToUpperInvariant() + " · AKREDİTE";
            Assert.AreEqual(InspectVerdict.Match, V(p, L(InspectField.LabStamp), Labs));
        }

        [Test]
        public void Unrelated_Pairs()
        {
            var p = Project();
            Assert.AreEqual(InspectVerdict.Unrelated, V(p, FormP(ParamType.ConcreteGrade), BookP(ParamType.ColumnThickness)), "Farklı parametre");
            Assert.AreEqual(InspectVerdict.Unrelated, V(p, F(InspectField.Parcel), F(InspectField.Date)), "Aynı belge");
            Assert.AreEqual(InspectVerdict.Unrelated, V(p, LabP(ParamType.ConcreteGrade), L(InspectField.LabName)), "Aynı belge");
            Assert.AreEqual(InspectVerdict.Unrelated, V(p, F(InspectField.Parcel), L(InspectField.Date)), "Farklı künye");
            Assert.AreEqual(InspectVerdict.Unrelated, V(p, F(InspectField.Parcel), Labs));
            Assert.AreEqual(InspectVerdict.Unrelated, V(p, FormP(ParamType.ConcreteGrade), L(InspectField.Parcel)));
            Assert.AreEqual(InspectVerdict.Unrelated, V(p, F(InspectField.Signature), BookP(ParamType.ConcreteGrade)));
        }

        // Üretilen projelerde: sahte olmayan belgede künye hiç uyuşmazlık vermez; dürüst projedeki değer farkı sadece uygun projede
        // masum yazım hatasıdır (gerçek değerler iyi); her sahte belge künye ya da değer farkıyla yakalanabilir; bazı sahtelerde
        // künye ipucu hiç yoktur (sadece değer farkı → ancak ek test kanıtlar).
        [Test]
        public void GeneratedProjects_KunyeMismatchOnlyOnFakes_ValueDiffInnocentOnHonest_AndEveryFakeIsCatchable()
        {
            var config = ScriptableObject.CreateInstance<DayConfigSO>();
            var balance = ScriptableObject.CreateInstance<BalanceSO>();
            config.ProjectsPerDay = 6; config.NonCompliantRatio = 0.5f; config.AllowBribes = true; config.AllowFakeDocuments = true;
            var pairs = new[]
            {
                (F(InspectField.Parcel), L(InspectField.Parcel)),
                (F(InspectField.Date), L(InspectField.Date)),
                (F(InspectField.Signature), L(InspectField.Signature)),
                (L(InspectField.LabName), Labs),
                (L(InspectField.LabStamp), Labs),
            };
            int fakes = 0, valueOnlyFakes = 0, typos = 0;
            for (int seed = 0; seed < 400; seed++)
                foreach (var p in ProjectGenerator.Generate(3, config, _rules, balance, _names, null, new System.Random(seed)))
                {
                    bool fake = p.Kind == ProjectKind.Fake || p.Kind == ProjectKind.BribeFake;
                    int mismatches = 0, valueDiffs = 0;
                    foreach (var (a, b) in pairs)
                        if (Inspection.Check(_ctx, p, a, b, Inspection.NoFloorGroup).Verdict == InspectVerdict.Mismatch) mismatches++;
                    foreach (var e in p.Params)
                    {
                        if (Inspection.Check(_ctx, p, FormP(e.Type), LabP(e.Type), Inspection.NoFloorGroup).Verdict != InspectVerdict.Mismatch) continue;
                        valueDiffs++;
                        Assert.IsTrue(e.ReportedPasses && e.LabValue >= e.Required, "Değer farkında iki beyan da yönetmeliği geçiyor görünmeli");
                        Assert.AreEqual(fake, !e.ActualPasses, "Değer farkı: sahtede gerçek değer kötü, dürüstte iyi");
                    }
                    if (fake)
                    {
                        fakes++;
                        Assert.Greater(mismatches + valueDiffs, 0, $"Sahte belge yakalanabilmeli ({string.Join(",", p.Documents[1].Clues)})");
                        if (mismatches == 0) valueOnlyFakes++;
                    }
                    else
                    {
                        Assert.AreEqual(0, mismatches, "Sahte olmayan belgede künye uyuşmazlığı çıkmamalı");
                        if (valueDiffs > 0) { typos++; Assert.AreEqual(ProjectKind.Compliant, p.Kind, "Masum değer farkı sadece uygun projede"); }
                    }
                }
            Assert.Greater(fakes, 50);
            Assert.Greater(valueOnlyFakes, 10, "Bazı sahtelerde künye ipucu olmamalı (sadece ek test kanıtlar)");
            Assert.Greater(typos, 10, "Uygun projelerde masum yazım hatası da çıkmalı");
            Object.DestroyImmediate(config);
            Object.DestroyImmediate(balance);
        }
    }
}
