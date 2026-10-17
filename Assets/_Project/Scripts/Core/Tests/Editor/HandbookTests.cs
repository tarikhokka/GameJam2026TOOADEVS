using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CivilJam.Core.Tests
{
    /// <summary>El kitabı v2: kat grubuna göre kurallar, 5 parametre, seçenekli değerler, gerçek RuleSet verisi. Mahalle/bölge yok.</summary>
    public class HandbookTests
    {
        RuleSetSO _rules;
        BalanceSO _balance;
        DayConfigSO _config;

        static readonly (int min, int max)[] Groups = { (1, 3), (4, 6), (7, 0) };

        [SetUp]
        public void SetUp()
        {
            _balance = ScriptableObject.CreateInstance<BalanceSO>();
            _config = ScriptableObject.CreateInstance<DayConfigSO>();
            _rules = ScriptableObject.CreateInstance<RuleSetSO>();
            for (int g = 0; g < Groups.Length; g++)
            {
                var (min, max) = Groups[g];
                _rules.Rules.Add(R(ParamType.ColumnThickness, min, max, 30 + 5 * g, unit: "cm"));
                _rules.Rules.Add(R(ParamType.FoundationDepth, min, max, 1.5f + 0.5f * g, unit: "m"));
                _rules.Rules.Add(R(ParamType.ConcreteGrade, min, max, 1 + g, new List<string> { "C16", "C20", "C25", "C30", "C35" }));
                _rules.Rules.Add(R(ParamType.RebarType, min, max, 1 + g, new List<string> { "B220 (düz)", "B420", "B500", "B500S" }));
                _rules.Rules.Add(R(ParamType.SoilType, min, max, 1 + g, new List<string> { "Dolgu", "Yumuşak kil", "Sert kil", "Kaya" }));
            }
            // Genel (katsız) kural: kata özel kural varken seçilmemeli
            _rules.Rules.Add(R(ParamType.ColumnThickness, 0, 0, 99f, unit: "cm"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_rules);
            Object.DestroyImmediate(_balance);
            Object.DestroyImmediate(_config);
        }

        static ParamRule R(ParamType p, int min, int max, float req, List<string> labels = null, string unit = "") => new ParamRule
        {
            BuildingType = BuildingType.Residential, Param = p, MinFloors = min, MaxFloors = max,
            Required = req, DisplayName = p.ToString(), Unit = unit,
            Labels = labels ?? new List<string>(), FailRatioRange = new Vector2(0.6f, 0.9f)
        };

        [Test]
        public void Rules_PickByFloorGroup()
        {
            Assert.AreEqual(30f, _rules.FindRule(BuildingType.Residential, ParamType.ColumnThickness, 2).Required);
            Assert.AreEqual(35f, _rules.FindRule(BuildingType.Residential, ParamType.ColumnThickness, 5).Required);
            Assert.AreEqual(40f, _rules.FindRule(BuildingType.Residential, ParamType.ColumnThickness, 9).Required, "7+ kat, genel kural değil");
            Assert.AreEqual(5, _rules.RulesFor(BuildingType.Residential, 4).Count, "Her binada 5 parametre");
            Assert.AreEqual("7+ kat", _rules.FindRule(BuildingType.Residential, ParamType.SoilType, 8).FloorsLabel);
            Assert.AreEqual("1-3 kat", _rules.FindRule(BuildingType.Residential, ParamType.SoilType, 1).FloorsLabel);
        }

        [Test]
        public void FormatValue_LabelsAndNumbers()
        {
            Assert.AreEqual("B500S", _rules.FormatValue(ParamType.RebarType, 3));
            Assert.AreEqual("Sert kil", _rules.FormatValue(ParamType.SoilType, 2));
            Assert.AreEqual("C25", _rules.FormatValue(ParamType.ConcreteGrade, 2));
            Assert.AreEqual("35 cm", _rules.FormatValue(ParamType.ColumnThickness, 35));
            Assert.AreEqual("2,5 m", _rules.FormatValue(ParamType.FoundationDepth, 2.5f), "Ondalık virgülle yazılmalı");
        }

        [Test]
        public void GeneratedProjects_MatchHandbook()
        {
            _config.ProjectsPerDay = 6;
            _config.NonCompliantRatio = 0.5f;
            _config.AllowBribes = true;
            _config.AllowFakeDocuments = true;
            var seenGroups = new HashSet<string>();

            for (int seed = 0; seed < 300; seed++)
            {
                foreach (var p in ProjectGenerator.Generate(1, _config, _rules, _balance, null, null, new System.Random(seed)))
                {
                    Assert.That(p.Floors, Is.InRange(2, 9));
                    Assert.AreEqual(5, p.Params.Count, "Her projede 5 parametre");
                    StringAssert.DoesNotContain("Mah.", p.Address, "Mahalle kaldırıldı");

                    foreach (var x in p.Params)
                    {
                        var rule = _rules.FindRule(p.BuildingType, x.Type, p.Floors);
                        Assert.IsNotNull(rule);
                        seenGroups.Add(rule.FloorsLabel);
                        Assert.AreEqual(rule.Required, x.Required, 1e-4f, "Gerekli değer el kitabındakiyle aynı olmalı");
                        Assert.AreEqual(rule.IsLabeled, x.Labeled);
                        if (x.Labeled)
                            foreach (var v in new[] { x.Required, x.Reported, x.Actual })
                            {
                                Assert.AreEqual(Mathf.Round(v), v, "Seçenekli değer tam sayı (seçenek sırası)");
                                Assert.That(v, Is.InRange(0, rule.Labels.Count - 1));
                            }
                    }

                    if (p.Kind == ProjectKind.Compliant) Assert.IsTrue(p.Params.TrueForAll(x => x.ActualPasses));
                    else Assert.IsTrue(p.Params.Exists(x => !x.ActualPasses), $"{p.Kind} projede en az bir yetersiz değer olmalı");
                    if (p.Kind == ProjectKind.Fake || p.Kind == ProjectKind.BribeFake)
                        Assert.IsTrue(p.Params.TrueForAll(x => x.ReportedPasses), "Sahte belgede beyan hep geçiyor görünmeli");
                    Assert.AreEqual(p.Kind == ProjectKind.Compliant, p.IsCompliant);
                }
            }
            CollectionAssert.IsSupersetOf(seenGroups, new[] { "1-3 kat", "4-6 kat", "7+ kat" }, "Üç kat grubu da gelmeli");
        }

        [Test]
        public void RealRuleSet_IsHandbookV2()
        {
            var real = AssetDatabase.LoadAssetAtPath<RuleSetSO>("Assets/_Project/Data/RuleSet.asset");
            Assert.IsNotNull(real);
            var expected = new[] { ParamType.ColumnThickness, ParamType.FoundationDepth, ParamType.ConcreteGrade, ParamType.RebarType, ParamType.SoilType };
            foreach (int floors in new[] { 2, 5, 8 })
            {
                var applied = real.RulesFor(BuildingType.Residential, floors);
                Assert.AreEqual(5, applied.Count, $"{floors} kat: 5 parametre olmalı");
                foreach (var p in expected)
                    Assert.IsTrue(applied.Exists(r => r.Param == p), $"{floors} kat: {p} yok");
                Assert.AreEqual(2, applied.FindAll(r => !r.IsLabeled).Count, "2 sayısal parametre");
            }
            // Demir tipi kat grubuna göre 3 farklı tür ister; en zayıf seçenek (düz) hiçbir grupta yeterli değil
            var rebar = new HashSet<float>();
            foreach (int floors in new[] { 2, 5, 8 })
            {
                var r = real.FindRule(BuildingType.Residential, ParamType.RebarType, floors);
                Assert.Greater(r.Required, 0f, "Düz demir hiçbir binada kabul edilmez");
                rebar.Add(r.Required);
            }
            Assert.AreEqual(3, rebar.Count, "Demir tipi: kat grubuna göre 3 farklı tür");
            Assert.IsFalse(string.IsNullOrEmpty(real.HandbookIntro));
        }
    }
}
