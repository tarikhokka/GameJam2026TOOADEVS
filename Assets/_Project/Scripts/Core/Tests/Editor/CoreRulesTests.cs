using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CivilJam.Core.Tests
{
    public class CoreRulesTests
    {
        BalanceSO _balance;
        RuleSetSO _rules;
        DayConfigSO _config;

        [SetUp]
        public void SetUp()
        {
            _balance = ScriptableObject.CreateInstance<BalanceSO>();
            _rules = ScriptableObject.CreateInstance<RuleSetSO>();
            _rules.Rules = new List<ParamRule>
            {
                new ParamRule { BuildingType = BuildingType.Residential, Param = ParamType.ConcreteStrength, Required = 30f },
                new ParamRule { BuildingType = BuildingType.Residential, Param = ParamType.RebarRatio, Required = 1.0f },
                new ParamRule { BuildingType = BuildingType.Residential, Param = ParamType.SoilQuality, Required = 150f },
            };
            _config = ScriptableObject.CreateInstance<DayConfigSO>();
            _config.IntensityMin = 1f;
            _config.IntensityMax = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_balance);
            UnityEngine.Object.DestroyImmediate(_rules);
            UnityEngine.Object.DestroyImmediate(_config);
        }

        static ProjectData Building(string id, float ratio, int dayDecided) => new ProjectData
        {
            Id = id, Name = id, Floors = 4, DayDecided = dayDecided, Verdict = ProjectVerdict.Approved,
            Params = { new ParamEntry { Type = ParamType.ConcreteStrength, Required = 30f, Reported = 30f, Actual = 30f * ratio } }
        };

        [Test]
        public void StrengthAtLeastOne_NeverCollapses()
        {
            var rng = new System.Random(1);
            for (int i = 0; i < 100000; i++)
                Assert.IsFalse(EarthquakeSimulator.ShouldCollapse(1f, 1f, 0.15f, rng));
        }

        [Test]
        public void WeakBuilding_CollapsesUnderStrongQuake()
        {
            var rng = new System.Random(1);
            Assert.IsTrue(EarthquakeSimulator.ShouldCollapse(0.5f, 1f, 0.15f, rng));
        }

        [Test]
        public void Quake_OnlyEvaluatesBuildingsSinceLastDisaster()
        {
            var state = new GameState { Day = 6, LastDisasterDay = 3 };
            state.Buildings.Add(Building("old", 0.5f, 2));  // önceki afeti atlattı → değerlendirilmez
            state.Buildings.Add(Building("new", 0.5f, 5));

            var result = EarthquakeSimulator.Run(state, 1f, _balance, null, new System.Random(1));

            Assert.AreEqual(1, result.Outcomes.Count);
            Assert.AreEqual("new", result.Outcomes[0].ProjectId);
            Assert.AreEqual(BuildingStatus.Standing, state.Buildings[0].Status);
        }

        [Test]
        public void Collapse_ProducesCasualtiesAndResponsibility()
        {
            var state = new GameState { Day = 2 };
            state.Buildings.Add(Building("a", 0.5f, 1));

            var o = EarthquakeSimulator.Run(state, 1f, _balance, null, new System.Random(3)).Outcomes[0];

            Assert.IsTrue(o.Collapsed);
            Assert.Greater(o.Deaths, 0);
            Assert.AreEqual(o.Deaths, o.VictimNames.Count);
            Assert.IsFalse(string.IsNullOrEmpty(o.ResponsibilityText));
            Assert.Greater(state.JailRisk, 0f);
        }

        [Test]
        public void DisasterChance_IncreasesUntilCap_AndResetsAfterDisaster()
        {
            _balance.DisasterChanceStart = 0.5f;
            _balance.DisasterChanceIncrement = 0.1f;
            _balance.DisasterChanceCap = 1f;

            // Olasılık 0 → afet olmaz, artar
            var state = new GameState { Day = 1, DisasterChance = 0f };
            var r = DisasterSystem.EndOfDay(state, _config, _balance, null, new System.Random(1));
            Assert.IsFalse(r.Occurred);
            Assert.AreEqual(0.1f, state.DisasterChance, 1e-5f);

            // Tavanı geçmez
            state.DisasterChance = 0.95f;
            state.Day = 2;
            // 0.95'te afet çıkabilir; çıkmayan bir seed bul
            for (int seed = 0; seed < 1000; seed++)
            {
                var s = new GameState { Day = 2, DisasterChance = 0.95f };
                if (!DisasterSystem.EndOfDay(s, _config, _balance, null, new System.Random(seed)).Occurred)
                {
                    Assert.AreEqual(1f, s.DisasterChance, 1e-5f);
                    break;
                }
            }

            // Olasılık 1 → kesin afet, tabana döner, LastDisasterDay güncellenir
            var st = new GameState { Day = 7, DisasterChance = 1f };
            var q = DisasterSystem.EndOfDay(st, _config, _balance, null, new System.Random(1));
            Assert.IsTrue(q.Occurred);
            Assert.AreEqual(0.5f, st.DisasterChance, 1e-5f);
            Assert.AreEqual(7, st.LastDisasterDay);
        }

        [Test]
        public void Generator_IsDeterministic_AndRespectsRatio()
        {
            _config.ProjectsPerDay = 5;
            _config.NonCompliantRatio = 0.4f;
            _config.AllowBribes = true;
            _config.AllowFakeDocuments = true;

            var a = ProjectGenerator.Generate(1, _config, _rules, _balance, null, null, new System.Random(42));
            var b = ProjectGenerator.Generate(1, _config, _rules, _balance, null, null, new System.Random(42));

            Assert.AreEqual(5, a.Count);
            Assert.AreEqual(2, a.FindAll(p => !p.IsCompliant).Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].Name, b[i].Name);
                Assert.AreEqual(a[i].Strength, b[i].Strength);
            }

            foreach (var p in a)
            {
                if (p.OffersBribe) Assert.IsFalse(p.IsCompliant, "Rüşvet teklif eden her proje uygunsuzdur");
                if (p.Documents[1].Clues.Count > 0) Assert.IsFalse(p.IsCompliant, "Sahte belge sadece uygunsuz projede");
            }
        }

        static ClientSO Client(string id, bool honest, bool thug, int bribeMin, int bribeMax)
        {
            var c = ScriptableObject.CreateInstance<ClientSO>();
            c.Id = id; c.IsHonest = honest; c.IsThug = thug; c.BribeRange = new Vector2Int(bribeMin, bribeMax);
            return c;
        }

        [Test]
        public void Clients_MatchProjectRole()
        {
            // Tarık'ın 6 müteahhidinin aynısı (Data/Clients)
            var clients = new List<ClientSO>
            {
                Client("rifat", true, false, 0, 0), Client("sevim", true, false, 0, 0),
                Client("kemal", false, false, 800, 1500), Client("nuri", false, false, 1000, 2000),
                Client("selim", false, false, 0, 0), Client("rustem", false, true, 1500, 2500)
            };
            var byId = new Dictionary<string, ClientSO>();
            foreach (var c in clients) byId[c.Id] = c;

            _config.ProjectsPerDay = 6;
            _config.NonCompliantRatio = 0.5f;
            _config.AllowBribes = true;
            _config.AllowFakeDocuments = true;

            foreach (bool thugs in new[] { false, true })
            {
                _config.AllowThugs = thugs;
                bool sawRustem = false;
                for (int seed = 0; seed < 200; seed++)
                {
                    foreach (var p in ProjectGenerator.Generate(1, _config, _rules, _balance, null, clients, new System.Random(seed)))
                    {
                        Assert.IsFalse(string.IsNullOrEmpty(p.ClientId), "Her projeye bir müteahhit atanmalı");
                        var c = byId[p.ClientId];

                        if (p.IsCompliant) Assert.IsTrue(c.IsHonest, $"Uygun proje dürüst ustadan gelmeli ({c.Id})");
                        else Assert.IsFalse(c.IsHonest, $"Uygunsuz proje dürüst ustadan gelmemeli ({c.Id})");

                        if (p.OffersBribe)
                        {
                            Assert.IsTrue(c.OffersBribes, $"Rüşvetli proje rüşvet vermeyen müteahhide düştü ({c.Id})");
                            Assert.Greater(p.BribeAmount, 0, "0 TL zarf olmamalı");
                        }
                        else if (!p.IsCompliant)
                        {
                            Assert.IsFalse(c.OffersBribes, $"Rüşvetçi müteahhit rüşvetsiz proje getirdi ({c.Id})");
                        }

                        if (c.IsThug)
                        {
                            Assert.IsTrue(thugs, "Kabadayı, kabadayı günü değilken geldi");
                            sawRustem = true;
                        }
                    }
                }
                if (thugs) Assert.IsTrue(sawRustem, "Kabadayı günlerinde Rüstem en az bir kez gelmeli");
            }

            foreach (var c in clients) UnityEngine.Object.DestroyImmediate(c);
        }

        [Test]
        public void Stamps_AreReadableText_AndWrongStampMismatchesLab()
        {
            _config.ProjectsPerDay = 10;
            _config.NonCompliantRatio = 1f;
            _config.AllowFakeDocuments = true;

            bool sawWrongStamp = false;
            for (int seed = 0; seed < 50; seed++)
            {
                foreach (var p in ProjectGenerator.Generate(1, _config, _rules, _balance, null, null, new System.Random(seed)))
                {
                    foreach (var d in p.Documents)
                    {
                        Assert.IsFalse(string.IsNullOrEmpty(d.StampId));
                        StringAssert.DoesNotContain("STAMP", d.StampId, "Kaşe metni gerçek/sahte bilgisini sızdırmamalı");
                        StringAssert.DoesNotContain("FAKE", d.StampId);
                    }

                    var lab = p.Documents[1];
                    if (lab.Clues.Contains(ClueType.WrongStamp) && !lab.Clues.Contains(ClueType.UnknownLab))
                    {
                        sawWrongStamp = true;
                        StringAssert.DoesNotStartWith(lab.LabName.Replace('i', 'İ').Replace('ı', 'I').ToUpperInvariant(), lab.StampId);
                    }
                }
            }
            Assert.IsTrue(sawWrongStamp, "Test en az bir WrongStamp örneği görmeli");
        }

        [Test]
        public void Session_FullDayLoop_DoesNotDoublePay()
        {
            var expense = ScriptableObject.CreateInstance<ExpenseSO>();
            expense.Id = "rent"; expense.Amount = 400; expense.Mandatory = true;
            _config.ProjectsPerDay = 3;

            var ctx = new GameContext { Balance = _balance, Rules = _rules };
            var session = new GameSession(ctx, new[] { _config }, new[] { expense });
            session.NewGame(123);
            session.BeginDay();

            var first = ctx.TodayProjects[0];
            int before = ctx.State.Money;
            ctx.Actions.Approve(first.Id, false);
            ctx.Actions.Approve(first.Id, false); // ikinci çağrı etkisiz
            Assert.AreEqual(before + first.Fee, ctx.State.Money);

            session.FinishDesk();
            Assert.AreEqual(1, ctx.State.Buildings.Count);
            Assert.IsTrue(ctx.TodayProjects.TrueForAll(p => p.Verdict != ProjectVerdict.Pending));

            session.RunDisasterCheck();
            session.PrepareExpenses();
            int beforeExp = ctx.State.Money;
            session.FinishExpenses();
            session.FinishExpenses(); // ikinci çağrı etkisiz
            Assert.AreEqual(beforeExp - 400, ctx.State.Money);

            UnityEngine.Object.DestroyImmediate(expense);
        }
    }
}
