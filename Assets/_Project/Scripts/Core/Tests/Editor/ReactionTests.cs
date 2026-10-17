using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CivilJam.Core.Tests
{
    /// <summary>Ç4: müteahhit hafızası, selamlama, tepkiler, ofis hediyesi, zarf artışı.</summary>
    public class ReactionTests
    {
        BalanceSO _balance;
        RuleSetSO _rules;
        DayConfigSO _config;
        ClientSO _honest, _briber;
        GiftSO _clock;
        GameContext _ctx;
        GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _balance = ScriptableObject.CreateInstance<BalanceSO>();
            _rules = ScriptableObject.CreateInstance<RuleSetSO>();
            _rules.Rules = new List<ParamRule>
            {
                new ParamRule { BuildingType = BuildingType.Residential, Param = ParamType.RebarRatio, Required = 1f, FailRatioRange = new Vector2(0.6f, 0.9f) },
                new ParamRule { BuildingType = BuildingType.Residential, Param = ParamType.SoilQuality, Required = 150f, FailRatioRange = new Vector2(0.6f, 0.9f) },
            };
            _config = ScriptableObject.CreateInstance<DayConfigSO>();
            _config.ProjectsPerDay = 6;
            _config.NonCompliantRatio = 0.5f;
            _config.AllowBribes = true;

            _clock = ScriptableObject.CreateInstance<GiftSO>();
            _clock.Id = "clock"; _clock.DisplayName = "Duvar saati";

            _honest = ScriptableObject.CreateInstance<ClientSO>();
            _honest.Id = "rifat"; _honest.IsHonest = true; _honest.BribeRange = Vector2Int.zero;
            _honest.GreetingLines = new List<string> { "Merhaba hocam." };
            _honest.ReturnLines = new List<string> { "Geçen sefer {last} hocam." };
            _honest.ApproveLines = new List<string> { "Sağ olun hocam." };
            _honest.RejectLines = new List<string> { "Düzeltip gelirim." };
            _honest.Gift = _clock; _honest.GiftChance = 1f;

            _briber = ScriptableObject.CreateInstance<ClientSO>();
            _briber.Id = "nuri"; _briber.BribeRange = new Vector2Int(1000, 1000); _briber.BribeRaiseOnReject = 250;
            _briber.GreetingLines = new List<string> { "Hocaam!" };
            _briber.RejectLines = new List<string> { "Peki." };
            _briber.BribeRefusedLines = new List<string> { "Zarfımı geri mi çeviriyorsunuz?" };
            _briber.Gift = _clock; _briber.GiftChance = 1f; // dürüst olmadığı için hediye vermemeli

            _ctx = new GameContext { Balance = _balance, Rules = _rules, Clients = new List<ClientSO> { _honest, _briber } };
            _session = new GameSession(_ctx, new[] { _config }, new ExpenseSO[0]);
            _session.NewGame(11);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in new Object[] { _balance, _rules, _config, _honest, _briber, _clock }) Object.DestroyImmediate(o);
        }

        ProjectData First(string clientId) => _ctx.TodayProjects.Find(p => p.ClientId == clientId);

        [Test]
        public void FirstVisitGreets_ReturnVisitRemembersLastVerdict()
        {
            _session.BeginDay();
            var p = First("rifat");
            Assume.That(p, Is.Not.Null);
            Assert.AreEqual("Merhaba hocam.", p.GreetingText);
            _ctx.Actions.Reject(p.Id);
            Assert.AreEqual("Düzeltip gelirim.", p.ReactionText);

            _ctx.State.Day++;
            _session.BeginDay();
            var again = First("rifat");
            Assume.That(again, Is.Not.Null);
            Assert.AreEqual("Geçen sefer reddetmiştiniz hocam.", again.GreetingText);
        }

        [Test]
        public void Approve_ThanksAndGivesGiftOnce()
        {
            _session.BeginDay();
            var p = First("rifat");
            Assume.That(p, Is.Not.Null);
            _ctx.Actions.Approve(p.Id, false);
            Assert.AreEqual("Sağ olun hocam.", p.ReactionText);
            Assert.AreEqual("clock", p.GiftId, "GiftChance 1 → hediye çıkmalı");
            Assert.IsTrue(_ctx.Actions.AcceptGift(p.Id));
            Assert.IsFalse(_ctx.Actions.AcceptGift(p.Id), "Aynı hediye iki kez alınmaz");
            CollectionAssert.AreEqual(new[] { "clock" }, _ctx.State.OfficeGifts);

            // Sonraki onaylarda aynı hediye bir daha çıkmaz
            for (int d = 0; d < 3; d++)
            {
                _ctx.State.Day++;
                _session.BeginDay();
                foreach (var x in _ctx.TodayProjects.FindAll(x => x.ClientId == "rifat"))
                {
                    _ctx.Actions.Approve(x.Id, false);
                    Assert.IsTrue(string.IsNullOrEmpty(x.GiftId), "Ofiste olan hediye tekrar verilmez");
                }
            }
        }

        [Test]
        public void OnlyHonestClientsGiveGifts()
        {
            for (int d = 0; d < 4; d++)
            {
                _ctx.State.Day = d + 1;
                _session.BeginDay();
                foreach (var p in _ctx.TodayProjects.FindAll(x => x.ClientId == "nuri"))
                {
                    _ctx.Actions.Approve(p.Id, true);
                    Assert.IsTrue(string.IsNullOrEmpty(p.GiftId), "Dürüst olmayan müteahhit hediye vermez");
                }
            }
        }

        [Test]
        public void RefusedBribe_HasOwnReaction_AndNextBribeGrows()
        {
            _session.BeginDay();
            var p = _ctx.TodayProjects.Find(x => x.ClientId == "nuri" && x.OffersBribe && x.Params.Exists(e => !e.ReportedPasses));
            Assume.That(p, Is.Not.Null);
            int first = p.BribeAmount;
            // Zarf ilk uyuşmazlıktan sonra teklif edilir (memory.md "💬"); geri çevirmek için önce teklif edilmeli
            var bad = p.Params.Find(e => !e.ReportedPasses).Type;
            Assert.IsTrue(_ctx.Actions.Inspect(p.Id, InspectTarget.OfParam(InspectDoc.Form, bad), InspectTarget.OfParam(InspectDoc.Handbook, bad), 0).BribeOffered);
            _ctx.Actions.Reject(p.Id);
            Assert.AreEqual("Zarfımı geri mi çeviriyorsunuz?", p.ReactionText);
            Assert.AreEqual(1, ClientReactions.RecordOf(_ctx.State, "nuri").BribesRefused);

            _ctx.State.Day++;
            _session.BeginDay();
            var next = _ctx.TodayProjects.Find(x => x.ClientId == "nuri" && x.OffersBribe);
            Assume.That(next, Is.Not.Null);
            int rejected = ClientReactions.RecordOf(_ctx.State, "nuri").Rejected;
            Assert.AreEqual(1000 + 250 * rejected, next.BribeAmount, "Her retten sonra zarf BribeRaiseOnReject kadar büyür");
        }

        [Test]
        public void EmptyLines_LeaveTextsEmpty_AndHistorySurvivesSave()
        {
            _honest.GreetingLines.Clear(); _honest.ApproveLines.Clear();
            _session.BeginDay();
            var p = First("rifat");
            Assume.That(p, Is.Not.Null);
            Assert.IsTrue(string.IsNullOrEmpty(p.GreetingText));
            _ctx.Actions.Approve(p.Id, false);
            Assert.IsTrue(string.IsNullOrEmpty(p.ReactionText));

            var json = JsonUtility.ToJson(_ctx.State);
            var loaded = JsonUtility.FromJson<GameState>(json);
            Assert.AreEqual(1, ClientReactions.RecordOf(loaded, "rifat").Approved, "Müteahhit geçmişi kayıtla saklanmalı");
        }
    }
}
