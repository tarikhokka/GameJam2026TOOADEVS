using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Saf C# oyun mantığı: gün döngüsünün adımları + IGameActions. MonoBehaviour değildir; GameManager sırayı yönetir.
    /// Rastgelelik seed + gün + amaç ile türetilir → kayıttan yüklenince aynı gün aynı projeler gelir.
    /// </summary>
    public class GameSession : IGameActions
    {
        public GameContext Context { get; }

        readonly IReadOnlyList<DayConfigSO> _dayConfigs;
        readonly IReadOnlyList<ExpenseSO> _expenses;

        GameState State => Context.State;
        BalanceSO Balance => Context.Balance;

        public GameSession(GameContext context, IReadOnlyList<DayConfigSO> dayConfigs, IReadOnlyList<ExpenseSO> expenses)
        {
            Context = context;
            Context.Actions = this;
            _dayConfigs = dayConfigs;
            _expenses = expenses;
        }

        // ---------- Oyun başlangıcı ----------

        public void NewGame(int seed)
        {
            Context.State = new GameState
            {
                Seed = seed,
                Day = 1,
                Money = Balance.StartMoney,
                DisasterChance = Balance.DisasterChanceStart
            };
            Context.NotifyStateChanged();
        }

        public void LoadGame(GameState state)
        {
            Context.State = state;
            Context.NotifyStateChanged();
        }

        // ---------- Gün döngüsü ----------

        /// <summary>DayIntro başında: günün ayarını seç, projeleri üret, gün içi sayaçları sıfırla.</summary>
        public void BeginDay()
        {
            Context.DayConfig = ConfigForDay(State.Day);
            Context.LastQuake = null;
            Context.PendingExpenses = null;
            State.ApprovedToday = 0;
            State.WindowRepairsToday = 0;

            Context.TodayProjects = Context.DayConfig == null || Context.Rules == null
                ? new List<ProjectData>()
                : ProjectGenerator.Generate(State.Day, Context.DayConfig, Context.Rules, Balance,
                    Context.Names, Context.Clients, RngFor(1));
            Context.NotifyStateChanged();
        }

        /// <summary>Desk bitince: karar verilmeyenler reddedilir, onaylananlar inşa edilir.</summary>
        public void FinishDesk()
        {
            foreach (var p in Context.TodayProjects)
            {
                if (p.Verdict == ProjectVerdict.Pending) Reject(p.Id);
                if (p.Verdict == ProjectVerdict.Approved && !State.Buildings.Contains(p))
                    State.Buildings.Add(p);
            }
        }

        /// <summary>Construction sonrası: afet zarı. Sonuç Context.LastQuake'te.</summary>
        public QuakeResult RunDisasterCheck()
        {
            Context.LastQuake = DisasterSystem.EndOfDay(State, Context.DayConfig, Balance, Context.Names, RngFor(2));
            Context.NotifyStateChanged();
            return Context.LastQuake;
        }

        public DailyExpenseResult PrepareExpenses()
        {
            Context.PendingExpenses = ExpenseCalculator.Build(State, _expenses, Balance);
            return Context.PendingExpenses;
        }

        public void FinishExpenses()
        {
            ExpenseCalculator.Apply(State, Context.PendingExpenses);
            Context.NotifyStateChanged();
        }

        /// <summary>
        /// GEÇİCİ son kuralları (öncelik henüz kararlaştırılmadı): Hapis → Kötü; son gün tamam → İyi.
        /// Ofis kapanışı / kota sonları kararlar netleşince eklenecek.
        /// </summary>
        public EndingType EvaluateEnding()
        {
            if (State.Jailed) State.Ending = EndingType.Bad;
            else if (State.Day >= Balance.TotalDays) State.Ending = EndingType.Good;
            return State.Ending;
        }

        /// <summary>Gün sonu: günü ilerlet ve kaydet (yükleme yeni günün DayIntro'sunu açar).</summary>
        public bool AdvanceDayAndSave()
        {
            State.Day++;
            return SaveSystem.Save(State);
        }

        // ---------- IGameActions ----------

        public bool BuyExtraTest(string projectId)
        {
            var p = Context.GetTodayProject(projectId);
            if (p == null || p.ExtraTestBought || p.Verdict != ProjectVerdict.Pending) return false;
            if (State.Money < Balance.ExtraTestCost) return false;

            State.Money -= Balance.ExtraTestCost;
            p.ExtraTestBought = true;
            foreach (var param in p.Params) param.Tested = true;
            Context.NotifyStateChanged();
            return true;
        }

        public void Approve(string projectId, bool acceptBribe)
        {
            var p = Context.GetTodayProject(projectId);
            if (p == null || p.Verdict != ProjectVerdict.Pending) return;

            p.Verdict = ProjectVerdict.Approved;
            p.DayDecided = State.Day;
            State.Money += p.Fee;
            State.ApprovedToday++;
            State.TotalApproved++;

            if (acceptBribe && p.OffersBribe)
            {
                p.BribeAccepted = true;
                State.Money += p.BribeAmount;
                State.TotalBribesTaken++;
            }
            Context.NotifyStateChanged();
        }

        public void Reject(string projectId)
        {
            var p = Context.GetTodayProject(projectId);
            if (p == null || p.Verdict != ProjectVerdict.Pending) return;

            p.Verdict = ProjectVerdict.Rejected;
            p.DayDecided = State.Day;

            var client = Context.GetClient(p.ClientId);
            if (client != null && client.IsThug && Context.DayConfig != null && Context.DayConfig.AllowThugs)
                State.WindowRepairsToday++;
            Context.NotifyStateChanged();
        }

        public void SetExpensePaid(string expenseId, bool paid)
        {
            var exp = Context.PendingExpenses;
            if (exp == null || exp.Applied) return;
            var line = exp.Lines.Find(l => l.ExpenseId == expenseId);
            if (line != null) line.Paid = paid;
        }

        // ---------- Yardımcılar ----------

        DayConfigSO ConfigForDay(int day)
        {
            DayConfigSO best = null;
            if (_dayConfigs == null) return null;
            foreach (var c in _dayConfigs)
                if (c != null && c.FromDay <= day && (best == null || c.FromDay > best.FromDay)) best = c;
            return best;
        }

        Random RngFor(int purpose) => new Random(unchecked(State.Seed * 7919 + State.Day * 104729 + purpose * 31));
    }
}
