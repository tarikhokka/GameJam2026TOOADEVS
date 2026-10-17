using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CivilJam.Core.Tests
{
    /// <summary>Uyuşmazlık diyaloğu + rüşvet uyuşmazlıktan sonra (memory.md "💬").</summary>
    public class InspectionDialogueTests
    {
        BalanceSO _balance;
        RuleSetSO _rules;
        DayConfigSO _config;
        ClientSO _briber;
        DialogueSO _dialogue;
        GameContext _ctx;
        GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _balance = ScriptableObject.CreateInstance<BalanceSO>();
            _rules = ScriptableObject.CreateInstance<RuleSetSO>();
            _rules.Rules = new List<ParamRule>
            {
                new ParamRule { BuildingType = BuildingType.Residential, Param = ParamType.RebarRatio, DisplayName = "Donatı", Required = 1f, FailRatioRange = new Vector2(0.6f, 0.9f) },
                new ParamRule { BuildingType = BuildingType.Residential, Param = ParamType.SoilQuality, DisplayName = "Zemin", Required = 150f, FailRatioRange = new Vector2(0.6f, 0.9f) },
            };
            _config = ScriptableObject.CreateInstance<DayConfigSO>();
            _config.ProjectsPerDay = 6;
            _config.NonCompliantRatio = 1f;
            _config.AllowBribes = true;

            _briber = ScriptableObject.CreateInstance<ClientSO>();
            _briber.Id = "nuri"; _briber.BribeRange = new Vector2Int(1000, 1000);
            _briber.BribeLines = new List<string> { "Göz nuru olsun hocam." };
            _briber.MismatchLines = new List<string> { "Ufak bir eksik hocam." };
            _briber.BribeCloseLines = new List<string> { "Aramızda kalsın." };
            _briber.RejectLines = new List<string> { "Peki." };
            _briber.BribeRefusedLines = new List<string> { "Zarfımı geri mi çeviriyorsunuz?" };

            _dialogue = ScriptableObject.CreateInstance<DialogueSO>();
            _dialogue.BribeReactions = new List<string> { "Bu zarf ne?" };

            _ctx = new GameContext { Balance = _balance, Rules = _rules, Clients = new List<ClientSO> { _briber }, Dialogue = _dialogue };
            _session = new GameSession(_ctx, new[] { _config }, new ExpenseSO[0]);
            // Seed 7: 1. günün 6 kötü projesinin 4'ü rüşvetli (müteahhit ataması günlük yapılınca seed 5'te hepsi dikkatsiz çıkıyordu)
            _session.NewGame(7);
            _session.BeginDay();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in new Object[] { _balance, _rules, _config, _briber, _dialogue }) Object.DestroyImmediate(o);
        }

        ProjectData Bribed()
        {
            var p = _ctx.TodayProjects.Find(x => x.OffersBribe && x.Params.Exists(e => !e.ReportedPasses));
            Assume.That(p, Is.Not.Null, "Değeri eksik görünen rüşvetli proje");
            return p;
        }

        static (InspectTarget, InspectTarget) FailingPair(ProjectData p)
        {
            var e = p.Params.Find(x => !x.ReportedPasses);
            return (InspectTarget.OfParam(InspectDoc.Form, e.Type), InspectTarget.OfParam(InspectDoc.Handbook, e.Type));
        }

        [Test]
        public void BribeHidden_UntilFirstMismatch_ThenOfferDialogue()
        {
            var p = Bribed();
            Assert.IsFalse(p.BribeRevealed, "Zarf başta teklif edilmez");

            var (a, b) = FailingPair(p);
            var o = _ctx.Actions.Inspect(p.Id, a, b, 0);
            Assert.AreEqual(InspectVerdict.Mismatch, o.Result.Verdict);
            Assert.IsTrue(o.BribeOffered);
            Assert.IsTrue(p.BribeRevealed);
            var speakers = o.Lines.ConvertAll(l => l.Speaker);
            CollectionAssert.AreEqual(new[] { DialogueSpeaker.Inspector, DialogueSpeaker.Client, DialogueSpeaker.Client, DialogueSpeaker.Inspector, DialogueSpeaker.Client }, speakers,
                "denetçi uyuşmazlık → müteahhit cevap → teklif → denetçi tepki → kapanış");
            StringAssert.Contains(_rules.DisplayNameOf(a.Param), o.Lines[0].Text, "Denetçi hangi parametrenin yanlış olduğunu söyler");
            Assert.AreEqual("Ufak bir eksik hocam.", o.Lines[1].Text);
            Assert.AreEqual("Göz nuru olsun hocam.", o.Lines[2].Text);
            Assert.AreEqual("Bu zarf ne?", o.Lines[3].Text);
            Assert.AreEqual("Aramızda kalsın.", o.Lines[4].Text);

            // Aynı uyuşmazlık tekrar: sadece kaşe
            var again = _ctx.Actions.Inspect(p.Id, b, a, 0);
            Assert.AreEqual(InspectVerdict.Mismatch, again.Result.Verdict);
            Assert.IsEmpty(again.Lines);
            Assert.IsFalse(again.BribeOffered);
        }

        [Test]
        public void Inspect_IgnoresDeskFloorGroup_UsesBuildingGroup()
        {
            // Poliş K1: masanın gönderdiği grup (seçilmemiş ya da yanlış) sonucu değiştirmez
            var p = Bribed();
            var (a, b) = FailingPair(p);
            Assert.AreEqual(InspectVerdict.Mismatch, _ctx.Actions.Inspect(p.Id, a, b, Inspection.NoFloorGroup).Result.Verdict);
            Assert.AreEqual(InspectVerdict.Mismatch, _ctx.Actions.Inspect(p.Id, a, b, 2).Result.Verdict);
        }

        [Test]
        public void BribeCannotBeTaken_BeforeOffer()
        {
            var p = Bribed();
            int money = _ctx.State.Money;
            _ctx.Actions.Approve(p.Id, true);
            Assert.AreEqual(0, _ctx.State.TotalBribesTaken, "Teklif edilmemiş zarf alınamaz");
            Assert.IsFalse(p.BribeAccepted);
            Assert.AreEqual(money + p.Fee, _ctx.State.Money, "Zarfsız onay: sadece ücret");
        }

        [Test]
        public void BribeTaken_AfterOffer()
        {
            var p = Bribed();
            var (a, b) = FailingPair(p);
            _ctx.Actions.Inspect(p.Id, a, b, 0);
            _ctx.Actions.Approve(p.Id, true);
            Assert.AreEqual(1, _ctx.State.TotalBribesTaken);
            Assert.IsTrue(p.BribeAccepted);
        }

        [Test]
        public void RejectWithoutOffer_IsNotBribeRefusal()
        {
            var p = Bribed();
            _ctx.Actions.Reject(p.Id);
            Assert.AreEqual("Peki.", p.ReactionText, "Teklif edilmemiş zarf geri çevrilmiş sayılmaz");
            Assert.AreEqual(0, ClientReactions.RecordOf(_ctx.State, "nuri").BribesRefused);
        }

        [Test]
        public void MatchAndUnrelated_HaveNoDialogue_AndDoNotOffer()
        {
            var p = Bribed();
            var o1 = _ctx.Actions.Inspect(p.Id, InspectTarget.Of(InspectDoc.Form, InspectField.Parcel), InspectTarget.Of(InspectDoc.Lab, InspectField.Parcel), 0);
            Assert.AreEqual(InspectVerdict.Match, o1.Result.Verdict);
            var o2 = _ctx.Actions.Inspect(p.Id, InspectTarget.Of(InspectDoc.Form, InspectField.Parcel), InspectTarget.Of(InspectDoc.Lab, InspectField.Date), 0);
            Assert.AreEqual(InspectVerdict.Unrelated, o2.Result.Verdict);
            Assert.IsEmpty(o1.Lines); Assert.IsEmpty(o2.Lines);
            Assert.IsFalse(p.BribeRevealed);
        }

        [Test]
        public void SecondDifferentMismatch_TalksAgain_ButNoSecondOffer()
        {
            var p = Bribed();
            var failing = p.Params.FindAll(x => !x.ReportedPasses);
            var (a, b) = FailingPair(p);
            _ctx.Actions.Inspect(p.Id, a, b, 0);
            // Farklı bir uyuşmazlık: lab beyanı ↔ El Kitabı (aynı parametre ama farklı alan çifti)
            var o = _ctx.Actions.Inspect(p.Id, InspectTarget.OfParam(InspectDoc.Lab, a.Param), b, 0);
            Assert.AreEqual(InspectVerdict.Mismatch, o.Result.Verdict);
            Assert.IsFalse(o.BribeOffered, "Teklif bir kez");
            Assert.AreEqual(2, o.Lines.Count, "Sadece denetçi + müteahhit cevabı");
        }

        [Test]
        public void EmptyClientLines_StillGiveInspectorLine()
        {
            _briber.MismatchLines.Clear(); _briber.BribeCloseLines.Clear(); _briber.BribeLines.Clear();
            var p = Bribed();
            var (a, b) = FailingPair(p);
            var o = _ctx.Actions.Inspect(p.Id, a, b, 0);
            Assert.IsTrue(o.BribeOffered);
            CollectionAssert.AreEqual(new[] { DialogueSpeaker.Inspector, DialogueSpeaker.Inspector }, o.Lines.ConvertAll(l => l.Speaker),
                "Boş replikler atlanır; denetçi cümlesi ve tepkisi kalır");
        }

        [Test]
        public void GenericClient_UsesSharedPool_WhenOwnListsEmpty()
        {
            var g = ScriptableObject.CreateInstance<ClientSO>();
            g.Id = "g01"; g.IsGeneric = true;
            _dialogue.GenericGreetingLines = new List<string> { "Merhaba hocam." };
            _dialogue.GenericMismatchLines = new List<string> { "Kontrol ettireyim." };
            Assert.AreEqual("Merhaba hocam.", ClientReactions.LinesOf(_ctx, g, ClientReactions.LineKind.Greeting)[0]);
            Assert.AreEqual("Kontrol ettireyim.", ClientReactions.LinesOf(_ctx, g, ClientReactions.LineKind.Mismatch)[0]);
            g.GreetingLines = new List<string> { "Kendi selamım." };
            Assert.AreEqual("Kendi selamım.", ClientReactions.LinesOf(_ctx, g, ClientReactions.LineKind.Greeting)[0], "Kendi listesi doluysa o");
            Assert.IsEmpty(ClientReactions.LinesOf(_ctx, _briber, ClientReactions.LineKind.Greeting), "Ana karakter havuza düşmez");
            Object.DestroyImmediate(g);
        }
    }
}
