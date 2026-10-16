using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CivilJam.Core
{
    /// <summary>
    /// Game sahnesinin tek yöneticisi: faz sırasını yürütür, ilgili IGameScreen'i gösterir.
    /// Oyun mantığı GameSession'dadır. Bir faz için sahnede ekran yoksa IMGUI yedek arayüzü gösterilir (FallbackGui).
    ///
    /// Akış: DayIntro → Desk → Construction → (afet ? Result : DaySummary) → DailyExpenses → (son ? Ending : Save) → DayIntro
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Veri")]
        [SerializeField] BalanceSO balance;
        [SerializeField] RuleSetSO rules;
        [SerializeField] NamePoolSO names;
        [SerializeField] HeadlineSO headlines;
        [SerializeField] List<DayConfigSO> dayConfigs = new List<DayConfigSO>();
        [SerializeField] List<ExpenseSO> expenses = new List<ExpenseSO>();
        [SerializeField] List<ClientSO> clients = new List<ClientSO>();
        [SerializeField] List<EndingSO> endings = new List<EndingSO>();
        [SerializeField] List<GiftSO> gifts = new List<GiftSO>();

        [Header("Başlangıç")]
        [Tooltip("0 = rastgele seed")]
        [SerializeField] int fixedSeed;

        [Header("Debug")]
        [Tooltip("Ekranı olmayan fazlar için IMGUI yedek arayüzü. Sadece Editor'de ve Development Build'de çalışır; " +
                 "gerçek build'de ekranı olmayan faz otomatik geçilir.")]
        [SerializeField] bool fallbackGui = true;

        [Tooltip("Jüri demosu için gizli tuşlar: F9 = son güne atla, F10 = bu gece afet kesin")]
        [SerializeField] bool demoKeys = true;

        bool UseFallback => fallbackGui && (Application.isEditor || Debug.isDebugBuild);

        readonly Dictionary<GamePhase, IGameScreen> _screens = new Dictionary<GamePhase, IGameScreen>();
        GameSession _session;
        IGameScreen _current;
        bool _waiting; // Mevcut faz Completed bekliyor (çift tetiklemeye karşı)

        public GameContext Context => _session?.Context;
        public GameSession Session => _session;

        void Awake()
        {
            var context = new GameContext
            {
                Balance = balance,
                Rules = rules,
                Names = names,
                Headlines = headlines,
                Clients = clients,
                Endings = endings,
                Gifts = gifts
            };
            _session = new GameSession(context, dayConfigs, expenses);
            DiscoverScreens();
        }

        void Start()
        {
            GameState loaded = GameLaunch.ContinueFromSave ? SaveSystem.Load() : null;
            GameLaunch.ContinueFromSave = false;

            if (loaded != null) _session.LoadGame(loaded);
            else _session.NewGame(fixedSeed != 0 ? fixedSeed : System.Environment.TickCount);

            Enter(GamePhase.DayIntro);
        }

        void DiscoverScreens()
        {
            var all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            foreach (var mb in all)
            {
                if (mb is IGameScreen screen)
                {
                    if (_screens.ContainsKey(screen.Phase))
                        Debug.LogWarning($"[GameManager] {screen.Phase} için birden fazla ekran var; ilki kullanılıyor.");
                    else
                        _screens[screen.Phase] = screen;
                    screen.Hide();
                }
            }
        }

        // ---------- Faz geçişleri ----------

        void Enter(GamePhase phase)
        {
            if (_current != null)
            {
                _current.Completed -= OnScreenCompleted;
                _current.Hide();
                _current = null;
            }

            Context.Phase = phase;
            switch (phase)
            {
                case GamePhase.DayIntro: _session.BeginDay(); break;
                case GamePhase.DailyExpenses: _session.PrepareExpenses(); break;
                case GamePhase.Save: _session.AdvanceDayAndSave(); break;
                case GamePhase.Ending: SaveSystem.Delete(); break; // Bitmiş oyun "Devam Et" ile açılmasın
            }

            _waiting = true;
            if (_screens.TryGetValue(phase, out var screen))
            {
                _current = screen;
                _current.Completed += OnScreenCompleted;
                _current.Show(Context);
            }
            else if (!UseFallback)
            {
                Complete(); // Ekran yok, yedek kapalı → fazı atla
            }
        }

        void OnScreenCompleted() => Complete();

        /// <summary>Mevcut fazı bitir ve sıradakine geç. Ekranlar Completed ile, yedek arayüz doğrudan çağırır.</summary>
        public void Complete()
        {
            if (!_waiting) return;
            _waiting = false;

            switch (Context.Phase)
            {
                case GamePhase.DayIntro:
                    Enter(GamePhase.Desk);
                    break;
                case GamePhase.Desk:
                    _session.FinishDesk();
                    Enter(GamePhase.Construction);
                    break;
                case GamePhase.Construction:
                    var quake = _session.RunDisasterCheck();
                    Enter(quake.Occurred ? GamePhase.Result : GamePhase.DaySummary);
                    break;
                case GamePhase.Result:
                case GamePhase.DaySummary:
                    Enter(GamePhase.DailyExpenses);
                    break;
                case GamePhase.DailyExpenses:
                    _session.FinishExpenses();
                    Enter(_session.EvaluateEnding() != EndingType.None ? GamePhase.Ending : GamePhase.Save);
                    break;
                case GamePhase.Save:
                    Enter(GamePhase.DayIntro);
                    break;
                case GamePhase.Ending:
                    // MainMenu'ye dönüş C'nin son ekranında. Kayıt Ending'e girerken silindi.
                    break;
            }
        }

        void Update()
        {
            if (!demoKeys || Context == null || Context.State == null) return;
            var kb = Keyboard.current;
            if (kb == null) return;

            // F9: son güne atla (bu günün sonunda son kuralları değerlendirilir)
            if (kb.f9Key.wasPressedThisFrame && Context.Phase != GamePhase.Ending)
            {
                Context.State.Day = Mathf.Max(Context.State.Day, Context.Balance.TotalDays);
                Context.NotifyStateChanged();
                Debug.Log($"[Demo] Son güne atlandı: {Context.State.Day}");
            }
            // F10: bu gece afet kesin
            if (kb.f10Key.wasPressedThisFrame)
            {
                Context.State.DisasterChance = 1f;
                Context.NotifyStateChanged();
                Debug.Log("[Demo] Bu gece afet kesin");
            }
        }

        void OnGUI()
        {
            if (UseFallback && _current == null && _waiting && Context != null)
                FallbackGui.Draw(this);
        }
    }
}
