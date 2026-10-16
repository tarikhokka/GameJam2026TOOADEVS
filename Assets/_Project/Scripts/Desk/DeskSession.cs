using System;
using System.Collections.Generic;
using System.Linq;

namespace CivilJam.Desk
{
    // Masa ekranının saf mantığı: kararlar, ek test harcaması, rüşvet. UI bilmez.
    public class DeskSession
    {
        public DeskSessionConfig Config { get; }
        public int Money { get; private set; }

        public event Action Changed;

        private readonly Dictionary<string, DeskProject> projects = new Dictionary<string, DeskProject>();
        private readonly Dictionary<string, DeskDecision> decisions = new Dictionary<string, DeskDecision>();
        private int extraTestSpent;
        private int bribeIncome;

        public DeskSession(DeskSessionConfig config)
        {
            Config = config;
            Money = config.StartMoney;
            foreach (var p in config.Projects)
            {
                projects[p.Id] = p;
                decisions[p.Id] = new DeskDecision { ProjectId = p.Id };
            }
        }

        public DeskProject GetProject(string id) => projects[id];
        public DeskDecision GetDecision(string id) => decisions[id];

        public int ApprovedCount => decisions.Values.Count(d => d.Verdict == DeskVerdict.Approved);
        public bool AllDecided => decisions.Values.All(d => d.Verdict != DeskVerdict.Pending);

        public string FirstPendingId()
        {
            var p = Config.Projects.FirstOrDefault(x => decisions[x.Id].Verdict == DeskVerdict.Pending);
            return p?.Id;
        }

        public bool CanBuyExtraTest(string id)
        {
            var d = decisions[id];
            return d.Verdict == DeskVerdict.Pending && !d.ExtraTestBought && Money >= Config.ExtraTestCost;
        }

        public bool BuyExtraTest(string id)
        {
            if (!CanBuyExtraTest(id)) return false;
            Money -= Config.ExtraTestCost;
            extraTestSpent += Config.ExtraTestCost;
            decisions[id].ExtraTestBought = true;
            Changed?.Invoke();
            return true;
        }

        public void Approve(string id, bool takeBribe)
        {
            var d = decisions[id];
            if (d.Verdict != DeskVerdict.Pending) return;

            d.Verdict = DeskVerdict.Approved;
            var p = projects[id];
            if (takeBribe && p.HasBribe)
            {
                d.TookBribe = true;
                Money += p.BribeAmount;
                bribeIncome += p.BribeAmount;
            }
            Changed?.Invoke();
        }

        public void Reject(string id)
        {
            var d = decisions[id];
            if (d.Verdict != DeskVerdict.Pending) return;

            d.Verdict = DeskVerdict.Rejected;
            Changed?.Invoke();
        }

        public DeskResult BuildResult()
        {
            return new DeskResult
            {
                Decisions = Config.Projects.Select(p => decisions[p.Id]).ToList(),
                ApprovedCount = ApprovedCount,
                Quota = Config.Quota,
                ExtraTestSpent = extraTestSpent,
                BribeIncome = bribeIncome,
                EndMoney = Money,
            };
        }
    }
}
