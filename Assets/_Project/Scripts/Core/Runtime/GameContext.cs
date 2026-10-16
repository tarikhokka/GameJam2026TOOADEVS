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
        public NamePoolSO Names;
        public IReadOnlyList<ClientSO> Clients;
        public IReadOnlyList<EndingSO> Endings;
        public IGameActions Actions;

        public GamePhase Phase;

        /// <summary>Bugün masaya gelen projeler.</summary>
        public List<ProjectData> TodayProjects = new List<ProjectData>();

        /// <summary>Bugünün gün sonu afet kontrolü. Occurred = false ise gazete yok.</summary>
        public QuakeResult LastQuake;

        /// <summary>Bugünün gider kalemleri; DailyExpenses ekranı bunu gösterir.</summary>
        public DailyExpenseResult PendingExpenses;

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
