using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Ekranlara Show() ile verilen her şey. Ekranlar buradan OKUR; değişiklik sadece Actions üzerinden yapılır.
    /// </summary>
    public class GameContext
    {
        public GameState State;
        public BalanceSO Balance;
        public RuleSetSO Rules;
        public DayConfigSO DayConfig;   // Bugün geçerli içerik ayarı
        public HeadlineSO Headlines;
        public IReadOnlyList<ClientSO> Clients;
        public IReadOnlyList<EndingSO> Endings;
        public IGameActions Actions;

        public GamePhase Phase;

        /// <summary>Bugün masaya gelen projeler.</summary>
        public List<ProjectData> TodayProjects = new List<ProjectData>();

        /// <summary>Son gün sonu afet kontrolü (afet olmadıysa Occurred = false).</summary>
        public QuakeResult LastQuake;

        /// <summary>Sadece ödeme günü dolu; WeeklyExpenses ekranı bunu gösterir.</summary>
        public WeeklyExpenseResult PendingWeekly;

        public bool IsPaymentDay => Balance != null && State.Day % Balance.DaysPerWeek == 0;
        public int Week => Balance == null ? 1 : (State.Day - 1) / Balance.DaysPerWeek + 1;

        /// <summary>Para/borç/risk değiştiğinde tetiklenir (HUD yenilemesi için).</summary>
        public event Action StateChanged;
        public void NotifyStateChanged() => StateChanged?.Invoke();

        public ClientSO GetClient(string id)
        {
            if (Clients == null) return null;
            foreach (var c in Clients) if (c.Id == id) return c;
            return null;
        }

        public ProjectData GetTodayProject(string id) => TodayProjects.Find(p => p.Id == id);
    }
}
