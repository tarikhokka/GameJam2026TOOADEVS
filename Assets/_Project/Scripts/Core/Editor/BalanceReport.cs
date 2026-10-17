using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CivilJam.Core.Editor
{
    /// <summary>
    /// Denge raporu: gerçek Data asset'leriyle farklı oyuncu tiplerini yüzlerce kez oynatır (UI yok, saf GameSession).
    /// Menü: CivilJam > Core > Balance Report. Sonuç Console'a ve Logs/balance_report.txt'ye yazılır.
    /// </summary>
    public static class BalanceReport
    {
        const int GamesPerStrategy = 300;

        delegate void Strategy(GameContext ctx);

        static readonly (string name, Strategy play)[] Strategies =
        {
            ("Dikkatli (ipucu + değer + değer farkında ek test, zarf yok)", ctx => { TestValueDiffs(ctx); Decide(ctx, p => Looks(p).ok && !p.OffersBribe, false); }),
            ("Saf (sadece değer, zarf yok)",        ctx => Decide(ctx, p => Looks(p).valuesOk && !p.OffersBribe, false)),
            ("Rüşvetçi (her şeyi onayla + zarf)",   ctx => { RevealBribes(ctx); Decide(ctx, p => true, true); }),
            ("Reddeci (her şeyi reddet)",           ctx => Decide(ctx, p => false, false)),
            ("Karışık (dikkatli ama zarfı alır)",   ctx => { RevealBribes(ctx); Decide(ctx, p => Looks(p).ok || p.BribeRevealed, true); }),
            ("Tek-zarf (dikkatli, sadece ilk zarfı alır)", ctx =>
            {
                RevealBribes(ctx);
                foreach (var p in ctx.TodayProjects)
                {
                    if (p.BribeRevealed && ctx.State.TotalBribesTaken == 0) ctx.Actions.Approve(p.Id, true);
                    else if (Looks(p).ok && !p.OffersBribe) ctx.Actions.Approve(p.Id, false);
                    else ctx.Actions.Reject(p.Id);
                }
            }),
        };

        /// <summary>Oyuncunun masada görebildikleri: belgedeki değerler ve iki belge arasındaki tutarsızlıklar.</summary>
        static (bool valuesOk, bool ok) Looks(ProjectData p)
        {
            bool valuesOk = p.Params.TrueForAll(x => x.ReportedPasses && x.LabValue >= x.Required);
            bool clue = p.Documents.Count > 1 && p.Documents[1].Clues.Count > 0;
            // Değer farkı (form ↔ lab beyanı): dikkatli oyuncu ek testle çözer; test yoksa riske girmez
            bool diff = p.Params.Exists(x => x.LabDiffers);
            bool diffResolved = !diff || (p.ExtraTestBought && p.Params.TrueForAll(x => x.ActualPasses));
            return (valuesOk, valuesOk && !clue && diffResolved);
        }

        /// <summary>Değer farkı görülen (başka ipucu olmayan) projelerde kasa yetiyorsa ek test alınır.</summary>
        static void TestValueDiffs(GameContext ctx)
        {
            foreach (var p in ctx.TodayProjects)
            {
                bool clue = p.Documents.Count > 1 && p.Documents[1].Clues.Count > 0;
                if (!p.ExtraTestBought && !clue && p.Params.Exists(x => x.LabDiffers) && p.Params.TrueForAll(x => x.ReportedPasses))
                    ctx.Actions.BuyExtraTest(p.Id); // para yetmezse alınamaz → proje reddedilir
            }
        }

        /// <summary>
        /// Zarf artık sadece uyuşmazlık bulununca teklif edilir (memory.md "💬"). Zarf alan oyuncu tipleri ek test almadan
        /// incelemeyle bulunabilen uyuşmazlıkları arar (doğru kat grubuyla): değer ↔ El Kitabı, künye, lab / kaşe ↔ akredite listesi.
        /// </summary>
        static void RevealBribes(GameContext ctx)
        {
            foreach (var p in ctx.TodayProjects)
            {
                if (!p.OffersBribe) continue;
                int group = p.Floors <= 3 ? 0 : p.Floors <= 6 ? 1 : 2;
                var pairs = new List<(InspectTarget, InspectTarget)>
                {
                    (InspectTarget.Of(InspectDoc.Form, InspectField.Parcel), InspectTarget.Of(InspectDoc.Lab, InspectField.Parcel)),
                    (InspectTarget.Of(InspectDoc.Form, InspectField.Date), InspectTarget.Of(InspectDoc.Lab, InspectField.Date)),
                    (InspectTarget.Of(InspectDoc.Form, InspectField.Signature), InspectTarget.Of(InspectDoc.Lab, InspectField.Signature)),
                    (InspectTarget.Of(InspectDoc.Lab, InspectField.LabName), InspectTarget.Of(InspectDoc.Handbook, InspectField.AccreditedLabs)),
                    (InspectTarget.Of(InspectDoc.Lab, InspectField.LabStamp), InspectTarget.Of(InspectDoc.Handbook, InspectField.AccreditedLabs)),
                };
                foreach (var e in p.Params)
                {
                    pairs.Add((InspectTarget.OfParam(InspectDoc.Form, e.Type), InspectTarget.OfParam(InspectDoc.Handbook, e.Type)));
                    pairs.Add((InspectTarget.OfParam(InspectDoc.Form, e.Type), InspectTarget.OfParam(InspectDoc.Lab, e.Type))); // değer farkı
                }
                foreach (var (a, b) in pairs)
                {
                    if (p.BribeRevealed) break;
                    ctx.Actions.Inspect(p.Id, a, b, group);
                }
            }
        }

        static void Decide(GameContext ctx, System.Predicate<ProjectData> approve, bool takeBribe)
        {
            foreach (var p in ctx.TodayProjects)
            {
                if (approve(p)) ctx.Actions.Approve(p.Id, takeBribe);
                else ctx.Actions.Reject(p.Id);
            }
        }

        [MenuItem("CivilJam/Core/Balance Report")]
        public static void Run()
        {
            const string data = "Assets/_Project/Data";
            var balance = AssetDatabase.LoadAssetAtPath<BalanceSO>(data + "/Balance.asset");
            var rules = AssetDatabase.LoadAssetAtPath<RuleSetSO>(data + "/RuleSet.asset");
            var names = AssetDatabase.LoadAssetAtPath<NamePoolSO>(data + "/NamePool.asset");
            var days = Load<DayConfigSO>(data + "/Days");
            var expenses = Load<ExpenseSO>(data + "/Expenses");
            var clients = Load<ClientSO>(data + "/Clients");

            var sb = new StringBuilder();
            sb.AppendLine($"DENGE RAPORU — {GamesPerStrategy} oyun/tip · {balance.TotalDays} gün · başlangıç {balance.StartMoney} TL · borç limiti {balance.DebtLimit} TL");
            sb.AppendLine($"Afet: taban {balance.DisasterChanceStart:P0}, +{balance.DisasterChanceIncrement:P0}/gün, tavan {balance.DisasterChanceCap:P0} · onay ücreti {balance.ApprovalFeeRange.x}-{balance.ApprovalFeeRange.y}");
            sb.AppendLine();

            foreach (var (name, play) in Strategies)
            {
                var endings = new Dictionary<EndingType, int>();
                int sumDay = 0, sumMoney = 0, sumDebt = 0, sumQuakes = 0, sumCollapses = 0, sumBribes = 0, sumApproved = 0, minMoney = int.MaxValue;

                for (int g = 0; g < GamesPerStrategy; g++)
                {
                    var ctx = new GameContext { Balance = balance, Rules = rules, Names = names, Clients = clients };
                    var session = new GameSession(ctx, days, expenses);
                    session.NewGame(1000 + g);

                    var ending = EndingType.None;
                    int quakes = 0;
                    while (ending == EndingType.None)
                    {
                        session.BeginDay();
                        play(ctx);
                        session.FinishDesk();
                        if (session.RunDisasterCheck().Occurred) quakes++;
                        session.PrepareExpenses();
                        session.FinishExpenses();
                        minMoney = Mathf.Min(minMoney, ctx.State.Money);
                        ending = session.EvaluateEnding();
                        if (ending == EndingType.None) ctx.State.Day++;
                    }

                    endings[ending] = endings.TryGetValue(ending, out var c) ? c + 1 : 1;
                    var s = ctx.State;
                    sumDay += s.Day; sumMoney += s.Money; sumDebt += s.Debt; sumQuakes += quakes;
                    sumCollapses += s.TotalCollapses; sumBribes += s.TotalBribesTaken; sumApproved += s.TotalApproved;
                }

                float n = GamesPerStrategy;
                sb.AppendLine($"■ {name}");
                sb.AppendLine("  Sonlar: " + string.Join(" · ",
                    new[] { EndingType.Good, EndingType.MiddleCollapsed, EndingType.MiddleClosed, EndingType.Bad }
                        .Select(e => $"{e} %{100f * (endings.TryGetValue(e, out var k) ? k : 0) / n:0}")));
                sb.AppendLine($"  Ort. bitiş günü {sumDay / n:0.0} · deprem {sumQuakes / n:0.0} · onay {sumApproved / n:0.0} · yıkım {sumCollapses / n:0.0} · zarf {sumBribes / n:0.0}");
                sb.AppendLine($"  Ort. son kasa {sumMoney / n:0} TL · borç {sumDebt / n:0} TL · görülen en düşük kasa {minMoney} TL");
                sb.AppendLine();
            }

            string report = sb.ToString();
            Debug.Log("[Balance]\n" + report);
            System.IO.Directory.CreateDirectory("Logs");
            System.IO.File.WriteAllText("Logs/balance_report.txt", report);
        }

        /// <summary>Sayı alternatiflerini karşılaştırır (asset'lere dokunmaz; Balance kopyası üzerinde çalışır).</summary>
        [MenuItem("CivilJam/Core/Balance Scenarios")]
        public static void RunScenarios()
        {
            const string data = "Assets/_Project/Data";
            var baseBalance = AssetDatabase.LoadAssetAtPath<BalanceSO>(data + "/Balance.asset");
            var rules = AssetDatabase.LoadAssetAtPath<RuleSetSO>(data + "/RuleSet.asset");
            var names = AssetDatabase.LoadAssetAtPath<NamePoolSO>(data + "/NamePool.asset");
            var days = Load<DayConfigSO>(data + "/Days");
            var expensesBase = Load<ExpenseSO>(data + "/Expenses");
            var clients = Load<ClientSO>(data + "/Clients");

            var scenarios = new (string label, System.Action<BalanceSO> tweak, int rentExtra)[]
            {
                ("A Şimdiki", b => { }, 0),
                ("B Ücret 300-500", b => b.ApprovalFeeRange = new Vector2Int(300, 500), 0),
                ("C Deprem %20 +%15", b => { b.DisasterChanceStart = 0.2f; b.DisasterChanceIncrement = 0.15f; }, 0),
                ("D B + C", b => { b.ApprovalFeeRange = new Vector2Int(300, 500); b.DisasterChanceStart = 0.2f; b.DisasterChanceIncrement = 0.15f; }, 0),
                ("E D + kira +200", b => { b.ApprovalFeeRange = new Vector2Int(300, 500); b.DisasterChanceStart = 0.2f; b.DisasterChanceIncrement = 0.15f; }, 200),
                ("F D + başlangıç 1.500", b => { b.ApprovalFeeRange = new Vector2Int(300, 500); b.DisasterChanceStart = 0.2f; b.DisasterChanceIncrement = 0.15f; b.StartMoney = 1500; }, 0),
            };

            var sb = new StringBuilder();
            sb.AppendLine("SENARYO KARŞILAŞTIRMASI (her satır: İyi/Yıkım/Kapandı/Hapis % · son kasa · en düşük kasa · deprem)");
            sb.AppendLine();
            foreach (var (label, tweak, rentExtra) in scenarios)
            {
                var b = Object.Instantiate(baseBalance);
                tweak(b);
                var expenses = expensesBase.Select(e =>
                {
                    var c = Object.Instantiate(e);
                    if (c.Id == "rent") c.Amount += rentExtra;
                    return c;
                }).ToList();

                sb.AppendLine($"[{label}]");
                foreach (var (name, play) in Strategies)
                {
                    var endings = new Dictionary<EndingType, int>();
                    int sumMoney = 0, sumQuakes = 0, minMoney = int.MaxValue;
                    for (int g = 0; g < GamesPerStrategy; g++)
                    {
                        var ctx = new GameContext { Balance = b, Rules = rules, Names = names, Clients = clients };
                        var session = new GameSession(ctx, days, expenses);
                        session.NewGame(1000 + g);
                        var ending = EndingType.None;
                        while (ending == EndingType.None)
                        {
                            session.BeginDay();
                            play(ctx);
                            session.FinishDesk();
                            if (session.RunDisasterCheck().Occurred) sumQuakes++;
                            session.PrepareExpenses();
                            session.FinishExpenses();
                            minMoney = Mathf.Min(minMoney, ctx.State.Money);
                            ending = session.EvaluateEnding();
                            if (ending == EndingType.None) ctx.State.Day++;
                        }
                        endings[ending] = endings.TryGetValue(ending, out var k) ? k + 1 : 1;
                        sumMoney += ctx.State.Money;
                    }
                    float n = GamesPerStrategy;
                    string pct(EndingType e) => $"{100f * (endings.TryGetValue(e, out var k) ? k : 0) / n:0}";
                    sb.AppendLine($"  {name.Split(' ')[0],-9} {pct(EndingType.Good),3}/{pct(EndingType.MiddleCollapsed),3}/{pct(EndingType.MiddleClosed),3}/{pct(EndingType.Bad),3}  kasa {sumMoney / n,6:0}  min {minMoney,5}  deprem {sumQuakes / n:0.0}");
                }
                sb.AppendLine();
            }
            Debug.Log("[Balance] " + sb);
            System.IO.File.WriteAllText("Logs/balance_scenarios.txt", sb.ToString());
        }

        /// <summary>İçerik istatistiği: proje türleri gün gün, isim tekrarları.</summary>
        [MenuItem("CivilJam/Core/Content Stats")]
        public static void ContentStats()
        {
            const string data = "Assets/_Project/Data";
            var balance = AssetDatabase.LoadAssetAtPath<BalanceSO>(data + "/Balance.asset");
            var rules = AssetDatabase.LoadAssetAtPath<RuleSetSO>(data + "/RuleSet.asset");
            var names = AssetDatabase.LoadAssetAtPath<NamePoolSO>(data + "/NamePool.asset");
            var days = Load<DayConfigSO>(data + "/Days");
            var expenses = Load<ExpenseSO>(data + "/Expenses");
            var clients = Load<ClientSO>(data + "/Clients");
            const int games = 300;
            var byDay = new int[balance.TotalDays + 1, 4]; // 0 uygun, 1 düz uygunsuz, 2 rüşvetli, 3 sahte belgeli
            int sumProjects = 0, sumDupNames = 0;
            var clientCount = new Dictionary<string, int>();
            for (int g = 0; g < games; g++)
            {
                var ctx = new GameContext { Balance = balance, Rules = rules, Names = names, Clients = clients };
                var session = new GameSession(ctx, days, expenses);
                session.NewGame(5000 + g);
                var seenNames = new HashSet<string>(); var seenClients = new HashSet<string>();
                for (int d = 1; d <= balance.TotalDays; d++)
                {
                    ctx.State.Day = d;
                    session.BeginDay();
                    foreach (var p in ctx.TodayProjects)
                    {
                        int kind = p.Kind == ProjectKind.Compliant ? 0 : p.Kind == ProjectKind.Sloppy ? 1 : p.Kind == ProjectKind.Fake ? 3 : 2;
                        byDay[d, kind]++;
                        sumProjects++;
                        if (!seenNames.Add(p.Name)) sumDupNames++;
                        seenClients.Add(p.ClientId ?? "-");
                        clientCount[p.ClientId ?? "-"] = clientCount.TryGetValue(p.ClientId ?? "-", out var c) ? c + 1 : 1;
                    }
                    foreach (var p in ctx.TodayProjects) ctx.Actions.Reject(p.Id);
                }
            }
            var sb = new StringBuilder();
            sb.AppendLine($"İÇERİK İSTATİSTİĞİ ({games} oyun, ortalama / oyun)");
            sb.AppendLine("Gün | uygun | dikkatsiz | rüşvetli (+sahte) | sahte belgeli");
            for (int d = 1; d <= balance.TotalDays; d++)
                sb.AppendLine($"{d,3} | {byDay[d,0]/(float)games,5:0.0} | {byDay[d,1]/(float)games,12:0.0} | {byDay[d,2]/(float)games,8:0.0} | {byDay[d,3]/(float)games,13:0.0}");
            sb.AppendLine($"Proje/oyun {sumProjects/(float)games:0.0} · tekrar eden bina adı/oyun {sumDupNames/(float)games:0.0}");
            sb.AppendLine("Müteahhit payı: " + string.Join(", ", clientCount.OrderByDescending(k => k.Value).Select(k => $"{k.Key} %{100f*k.Value/sumProjects:0}")));
            System.IO.Directory.CreateDirectory("Logs");
            System.IO.File.WriteAllText("Logs/content_stats.txt", sb.ToString());
        }

        static List<T> Load<T>(string dir) where T : Object =>
            AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { dir })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).OrderBy(a => a.name).ToList();
    }
}
