using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CivilJam.Core.Tests
{
    /// <summary>Ç1: bölge + kat kuralları, el kitabıyla tutarlı proje üretimi, harfli değerler, gerçek RuleSet verisi.</summary>
    public class HandbookTests
    {
        RuleSetSO _rules;
        BalanceSO _balance;
        DayConfigSO _config;

        [SetUp]
        public void SetUp()
        {
            _balance = ScriptableObject.CreateInstance<BalanceSO>();
            _config = ScriptableObject.CreateInstance<DayConfigSO>();
            _rules = ScriptableObject.CreateInstance<RuleSetSO>();
            _rules.Zones = new List<ZoneEntry>
            {
                new ZoneEntry { Neighborhood = "Kuzey", Zone = 1 },
                new ZoneEntry { Neighborhood = "Güney", Zone = 4 },
            };
            var grades = new List<string> { "C16", "C20", "C25", "C30", "C35" };
            foreach (int zone in new[] { 1, 4 })
            {
                foreach (var (min, max) in new[] { (1, 4), (5, 0) })
                {
                    float k = (zone == 1 ? 1.5f : 1f) * (min == 5 ? 1.2f : 1f);
                    _rules.Rules.Add(R(ParamType.ConcreteGrade, zone, min, max, zone == 1 ? 3 : 1, grades));
                    _rules.Rules.Add(R(ParamType.RebarRatio, zone, min, max, 1.0f * k));
                    _rules.Rules.Add(R(ParamType.BarDiameter, zone, min, max, 12f * k));
                    _rules.Rules.Add(R(ParamType.ColumnSection, zone, min, max, 1000f * k));
                    _rules.Rules.Add(R(ParamType.SoilQuality, zone, min, max, 150f * k));
                    _rules.Rules.Add(R(ParamType.SeismicDesign, zone, min, max, 0.8f * k));
                    if (min == 5) _rules.Rules.Add(R(ParamType.ShearWallRatio, zone, min, max, 1.5f * k));
                }
            }
            // Genel (bölgesiz) kural: özel kural varken seçilmemeli
            _rules.Rules.Add(R(ParamType.RebarRatio, 0, 0, 0, 99f));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_rules);
            Object.DestroyImmediate(_balance);
            Object.DestroyImmediate(_config);
        }

        static ParamRule R(ParamType p, int zone, int min, int max, float req, List<string> labels = null) => new ParamRule
        {
            BuildingType = BuildingType.Residential, Param = p, Zone = zone, MinFloors = min, MaxFloors = max,
            Required = req, DisplayName = p.ToString(), Unit = labels == null ? "u" : "",
            Labels = labels ?? new List<string>(), FailRatioRange = new Vector2(0.55f, 0.9f)
        };

        [Test]
        public void Rules_PickMostSpecificForZoneAndFloors()
        {
            var low = _rules.FindRule(BuildingType.Residential, ParamType.RebarRatio, 1, 3);
            var high = _rules.FindRule(BuildingType.Residential, ParamType.RebarRatio, 1, 7);
            Assert.AreEqual(1.5f, low.Required, 1e-4f, "Bölge 1, 1-4 kat");
            Assert.AreEqual(1.8f, high.Required, 1e-4f, "Bölge 1, 5+ kat");
            Assert.IsNull(_rules.FindRule(BuildingType.Residential, ParamType.ShearWallRatio, 1, 3), "Perde duvar 1-4 katta aranmaz");
            Assert.AreEqual(1, _rules.ZoneOf("kuzey"));
            Assert.AreEqual(0, _rules.ZoneOf("Yok"));
            Assert.AreEqual(7, _rules.RulesFor(BuildingType.Residential, 4, 6).Count, "5+ katta 7 parametre");
        }

        [Test]
        public void FormatValue_LabelsAndNumbers()
        {
            Assert.AreEqual("C30", _rules.FormatValue(ParamType.ConcreteGrade, 3));
            Assert.AreEqual("1.000 u".Replace(".", ""), _rules.FormatValue(ParamType.ColumnSection, 1000).Replace(".", ""));
            StringAssert.Contains(",", _rules.FormatValue(ParamType.RebarRatio, 1.2f), "Ondalık virgülle yazılmalı");
        }

        [Test]
        public void GeneratedProjects_MatchHandbook()
        {
            _config.ProjectsPerDay = 6;
            _config.NonCompliantRatio = 0.5f;
            _config.AllowBribes = true;
            _config.AllowFakeDocuments = true;

            for (int seed = 0; seed < 300; seed++)
            {
                foreach (var p in ProjectGenerator.Generate(1, _config, _rules, _balance, null, null, new System.Random(seed)))
                {
                    StringAssert.StartsWith(p.Neighborhood + " Mah.", p.Address, "Adres mahalleyle başlamalı");
                    Assert.AreEqual(_rules.ZoneOf(p.Neighborhood), p.Zone);
                    Assert.That(p.Params.Count, Is.InRange(5, 6), "Her projede 5-6 parametre");

                    foreach (var x in p.Params)
                    {
                        var rule = _rules.FindRule(p.BuildingType, x.Type, p.Zone, p.Floors);
                        Assert.IsNotNull(rule, $"{x.Type} bu bölge/katta kuralsız");
                        Assert.AreEqual(rule.Required, x.Required, 1e-4f, "Gerekli değer el kitabındakiyle aynı olmalı");
                        Assert.AreEqual(rule.IsLabeled, x.Labeled);
                        if (x.Labeled)
                        {
                            foreach (var v in new[] { x.Required, x.Reported, x.Actual })
                            {
                                Assert.AreEqual(Mathf.Round(v), v, "Harfli değer tam sayı (etiket sırası)");
                                Assert.That(v, Is.InRange(0, rule.Labels.Count - 1));
                            }
                        }
                    }

                    if (p.Kind == ProjectKind.Compliant) Assert.IsTrue(p.Params.TrueForAll(x => x.ActualPasses));
                    else Assert.IsTrue(p.Params.Exists(x => !x.ActualPasses), $"{p.Kind} projede en az bir yetersiz değer olmalı");
                    if (p.Kind == ProjectKind.Fake || p.Kind == ProjectKind.BribeFake)
                        Assert.IsTrue(p.Params.TrueForAll(x => x.ReportedPasses), "Sahte belgede beyan hep geçiyor görünmeli");
                    Assert.AreEqual(p.Kind == ProjectKind.Compliant, p.IsCompliant);
                }
            }
        }

        [Test]
        public void RealRuleSet_CoversEveryZoneAndFloorGroup()
        {
            var real = AssetDatabase.LoadAssetAtPath<RuleSetSO>("Assets/_Project/Data/RuleSet.asset");
            Assert.IsNotNull(real);
            Assert.GreaterOrEqual(real.Zones.Count, 16, "En az 16 mahalle");
            for (int zone = 1; zone <= 4; zone++)
            {
                Assert.IsTrue(real.Zones.Exists(z => z.Zone == zone), $"Bölge {zone}'de mahalle yok");
                foreach (int floors in new[] { 3, 8 })
                    Assert.GreaterOrEqual(real.RulesFor(BuildingType.Residential, zone, floors).Count, 6,
                        $"Bölge {zone}, {floors} kat: en az 6 kural olmalı");
            }
            Assert.IsFalse(string.IsNullOrEmpty(real.HandbookIntro));
        }
    }
}
