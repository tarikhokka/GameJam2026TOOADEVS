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
            Context.RetrySaveHandler = SaveNow;
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
            // 7 günlük yapı (memory.md "📅"): bütün günlerin ziyaret programı oyun başında, kayıtla saklanır
            if (Balance.UseVisitSchedule && Context.Clients != null && Context.Clients.Count > 0)
                Context.State.Schedule = VisitPlanner.Plan(Balance.TotalDays, ConfigForDay, Context.Clients, Balance,
                    new Random(unchecked(seed * 7919 + 4243)));
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

            // Dünün son müteahhidi bugün ilk gelmesin (yüklemeden sonra liste boş: kısıt yok).
            var yesterday = Context.TodayProjects;
            string lastClient = yesterday != null && yesterday.Count > 0 ? yesterday[yesterday.Count - 1].ClientId : null;
            var planned = State.Schedule != null ? State.Schedule.Find(s => s.Day == State.Day) : null;
            if (Context.DayConfig == null || Context.Rules == null)
                Context.TodayProjects = new List<ProjectData>();
            else if (planned != null)
                Context.TodayProjects = ProjectGenerator.GenerateFromSchedule(State.Day, planned, Context.DayConfig, Context.Rules,
                    Balance, Context.Names, Context.Clients, RngFor(1), State.UsedBuildingNames);
            else
                Context.TodayProjects = ProjectGenerator.Generate(State.Day, Context.DayConfig, Context.Rules, Balance,
                    Context.Names, Context.Clients, RngFor(1), State.UsedBuildingNames, lastClient);
            ClientReactions.PrepareArrivals(Context, RngFor);
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
        /// Son kuralları (karar 2026-10-17). Günlük ödemeden sonra çağrılır; öncelik en ağırdan hafife:
        ///  1. Hapis zarı tuttu                         → Bad
        ///  2. Borç DebtLimit'i aştı                    → MiddleClosed (ofis kapandı; her gün kontrol edilir)
        ///  3. Son gün bitti, en az bir bina yıkıldı     → MiddleCollapsed
        ///  4. Son gün bitti, hiç bina yıkılmadı         → Good
        /// </summary>
        public EndingType EvaluateEnding()
        {
            if (State.Jailed) State.Ending = EndingType.Bad;
            else if (State.Debt > Balance.DebtLimit) State.Ending = EndingType.MiddleClosed;
            else if (State.Day >= Balance.TotalDays)
                State.Ending = State.TotalCollapses > 0 ? EndingType.MiddleCollapsed : EndingType.Good;
            return State.Ending;
        }

        /// <summary>Gün sonu: günü BİR KEZ ilerlet ve kaydet (yükleme yeni günün DayIntro'sunu açar). Sonuç Context.LastSave'de.
        /// Kayıt başarısızsa tekrar deneme Context.RetrySave() ile yapılır; o günü tekrar artırmaz.</summary>
        public bool AdvanceDayAndSave()
        {
            State.Day++;
            return SaveNow();
        }

        /// <summary>Mevcut state'i kaydeder (günü değiştirmez).</summary>
        public bool SaveNow()
        {
            bool ok = SaveSystem.Save(State, out string error);
            Context.LastSave = new SaveResult { Ok = ok, Day = State.Day, Error = error };
            return ok;
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

            // Zarf sadece teklif edildiyse alınabilir (ilk uyuşmazlıktan sonra); yoksa zarfsız onay.
            if (acceptBribe && p.OffersBribe && p.BribeRevealed)
            {
                p.BribeAccepted = true;
                State.Money += p.BribeAmount;
                State.TotalBribesTaken++;
            }
            ClientReactions.OnApproved(Context, p, RngFor);
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
            ClientReactions.OnRejected(Context, p, RngFor);
            Context.NotifyStateChanged();
        }

        public InspectOutcome Inspect(string projectId, InspectTarget a, InspectTarget b, int handbookFloorGroup)
        {
            var p = Context.GetTodayProject(projectId);
            // Poliş K1: kat grubu otomatik — masanın gönderdiği grup yok sayılır, binanın kendi grubu kullanılır.
            var outcome = InspectionDialogue.Run(Context, p, a, b, Inspection.FloorGroupOf(Context, p),
                RngFor((projectId ?? "") + ":inspect:" + (p != null ? p.InspectedMismatches.Count : 0)));
            if (outcome.BribeOffered) Context.NotifyStateChanged();
            return outcome;
        }

        /// <summary>
        /// (EEE, 18 Ekim) Jüri menüsü: masada en öne istenen sahnenin projesini koyar (rüşvetçi, hediye getiren dürüst usta
        /// ya da kabadayı). Kabadayıya izin veren en geç günün ayarı kullanılır; 1. günse 2. güne geçilir (öğretici tekrar açılmasın).
        /// Bulursa true. Proje kimliği "jury-" ile başlar, günün diğer projeleriyle çakışmaz.
        /// </summary>
        public bool JuryPrepareDesk(JuryScene scene)
        {
            if (Context.Rules == null || Context.Clients == null) return false;
            if (State.Day < 2) State.Day = 2;
            State.Ending = EndingType.None; // jüri önce bir son açtıysa gün sonunda yine sona düşmesin
            DayConfigSO config = null;
            for (int d = Balance.TotalDays; d >= 1 && config == null; d--)
            {
                var c = ConfigForDay(d);
                if (c != null && c.AllowThugs) config = c;
            }
            config = config ?? ConfigForDay(State.Day);
            if (config == null) return false;
            Context.DayConfig = config;

            var rng = new Random(Environment.TickCount);
            for (int attempt = 0; attempt < 300; attempt++)
            {
                var list = ProjectGenerator.Generate(State.Day, config, Context.Rules, Balance, Context.Names, Context.Clients, rng,
                    new HashSet<string>(State.UsedBuildingNames));
                foreach (var p in list)
                {
                    var client = Context.GetClient(p.ClientId);
                    if (client == null) continue;
                    bool ok;
                    switch (scene)
                    {
                        case JuryScene.Bribe: ok = p.OffersBribe && !client.IsThug; break;
                        case JuryScene.Thug: ok = client.IsThug; break;
                        case JuryScene.Gift: ok = client.IsHonest && client.Gift != null && !string.IsNullOrEmpty(client.Gift.Id); break;
                        default: ok = false; break;
                    }
                    if (!ok) continue;

                    p.Id = "jury-" + attempt + "-" + rng.Next(100000);
                    p.GreetingText = rng.Pick(ClientReactions.LinesOf(Context, client, ClientReactions.LineKind.Greeting), "");
                    if (scene == JuryScene.Gift)
                    {
                        State.OfficeGifts.Remove(client.Gift.Id); // masada yeniden sunulabilsin
                        p.GiftId = client.Gift.Id;              // onaylanınca kesin gelir (normalde GiftChance)
                    }
                    Context.TodayProjects.RemoveAll(x => x.Id != null && x.Id.StartsWith("jury-") && x.Verdict == ProjectVerdict.Pending);
                    Context.TodayProjects.Insert(0, p);
                    Context.NotifyStateChanged();
                    return true;
                }
            }
            return false;
        }

        public bool AcceptGift(string projectId)
        {
            var p = Context.GetTodayProject(projectId);
            if (p == null || string.IsNullOrEmpty(p.GiftId) || State.OfficeGifts.Contains(p.GiftId)) return false;
            State.OfficeGifts.Add(p.GiftId);
            Context.NotifyStateChanged();
            return true;
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

        /// <summary>Anahtara bağlı tekrarlanabilir rastgelelik (proje Id'si gibi). string.GetHashCode platforma göre değişebildiği için kendi hash'imiz.</summary>
        Random RngFor(string key)
        {
            int h = 17;
            foreach (char c in key) h = unchecked(h * 31 + c);
            return new Random(unchecked(State.Seed * 7919 + h));
        }
    }
}
