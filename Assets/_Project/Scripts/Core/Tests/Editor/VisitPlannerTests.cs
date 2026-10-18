using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CivilJam.Core.Tests
{
    /// <summary>7 günlük yapı (memory.md "📅"): gerçek Data asset'leri + geçici 10 kişiliksiz karakter.</summary>
    public class VisitPlannerTests
    {
        BalanceSO _balance;
        List<DayConfigSO> _days;
        List<ClientSO> _mains;
        List<ClientSO> _generics;

        [SetUp]
        public void SetUp()
        {
            _balance = AssetDatabase.LoadAssetAtPath<BalanceSO>("Assets/_Project/Data/Balance.asset");
            _days = new List<DayConfigSO>();
            foreach (var guid in AssetDatabase.FindAssets("t:DayConfigSO", new[] { "Assets/_Project/Data/Days" }))
                _days.Add(AssetDatabase.LoadAssetAtPath<DayConfigSO>(AssetDatabase.GUIDToAssetPath(guid)));
            _mains = new List<ClientSO>();
            foreach (var guid in AssetDatabase.FindAssets("t:ClientSO", new[] { "Assets/_Project/Data/Clients" }))
            {
                var c = AssetDatabase.LoadAssetAtPath<ClientSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (!c.IsGeneric) _mains.Add(c);
            }
            _generics = new List<ClientSO>();
            for (int i = 1; i <= 10; i++)
            {
                var g = ScriptableObject.CreateInstance<ClientSO>();
                g.Id = $"test_g{i:00}"; g.IsGeneric = true;
                _generics.Add(g);
            }
            Assume.That(_balance, Is.Not.Null);
            Assume.That(_mains.Count, Is.EqualTo(6));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var g in _generics) Object.DestroyImmediate(g);
        }

        DayConfigSO ConfigFor(int day)
        {
            DayConfigSO best = null;
            foreach (var c in _days) if (c.FromDay <= day && (best == null || c.FromDay > best.FromDay)) best = c;
            return best;
        }

        List<ClientSO> All(bool withGenerics)
        {
            var all = new List<ClientSO>(_mains);
            if (withGenerics) all.AddRange(_generics);
            return all;
        }

        [Test]
        public void SevenDays_3_3_4_4_4_5_5()
        {
            Assert.AreEqual(7, _balance.TotalDays);
            Assert.IsTrue(_balance.UseVisitSchedule);
            var counts = new List<int>();
            for (int d = 1; d <= 7; d++) counts.Add(ConfigFor(d).ProjectsPerDay);
            CollectionAssert.AreEqual(new[] { 3, 3, 4, 4, 4, 5, 5 }, counts);
            Assert.IsFalse(ConfigFor(1).AllowBribes); Assert.IsTrue(ConfigFor(2).AllowBribes);
            Assert.IsFalse(ConfigFor(2).AllowFakeDocuments); Assert.IsTrue(ConfigFor(3).AllowFakeDocuments);
            Assert.IsFalse(ConfigFor(3).AllowThugs); Assert.IsTrue(ConfigFor(4).AllowThugs);
        }

        [Test]
        public void Plan_FollowsAllRules_ForManySeeds()
        {
            var byId = new Dictionary<string, ClientSO>();
            foreach (var c in All(true)) byId[c.Id] = c;

            for (int seed = 0; seed < 300; seed++)
            {
                var plan = VisitPlanner.Plan(7, ConfigFor, All(true), _balance, new System.Random(seed));
                Assert.AreEqual(7, plan.Count);
                var visits = new Dictionary<string, int>();
                int total = 0, bad = 0;
                string last = null;
                foreach (var day in plan)
                {
                    var cfg = ConfigFor(day.Day);
                    Assert.AreEqual(cfg.ProjectsPerDay, day.Slots.Count, $"seed {seed} gün {day.Day} sayı");
                    var today = new HashSet<string>();
                    int dayBad = 0;
                    for (int i = 0; i < day.Slots.Count; i++)
                    {
                        var s = day.Slots[i];
                        var c = byId[s.ClientId];
                        Assert.IsTrue(today.Add(s.ClientId), $"seed {seed} gün {day.Day}: aynı müteahhit iki kez");
                        if (i == 0) Assert.AreNotEqual(last, s.ClientId, $"seed {seed} gün {day.Day}: arka arkaya");
                        visits[s.ClientId] = visits.TryGetValue(s.ClientId, out var v) ? v + 1 : 1;
                        if (c.IsThug) Assert.IsTrue(cfg.AllowThugs || day.Day == 2, $"seed {seed}: kabadayı {day.Day}. günde (2. gün jüri vitrini)");

                        bool isBad = s.Kind != ProjectKind.Compliant;
                        if (isBad) { bad++; dayBad++; }
                        bool bribeKind = s.Kind == ProjectKind.Bribe || s.Kind == ProjectKind.BribeFake;
                        bool fakeKind = s.Kind == ProjectKind.Fake || s.Kind == ProjectKind.BribeFake;
                        if (bribeKind) Assert.IsTrue(cfg.AllowBribes && (c.OffersBribes || c.IsGeneric), $"seed {seed}: zarf kuralı");
                        if (fakeKind) Assert.IsTrue(cfg.AllowFakeDocuments, $"seed {seed}: sahte belge kuralı");
                        if (c.IsHonest) Assert.IsFalse(bribeKind || fakeKind, "Dürüst usta zarf / sahte belge getirmez");
                        total++;
                    }
                    if (day.Day == 1) Assert.LessOrEqual(dayBad, 1, "1. gün en çok 1 uygunsuz");
                    last = day.Slots[day.Slots.Count - 1].ClientId;
                }
                Assert.AreEqual(28, total);
                Assert.AreEqual(14, bad, $"seed {seed}: tam 14 uygunsuz");
                foreach (var m in _mains) Assert.AreEqual(3, visits[m.Id], $"seed {seed}: {m.Id} 3 kez");
                foreach (var g in _generics) Assert.AreEqual(1, visits[g.Id], $"seed {seed}: {g.Id} bir kez");
            }
        }

        [Test]
        public void RealCast_MainsThreeTimesOnDifferentDays_GenericsOnce()
        {
            // Poliş madde 7: gerçek 16 kişilik kadro (Data/Clients) — 6 ana müteahhit 3'er kez ve 3 ayrı günde,
            // 10 kişiliksiz karakter 1'er kez, hiçbiri aynı gün iki kez gelmez.
            var cast = new List<ClientSO>();
            foreach (var guid in AssetDatabase.FindAssets("t:ClientSO", new[] { "Assets/_Project/Data/Clients" }))
                cast.Add(AssetDatabase.LoadAssetAtPath<ClientSO>(AssetDatabase.GUIDToAssetPath(guid)));
            Assert.AreEqual(16, cast.Count);
            Assert.AreEqual(10, cast.FindAll(c => c.IsGeneric).Count);

            for (int seed = 0; seed < 500; seed++)
            {
                var plan = VisitPlanner.Plan(7, ConfigFor, cast, _balance, new System.Random(seed));
                var days = new Dictionary<string, HashSet<int>>();
                var visits = new Dictionary<string, int>();
                foreach (var day in plan)
                    foreach (var s in day.Slots)
                    {
                        if (!days.TryGetValue(s.ClientId, out var set)) days[s.ClientId] = set = new HashSet<int>();
                        Assert.IsTrue(set.Add(day.Day), $"seed {seed} gün {day.Day}: {s.ClientId} aynı gün iki kez");
                        visits[s.ClientId] = visits.TryGetValue(s.ClientId, out var v) ? v + 1 : 1;
                    }
                foreach (var c in cast)
                    Assert.AreEqual(c.IsGeneric ? 1 : 3, visits.TryGetValue(c.Id, out var n) ? n : 0, $"seed {seed}: {c.Id}");
            }
        }

        List<ClientSO> RealCast()
        {
            var cast = new List<ClientSO>();
            foreach (var guid in AssetDatabase.FindAssets("t:ClientSO", new[] { "Assets/_Project/Data/Clients" }))
                cast.Add(AssetDatabase.LoadAssetAtPath<ClientSO>(AssetDatabase.GUIDToAssetPath(guid)));
            return cast;
        }

        [Test]
        public void Showcase_FirstTwoDays_GiftBribeThug_RatesUnchanged()
        {
            // Jüri vitrini (Ahmet ~03:30): 1. gün dürüst usta uygun + hediye kesin; 2. gün rüşvetçi zarflı, kabadayı uygunsuz.
            Assume.That(_balance.ShowcaseEarlyDays, Is.True);
            var cast = RealCast();
            for (int seed = 0; seed < 500; seed++)
            {
                var plan = VisitPlanner.Plan(7, ConfigFor, cast, _balance, new System.Random(seed));
                ClientSO C(string id) => cast.Find(x => x.Id == id);
                int gifts = 0, bad = 0, dayOneBad = 0;
                bool honestGift = false, bribeDay2 = false, thugDay2 = false;
                var visits = new Dictionary<string, int>();
                foreach (var day in plan)
                    foreach (var sl in day.Slots)
                    {
                        var c = C(sl.ClientId);
                        visits[sl.ClientId] = visits.TryGetValue(sl.ClientId, out var v) ? v + 1 : 1;
                        bool isBad = sl.Kind != ProjectKind.Compliant;
                        if (isBad) { bad++; if (day.Day == 1) dayOneBad++; }
                        if (sl.Gift)
                        {
                            gifts++;
                            Assert.AreEqual(1, day.Day, $"seed {seed}: hediye 1. gün");
                            Assert.IsTrue(c.IsHonest && sl.Kind == ProjectKind.Compliant, $"seed {seed}: hediyeyi dürüst usta uygun projeyle getirir");
                            honestGift = true;
                        }
                        if (day.Day == 2 && c.OffersBribes && !c.IsThug && sl.Kind == ProjectKind.Bribe) bribeDay2 = true;
                        if (day.Day == 2 && c.IsThug) { thugDay2 = true; Assert.IsTrue(isBad, $"seed {seed}: kabadayı 2. gün uygunsuz dosyayla"); }
                        if (c.IsThug && day.Day != 2) Assert.IsTrue(ConfigFor(day.Day).AllowThugs, $"seed {seed}: kabadayının diğer ziyaretleri 4-7. gün");
                    }
                Assert.AreEqual(1, gifts, $"seed {seed}: tek vitrin hediyesi");
                Assert.IsTrue(honestGift && bribeDay2 && thugDay2, $"seed {seed}: vitrin eksik");
                Assert.AreEqual(14, bad, $"seed {seed}: 14 uygunsuz");
                Assert.LessOrEqual(dayOneBad, 1, $"seed {seed}: 1. gün en çok 1 uygunsuz");
                foreach (var c in cast) Assert.AreEqual(c.IsGeneric ? 1 : 3, visits[c.Id], $"seed {seed}: {c.Id} ziyaret sayısı");
            }
        }

        [Test]
        public void Showcase_Session_GiftOnDayOne_ThugDamageOnDayTwo()
        {
            var rules = AssetDatabase.LoadAssetAtPath<RuleSetSO>("Assets/_Project/Data/RuleSet.asset");
            var names = AssetDatabase.LoadAssetAtPath<NamePoolSO>("Assets/_Project/Data/NamePool.asset");
            for (int seed = 1; seed <= 20; seed++)
            {
                var ctx = new GameContext { Balance = _balance, Rules = rules, Names = names, Clients = RealCast() };
                var session = new GameSession(ctx, _days, new ExpenseSO[0]);
                session.NewGame(seed);
                session.BeginDay();
                var giftProject = ctx.TodayProjects.Find(p => p.GiftGuaranteed);
                Assert.IsNotNull(giftProject, $"seed {seed}: 1. gün hediye projesi");
                ctx.Actions.Approve(giftProject.Id, false);
                Assert.IsFalse(string.IsNullOrEmpty(giftProject.GiftId), $"seed {seed}: onaylanınca hediye kesin");

                ctx.State.Day = 2;
                session.BeginDay();
                var thug = ctx.TodayProjects.Find(p => ctx.GetClient(p.ClientId).IsThug);
                Assert.IsNotNull(thug, $"seed {seed}: 2. gün kabadayı");
                Assert.IsFalse(thug.IsCompliant, $"seed {seed}: kabadayı dosyası uygunsuz");
                Assert.IsTrue(ctx.TodayProjects.Exists(p => p.OffersBribe && !ctx.GetClient(p.ClientId).IsThug), $"seed {seed}: 2. gün zarflı rüşvetçi");
                int before = ctx.State.WindowRepairsToday;
                ctx.Actions.Reject(thug.Id);
                Assert.AreEqual(before + 1, ctx.State.WindowRepairsToday, $"seed {seed}: 2. gün kabadayı reddi hasar verir");
            }
        }

        [Test]
        public void Showcase_DisasterGuaranteedByDayTwo_ChanceUnchangedBefore()
        {
            var cfg = ConfigFor(1);
            var names = AssetDatabase.LoadAssetAtPath<NamePoolSO>("Assets/_Project/Data/NamePool.asset");
            Assume.That(_balance.GuaranteedDisasterByDay, Is.EqualTo(2));
            // 1. gece: olasılık 0 ise afet yok (zar aynen)
            var s1 = new GameState { Day = 1, DisasterChance = 0f };
            Assert.IsFalse(DisasterSystem.EndOfDay(s1, cfg, _balance, names, new System.Random(1)).Occurred);
            // 2. gece: hiç afet olmadıysa kesin
            var s2 = new GameState { Day = 2, DisasterChance = 0f };
            Assert.IsTrue(DisasterSystem.EndOfDay(s2, cfg, _balance, names, new System.Random(1)).Occurred);
            Assert.AreEqual(_balance.DisasterChanceStart, s2.DisasterChance, 1e-6, "afetten sonra olasılık tabana döner");
            // Daha önce afet olduysa garanti yok
            var s3 = new GameState { Day = 2, DisasterChance = 0f, LastDisasterDay = 1 };
            Assert.IsFalse(DisasterSystem.EndOfDay(s3, cfg, _balance, names, new System.Random(1)).Occurred);
        }

        [Test]
        public void Backup_WithoutGenerics_MainsFillTheRest()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                var plan = VisitPlanner.Plan(7, ConfigFor, All(false), _balance, new System.Random(seed));
                int total = 0, bad = 0;
                foreach (var day in plan)
                {
                    var today = new HashSet<string>();
                    foreach (var s in day.Slots)
                    {
                        Assert.IsTrue(today.Add(s.ClientId), $"seed {seed} gün {day.Day}: aynı müteahhit iki kez");
                        total++;
                        if (s.Kind != ProjectKind.Compliant) bad++;
                    }
                }
                Assert.AreEqual(28, total);
                Assert.AreEqual(14, bad);
            }
        }

        [Test]
        public void Session_UsesSchedule_AndKeepsItInSave()
        {
            var rules = AssetDatabase.LoadAssetAtPath<RuleSetSO>("Assets/_Project/Data/RuleSet.asset");
            var names = AssetDatabase.LoadAssetAtPath<NamePoolSO>("Assets/_Project/Data/NamePool.asset");
            var ctx = new GameContext { Balance = _balance, Rules = rules, Names = names, Clients = All(true) };
            var session = new GameSession(ctx, _days, new ExpenseSO[0]);
            session.NewGame(42);
            Assert.AreEqual(7, ctx.State.Schedule.Count);
            for (int d = 1; d <= 7; d++)
            {
                ctx.State.Day = d;
                session.BeginDay();
                var plan = ctx.State.Schedule[d - 1];
                Assert.AreEqual(plan.Slots.Count, ctx.TodayProjects.Count);
                for (int i = 0; i < plan.Slots.Count; i++)
                {
                    Assert.AreEqual(plan.Slots[i].ClientId, ctx.TodayProjects[i].ClientId);
                    Assert.AreEqual(plan.Slots[i].Kind, ctx.TodayProjects[i].Kind);
                }
            }
            var loaded = JsonUtility.FromJson<GameState>(JsonUtility.ToJson(ctx.State));
            Assert.AreEqual(7, loaded.Schedule.Count);
            Assert.AreEqual(ctx.State.Schedule[3].Slots[0].ClientId, loaded.Schedule[3].Slots[0].ClientId);
        }
    }
}
