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
        public IReadOnlyList<GiftSO> Gifts;
        public DialogueSO Dialogue;     // Denetçinin uyuşmazlık / rüşvet tepkisi cümleleri (boşsa Core varsayılanı)
        public IGameActions Actions;

        public GamePhase Phase;

        /// <summary>Bugün masaya gelen projeler.</summary>
        public List<ProjectData> TodayProjects = new List<ProjectData>();

        /// <summary>Bugünün gün sonu afet kontrolü. Occurred = false ise gazete yok.</summary>
        public QuakeResult LastQuake;

        /// <summary>Bugünün gider kalemleri; DailyExpenses ekranı bunu gösterir.</summary>
        public DailyExpenseResult PendingExpenses;

        /// <summary>Gün sonu kaydının sonucu (Save fazına girerken dolar). Ok = false ise ekran "Kaydedildi" yazmaz.</summary>
        public SaveResult LastSave;

        /// <summary>"Devam Et" istendi ama kayıt okunamadı; oyun yeni başladı. Ekran (DayIntro) oyuncuya söyleyebilir.</summary>
        public bool LoadFailed;
        public string LoadError;

        /// <summary>Kaydı aynı günle yeniden yazmayı dener (günü ilerletmez). Sonuç LastSave'e de yazılır.
        /// Kayıt state'i değiştirmediği için Actions dışında; GameSession bağlar.</summary>
        public Func<bool> RetrySaveHandler;
        public bool RetrySave() => RetrySaveHandler != null && RetrySaveHandler();

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

        public GiftSO GetGift(string id)
        {
            if (Gifts == null || string.IsNullOrEmpty(id)) return null;
            foreach (var g in Gifts) if (g != null && g.Id == id) return g;
            return null;
        }
    }
}

namespace CivilJam.Core
{
    /// <summary>Gün sonu kaydının sonucu. Day = kaydedilen gün (Devam Et bu günün başını açar).</summary>
    public class SaveResult
    {
        public bool Ok;
        public int Day;
        public string Error;   // Ok = false ise teknik hata metni (oyuncuya gösterilmesi şart değil)
    }
}
