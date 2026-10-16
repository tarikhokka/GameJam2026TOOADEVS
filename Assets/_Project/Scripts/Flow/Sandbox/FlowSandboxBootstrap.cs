using System.Collections.Generic;
using CivilJam.Core;
using UnityEngine;

namespace CivilJam.Flow
{
    /// <summary>
    /// Sandbox_Flow için: Flow ekranlarını sahte GameContext ile dener (Game.unity'ye dokunmadan).
    /// Sol üstteki IMGUI düğmeleri ekranlar arasında geçiş yapar. Gerçek akış: GameManager (Core).
    /// </summary>
    public class FlowSandboxBootstrap : MonoBehaviour
    {
        [SerializeField] ResultScreen result;
        [SerializeField] DaySummaryScreen daySummary;
        [SerializeField] DailyExpensesScreen dailyExpenses;
        [SerializeField] EndingScreen ending;

        GameContext _ctx;
        FlowScreen _current;

        void Start()
        {
            _ctx = CreateContext();
            foreach (var s in new FlowScreen[] { result, daySummary, dailyExpenses, ending })
            {
                if (s == null) continue;
                s.Hide();
                s.Completed += () => Debug.Log($"[FlowSandbox] {s.Phase} Completed");
            }
            Open(result);
        }

        void Open(FlowScreen screen)
        {
            if (screen == null) return;
            if (_current != null) _current.Hide();
            _current = screen;
            _current.Show(_ctx);
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Gazete")) Open(result);
            if (GUILayout.Button("Gün özeti")) Open(daySummary);
            if (GUILayout.Button("Günlük ödeme")) Open(dailyExpenses);
            if (GUILayout.Button("Son: İyi")) { _ctx.State.Ending = EndingType.Good; Open(ending); }
            if (GUILayout.Button("Son: Kötü")) { _ctx.State.Ending = EndingType.Bad; Open(ending); }
            if (GUILayout.Button("Son: Ofis kapandı")) { _ctx.State.Ending = EndingType.MiddleClosed; Open(ending); }
            GUILayout.EndHorizontal();
        }

        // ---------- sahte veri ----------

        static GameContext CreateContext()
        {
            var headlines = ScriptableObject.CreateInstance<HeadlineSO>();
            headlines.NoCollapse.Add("Deprem korkuttu, binalar ayakta kaldı");
            headlines.SingleCollapse.Add("{building} yıkıldı: {count} can kaybı");
            headlines.MultiCollapse.Add("Depremde {count} bina yıkıldı");
            headlines.BribedCollapse.Add("Yıkılan binanın onayında usulsüzlük şüphesi");

            var state = new GameState
            {
                Seed = 7, Day = 4, Money = 6200, Debt = 1500, TotalApproved = 6, TotalCollapses = 2,
                TotalBribesTaken = 1, ApprovedToday = 2
            };

            var quake = new QuakeResult { Day = 4, Occurred = true, Jailed = false };
            quake.Outcomes.Add(new BuildingOutcome
            {
                BuildingName = "Gül Apartmanı", Address = "Çınar Mah. 12. Sk. No:4", Collapsed = true, WasBribed = true,
                Residents = 18, Deaths = 3, Injured = 5,
                VictimNames = new List<string> { "Ayşe Demir", "Kerem Aksoy", "Ela Yıldız" },
                ResponsibilityText = "Bu binanın onayında sizin imzanız vardı.",
                FailedParams = new List<ParamEntry>
                {
                    new ParamEntry { Type = ParamType.ConcreteStrength, Required = 25, Reported = 25, Actual = 14 },
                    new ParamEntry { Type = ParamType.SoilQuality, Required = 1, Reported = 1, Actual = 0.5f },
                }
            });
            quake.Outcomes.Add(new BuildingOutcome
            {
                BuildingName = "Lale Sitesi A Blok", Address = "Akasya Mah. 3. Sk. No:9", Collapsed = false
            });
            quake.Outcomes.Add(new BuildingOutcome
            {
                BuildingName = "Zeytin Apartmanı", Address = "Defne Mah. 7. Sk. No:2", Collapsed = false
            });

            var expenses = new DailyExpenseResult { Day = 4, MoneyBefore = 6200, DebtBefore = 1500 };
            expenses.Lines.Add(new ExpenseLine { ExpenseId = "rent", DisplayName = "Ofis kirası", Amount = 800, Mandatory = true, Paid = true });
            expenses.Lines.Add(new ExpenseLine { ExpenseId = "loan", DisplayName = "Kredi taksiti", Amount = 1200, Mandatory = true, Paid = true });
            expenses.Lines.Add(new ExpenseLine { ExpenseId = "insurance", DisplayName = "Sigorta", Amount = 500, Mandatory = false, Paid = true });

            var ctx = new GameContext
            {
                State = state,
                Headlines = headlines,
                LastQuake = quake,
                PendingExpenses = expenses,
                Endings = new List<EndingSO>(),
                Clients = new List<ClientSO>(),
            };
            ctx.Actions = new FakeFlowActions(ctx);
            return ctx;
        }

        sealed class FakeFlowActions : IGameActions
        {
            readonly GameContext _ctx;
            public FakeFlowActions(GameContext ctx) { _ctx = ctx; }
            public bool BuyExtraTest(string projectId) => false;
            public void Approve(string projectId, bool acceptBribe) { }
            public void Reject(string projectId) { }

            public void SetExpensePaid(string expenseId, bool paid)
            {
                var line = _ctx.PendingExpenses.Lines.Find(l => l.ExpenseId == expenseId);
                if (line != null) line.Paid = paid;
                Debug.Log($"[FlowSandbox] SetExpensePaid({expenseId}, {paid})");
            }
        }
    }
}
