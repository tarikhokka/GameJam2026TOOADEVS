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

        static ExpenseSO Expense(string id, int amount, bool mandatory)
        {
            var e = ScriptableObject.CreateInstance<ExpenseSO>();
            e.Id = id; e.DisplayName = id; e.Amount = amount; e.Mandatory = mandatory;
            return e;
        }

        [Test]
        public void Expenses_NeverMakeMoneyNegative_UnpaidMandatoryBecomesDebt()
        {
            var rent = Expense("rent", 400, true);
            var loan = Expense("loan", 300, true);
            var state = new GameState { Day = 1, Money = 500 };

            var r = ExpenseCalculator.Build(state, new[] { rent, loan }, _balance);
            Assert.IsTrue(r.Lines[0].Paid, "Kira kasadan karşılanabilir, işaretli gelmeli");
            Assert.IsFalse(r.Lines[1].Paid, "Kasa yetmeyen zorunlu kalem işaretsiz gelmeli");

            r.Lines[1].Paid = true; // oyuncu zorla işaretlese bile
            ExpenseCalculator.Apply(state, r);
            Assert.AreEqual(100, state.Money);
            Assert.AreEqual(300, state.Debt);
            Assert.GreaterOrEqual(state.Money, 0);

            // Ertesi gün borç kısmen ödenebilir
            state.Money = 1000;
            var r2 = ExpenseCalculator.Build(state, new ExpenseSO[0], _balance);
            var debtLine = r2.Lines.Find(l => l.ExpenseId == ExpenseCalculator.DebtPaymentId);
            Assert.IsNotNull(debtLine);
            debtLine.Paid = true;
            ExpenseCalculator.Apply(state, r2);
            Assert.AreEqual(0, state.Debt);
            Assert.AreEqual(700, state.Money);

            UnityEngine.Object.DestroyImmediate(rent);
            UnityEngine.Object.DestroyImmediate(loan);
        }

        [Test]
        public void Endings_FollowPriority()
        {
            var ctx = new GameContext { Balance = _balance, Rules = _rules };
            var session = new GameSession(ctx, new[] { _config }, new ExpenseSO[0]);
            _balance.TotalDays = 10;
            _balance.DebtLimit = 2000;

            session.NewGame(1);
            ctx.State.Day = 3;
            Assert.AreEqual(EndingType.None, session.EvaluateEnding(), "Gün ortasında son yok");

            ctx.State.Day = 10;
            Assert.AreEqual(EndingType.Good, session.EvaluateEnding(), "Yıkımsız son gün → İyi");

            session.NewGame(1); ctx.State.Day = 10; ctx.State.TotalCollapses = 1;
            Assert.AreEqual(EndingType.MiddleCollapsed, session.EvaluateEnding(), "Yıkım var, hapis yok → Orta (yıkım)");

            session.NewGame(1); ctx.State.Day = 4; ctx.State.Debt = 2100;
            Assert.AreEqual(EndingType.MiddleClosed, session.EvaluateEnding(), "Borç limiti aşıldı → ofis kapandı (gün ortasında da)");

            session.NewGame(1); ctx.State.Day = 10; ctx.State.Debt = 2100; ctx.State.Jailed = true; ctx.State.TotalCollapses = 2;
            Assert.AreEqual(EndingType.Bad, session.EvaluateEnding(), "Hapis her şeyden önce gelir");
        }

        [Test]
        public void RejectEverything_LosesOfficeOnDaySeven()
        {
            // Data/Balance.asset ve Expenses ile aynı değerler (karar 2026-10-17)
            _balance.StartMoney = 2500; _balance.DebtLimit = 2000; _balance.TotalDays = 10;
            var rent = Expense("rent", 400, true);
            var loan = Expense("loan", 300, true);
            _config.ProjectsPerDay = 3;

            var ctx = new GameContext { Balance = _balance, Rules = _rules };
            var session = new GameSession(ctx, new[] { _config }, new[] { rent, loan });
            session.NewGame(7);

            EndingType ending = EndingType.None;
            while (ending == EndingType.None && ctx.State.Day <= 20)
            {
                session.BeginDay();
                session.FinishDesk();          // karar verilmeyenler reddedilir
                session.RunDisasterCheck();
                session.PrepareExpenses();
                session.FinishExpenses();
                Assert.GreaterOrEqual(ctx.State.Money, 0, "Kasa eksiye düşmemeli");
                ending = session.EvaluateEnding();
                if (ending == EndingType.None) ctx.State.Day++;
            }

            Assert.AreEqual(EndingType.MiddleClosed, ending, "Her şeyi reddeden oyuncu ofisini kaybetmeli");
            // 2.500 TL: 3 gün tam ödenir, 4. gün kira ödenir + kredi borca, 5-6-7. gün borç 2.400 > 2.000
            Assert.AreEqual(7, ctx.State.Day);

            UnityEngine.Object.DestroyImmediate(rent);
            UnityEngine.Object.DestroyImmediate(loan);
        }

        static ClientSO Client(string id, bool honest, bool thug, int bribeMin, int bribeMax)
        {
            var c = ScriptableObject.CreateInstance<ClientSO>();
            c.Id = id; c.IsHonest = honest; c.IsThug = thug; c.BribeRange = new Vector2Int(bribeMin, bribeMax);
            return c;
        }

        static List<ClientSO> TarikClients() => new List<ClientSO>
        {
            // Tarık'ın 6 müteahhidinin aynısı (Data/Clients)
            Client("rifat", true, false, 0, 0), Client("sevim", true, false, 0, 0),
            Client("kemal", false, false, 800, 1500), Client("nuri", false, false, 1000, 2000),
            Client("selim", false, false, 0, 0), Client("rustem", false, true, 1500, 2500)
        };

        [Test]
        public void Clients_FollowRoleRules_ButNameDoesNotRevealKind()
        {
            var clients = TarikClients();
            var byId = new Dictionary<string, ClientSO>();
            foreach (var c in clients) byId[c.Id] = c;

            _config.ProjectsPerDay = 6;
            _config.NonCompliantRatio = 0.5f;
            _config.AllowBribes = true;
            _config.AllowFakeDocuments = true;

            var kindsPerClient = new Dictionary<string, HashSet<ProjectKind>>();
            foreach (bool thugs in new[] { false, true })
            {
                _config.AllowThugs = thugs;
                for (int seed = 0; seed < 300; seed++)
                {
                    foreach (var p in ProjectGenerator.Generate(1, _config, _rules, _balance, null, clients, new System.Random(seed)))
                    {
                        Assert.IsFalse(string.IsNullOrEmpty(p.ClientId), "Her projeye bir müteahhit atanmalı");
                        var c = byId[p.ClientId];

                        if (p.OffersBribe)
                        {
                            Assert.IsTrue(c.OffersBribes, $"Rüşvetli proje rüşvet vermeyen müteahhide düştü ({c.Id})");
                            Assert.Greater(p.BribeAmount, 0, "0 TL zarf olmamalı");
                            Assert.IsFalse(p.IsCompliant, "Rüşvet teklif eden her proje uygunsuzdur");
                        }
                        if (c.IsHonest)
                        {
                            Assert.IsFalse(p.OffersBribe, "Dürüst usta rüşvet teklif etmez");
                            Assert.IsTrue(p.Kind == ProjectKind.Compliant || p.Kind == ProjectKind.Sloppy,
                                $"Dürüst usta sahte belge getirmez ({p.Kind})");
                        }
                        if (c.IsThug) Assert.IsTrue(thugs, "Kabadayı, kabadayı günü değilken geldi");

                        if (!kindsPerClient.TryGetValue(c.Id, out var set)) kindsPerClient[c.Id] = set = new HashSet<ProjectKind>();
                        set.Add(p.Kind);
                    }
                }
            }

            // Ç3: hiçbir müteahhidin adı tek bir türü ele vermemeli
            foreach (var kv in kindsPerClient)
                Assert.Greater(kv.Value.Count, 1, $"{kv.Key} hep aynı tür proje getiriyor: {string.Join(",", kv.Value)}");
            Assert.IsTrue(kindsPerClient["rifat"].Contains(ProjectKind.Sloppy), "Dürüst usta da ara sıra dikkatsiz olmalı");
            Assert.IsTrue(kindsPerClient["kemal"].Contains(ProjectKind.Compliant), "Rüşvetçi de ara sıra temiz proje getirmeli");

            foreach (var c in clients) UnityEngine.Object.DestroyImmediate(c);
        }

        [Test]
        public void Clients_DoNotComeTwiceInARow()
        {
            // EEE isteği: aynı müteahhit arka arkaya gelmez (gün içinde ve dünün sonuyla bugünün başı arasında)
            var clients = TarikClients();
            _config.ProjectsPerDay = 6;
            _config.NonCompliantRatio = 0.5f;
            _config.AllowBribes = true;
            _config.AllowFakeDocuments = true;

            foreach (bool thugs in new[] { false, true })
            {
                _config.AllowThugs = thugs;
                for (int seed = 0; seed < 300; seed++)
                {
                    string previous = seed % 2 == 0 ? "selim" : "rifat";
                    foreach (var p in ProjectGenerator.Generate(3, _config, _rules, _balance, null, clients,
                                 new System.Random(seed), null, previous))
                    {
                        Assert.AreNotEqual(previous, p.ClientId, $"seed {seed}: {p.Id} bir öncekiyle aynı müteahhit ({p.ClientId})");
                        previous = p.ClientId;
                    }
                }
            }

            foreach (var c in clients) UnityEngine.Object.DestroyImmediate(c);
        }

        [Test]
        public void Clients_ComeOncePerDay_WhenPossible()
        {
            // EEE isteği: bir müteahhit günde bir kez gelir. 4 projelik günde (5 müteahhit) hiç tekrar yok;
            // 6 projelik kabadayı gününde (6 müteahhit) tekrar sadece nadir, çözümsüz tür karışımında.
            var clients = TarikClients();
            var byId = new Dictionary<string, ClientSO>();
            foreach (var c in clients) byId[c.Id] = c;
            _config.AllowBribes = true;
            _config.AllowFakeDocuments = true;

            _config.ProjectsPerDay = 4; _config.NonCompliantRatio = 0.5f; _config.AllowThugs = false;
            for (int seed = 0; seed < 300; seed++)
            {
                var seen = new HashSet<string>();
                foreach (var p in ProjectGenerator.Generate(3, _config, _rules, _balance, null, clients, new System.Random(seed)))
                    Assert.IsTrue(seen.Add(p.ClientId), $"seed {seed}: {p.ClientId} aynı gün iki kez geldi");
            }

            _config.ProjectsPerDay = 6; _config.NonCompliantRatio = 0.6f; _config.AllowThugs = true;
            int daysWithRepeat = 0;
            for (int seed = 0; seed < 300; seed++)
            {
                var seen = new HashSet<string>();
                bool repeat = false;
                foreach (var p in ProjectGenerator.Generate(8, _config, _rules, _balance, null, clients, new System.Random(seed)))
                {
                    if (!seen.Add(p.ClientId)) repeat = true;
                    var c = byId[p.ClientId];
                    if (c.IsHonest) Assert.IsFalse(p.OffersBribe || p.Kind == ProjectKind.Fake || p.Kind == ProjectKind.BribeFake,
                        "Dürüst usta rüşvet / sahte belge getirmez");
                    if (p.OffersBribe) Assert.IsTrue(c.OffersBribes, "Zarf sadece rüşvetçiden");
                }
                if (repeat) daysWithRepeat++;
            }
            Assert.Less(daysWithRepeat, 300 / 10, $"6 projelik günlerin %10'undan azında tekrar olmalı ({daysWithRepeat}/300)");

            foreach (var c in clients) UnityEngine.Object.DestroyImmediate(c);
        }

        [Test]
        public void BuildingNames_DoNotRepeatWithinAGame()
        {
            var names = ScriptableObject.CreateInstance<NamePoolSO>();
            names.BuildingNames = new List<string> { "Papatya", "Lale", "Menekşe", "Yasemin", "Çınar", "Ihlamur",
                "Martı", "Güneş", "Huzur", "Umut", "Bahar", "Yıldız" }; // Data/NamePool ile aynı 12 ad
            _config.ProjectsPerDay = 6;
            var used = new HashSet<string>();
            int total = 0;
            for (int day = 1; day <= 10; day++)
            {
                foreach (var p in ProjectGenerator.Generate(day, _config, _rules, _balance, names, null, new System.Random(day), used))
                    total++;
            }
            Assert.AreEqual(total, used.Count, "Bir oyunda aynı bina adı iki kez gelmemeli");
            UnityEngine.Object.DestroyImmediate(names);
        }

        [Test]
        public void BadProjects_IncludeSloppyEveryDay_AndFakesWhenAllowed()
        {
            _config.ProjectsPerDay = 6;
            _config.NonCompliantRatio = 0.5f;
            var seen = new HashSet<ProjectKind>();

            // Sadece konut açık gün: uygunsuzların hepsi dikkatsiz (rüşvet / sahte belge yok)
            for (int seed = 0; seed < 50; seed++)
                foreach (var p in ProjectGenerator.Generate(1, _config, _rules, _balance, null, null, new System.Random(seed)))
                    if (!p.IsCompliant) Assert.AreEqual(ProjectKind.Sloppy, p.Kind);

            _config.AllowBribes = true;
            _config.AllowFakeDocuments = true;
            for (int seed = 0; seed < 200; seed++)
                foreach (var p in ProjectGenerator.Generate(3, _config, _rules, _balance, null, null, new System.Random(seed)))
                {
                    seen.Add(p.Kind);
                    if (p.Kind == ProjectKind.Sloppy)
                    {
                        Assert.IsFalse(p.OffersBribe);
                        Assert.AreEqual(0, p.Documents[1].Clues.Count, "Dikkatsiz projede belge ipucu olmaz");
                        Assert.IsTrue(p.Params.Exists(x => !x.ReportedPasses), "Dikkatsiz projenin eksik değeri belgede görünür");
                    }
                    if (p.Kind == ProjectKind.Fake || p.Kind == ProjectKind.BribeFake)
                        Assert.IsTrue(p.Documents[1].Clues.Count > 0 || p.Params.Exists(x => x.LabDiffers),
                            "Sahte belgede künye ipucu ya da form ↔ lab değer farkı olmalı");
                }
            foreach (var k in new[] { ProjectKind.Sloppy, ProjectKind.Bribe, ProjectKind.Fake, ProjectKind.BribeFake })
                Assert.IsTrue(seen.Contains(k), $"{k} türü hiç üretilmedi");
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

        // Onur (YOL_HARITASI_ONUR 2): ödeme ekranının önizlemesi gerçek ödemeyle her durumda aynı çıkmalı.
        [TestCase(5000, 0, 0, true, TestName = "Preview_YeterliKasa")]
        [TestCase(500, 0, 0, true, TestName = "Preview_YetersizKasa")]
        [TestCase(2000, 0, 2, true, TestName = "Preview_CamTamiri")]
        [TestCase(1000, 2000, 0, true, TestName = "Preview_BorcKismenOdenir")]
        [TestCase(1000, 2000, 0, false, TestName = "Preview_BorcOdemesiSecilmedi")]
        public void ExpensePreview_MatchesApply(int money, int debt, int windows, bool payDebt)
        {
            var rent = Expense("rent", 600, true);
            var loan = Expense("loan", 300, true);
            var state = new GameState { Day = 3, Money = money, Debt = debt, WindowRepairsToday = windows };
            var r = ExpenseCalculator.Build(state, new[] { rent, loan }, _balance);
            foreach (var l in r.Lines) if (l.ExpenseId == ExpenseCalculator.DebtPaymentId) l.Paid = payDebt;

            var preview = ExpenseCalculator.Preview(state, r);
            Assert.AreEqual(money, state.Money, "Önizleme kasaya dokunmamalı");
            Assert.AreEqual(debt, state.Debt, "Önizleme borca dokunmamalı");
            Assert.IsFalse(r.Applied, "Önizleme pending'i uygulanmış saymamalı");

            ExpenseCalculator.Apply(state, r);
            Assert.AreEqual(state.Money, preview.MoneyAfter);
            Assert.AreEqual(state.Debt, preview.DebtAfter);
            Assert.AreEqual(r.PaidTotal, preview.PaidTotal);
            Assert.AreEqual(r.AddedToDebt, preview.AddedToDebt);
            Assert.AreEqual(r.DebtPaid, preview.DebtPaid);
            Assert.GreaterOrEqual(preview.MoneyAfter, 0);
            for (int i = 0; i < r.Lines.Count; i++) Assert.AreEqual(r.Lines[i].Paid, preview.Lines[i].Paid, r.Lines[i].ExpenseId);

            UnityEngine.Object.DestroyImmediate(rent);
            UnityEngine.Object.DestroyImmediate(loan);
        }

        // Onur (YOL_HARITASI_ONUR 3): kayıt sonucu ekrana gelir; tekrar deneme günü ikinci kez artırmaz.
        [Test]
        public void Save_ResultReported_RetryDoesNotAdvanceDay()
        {
            string path = SaveSystem.FilePath;
            string backup = System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null;
            try
            {
                var ctx = new GameContext { Balance = _balance, Rules = _rules };
                var session = new GameSession(ctx, new[] { _config }, new ExpenseSO[0]);
                session.NewGame(5);
                Assert.IsTrue(session.AdvanceDayAndSave());
                Assert.AreEqual(2, ctx.State.Day);
                Assert.IsTrue(ctx.LastSave.Ok);
                Assert.AreEqual(2, ctx.LastSave.Day);

                Assert.IsTrue(ctx.RetrySave());
                Assert.IsTrue(ctx.RetrySave());
                Assert.AreEqual(2, ctx.State.Day, "Tekrar deneme günü artırmamalı");
                Assert.AreEqual(2, SaveSystem.Load().Day);
            }
            finally
            {
                if (backup != null) System.IO.File.WriteAllText(path, backup);
                else SaveSystem.Delete();
            }
        }
    }
}
